using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using RulesEngine.Models;
using RulesEngine.Services;

namespace RulesEngine.Tests;

public class MqttServiceTests
{
    private readonly Mock<ILogger<MqttService>> _mockLogger;
    private readonly Mock<IRulesEngineService> _mockRulesEngineService;
    private readonly Mock<IMqttClient> _mockMqttClient;
    private readonly Mock<MqttFactory> _mockMqttFactory;
    private readonly MqttService _mqttService;

    public MqttServiceTests()
    {
        _mockLogger = new Mock<ILogger<MqttService>>();
        _mockRulesEngineService = new Mock<IRulesEngineService>();
        _mockMqttClient = new Mock<IMqttClient>();
        _mockMqttFactory = new Mock<MqttFactory>();

        _mockMqttFactory
            .Setup(f => f.CreateMqttClient())
            .Returns(_mockMqttClient.Object);

        _mqttService = new MqttService(
            _mockLogger.Object,
            _mockRulesEngineService.Object,
            _mockMqttFactory.Object
        );
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToMqttBroker()
    {
        // Arrange
        const string topicId = "test-topic";

        // Act
        await _mqttService.StartAsync(topicId);

        // Assert
        _mockMqttClient.Verify(
            x => x.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task StopAsync_ShouldDisconnectFromMqttBroker()
    {
        // Arrange
        const string topicId = "stop-topic";
        _mockMqttClient.Setup(x => x.IsConnected).Returns(true);
        _mockMqttClient
            .Setup(x => x.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mqttService.StopAsync(topicId);

        // Assert
        _mockMqttClient.Verify(
            x => x.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }


    [Fact]
    public async Task StartAsync_ShouldTriggerMessageReceivedHandler()
    {
        // Arrange
        var inputData = new InputData
        {
            Id = "test-id",
            NumOfChildren = 2,
            Composition = "Standard",
            InPayForDec = true
        };

        var expectedResult = new OutputData
        {
            Id = "test-id",
            IsEligible = true,
            BaseAmount = 100.0f,
            ChildrenAmount = 50.0f,
            SupplementAmount = 150.0f
        };

        var messageEventArgs = CreateMockMessageReceivedEventArgs(inputData, "test-topic");

        _mockRulesEngineService
            .Setup(x => x.CalculateSupplement(It.IsAny<InputData>()))
            .Returns(expectedResult);

        _mockMqttClient
            .Setup(x => x.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(
                packetIdentifier: 1,
                reasonCode: MqttClientPublishReasonCode.Success,
                reasonString: "Success",
                userProperties: Array.Empty<MqttUserProperty>()
            ));

        // Act
        await _mqttService.StartAsync("test-topic");
        _mockMqttClient.Raise(
            m => m.ApplicationMessageReceivedAsync += null,
            messageEventArgs
        );

        // Assert
        _mockRulesEngineService.Verify(
            x => x.CalculateSupplement(It.IsAny<InputData>()),
            Times.Never
        );

        _mockMqttClient.Verify(
            x => x.PublishAsync(It.IsAny<MqttApplicationMessage>(), default),
            Times.Never
        );

        _mockMqttClient.Verify(
            x => x.PublishAsync(
                It.Is<MqttApplicationMessage>(
                    m => m.Topic.Contains("BRE/calculateWinterSupplementOutput/test-topic")
                ),
                default
            ),
            Times.Once
        );

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Failed to deserialize input data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    private MqttApplicationMessageReceivedEventArgs CreateMockMessageReceivedEventArgs(
        InputData inputData,
        string topicId)
    {
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(inputData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }));

        var applicationMessage = new MqttApplicationMessage
        {
            Topic = $"BRE/calculateWinterSupplementInput/{topicId}",
            PayloadSegment = new ArraySegment<byte>(payload)
        };
        var publishPacket = new MqttPublishPacket
        {
            Topic = applicationMessage.Topic
        };
        Func<MqttApplicationMessageReceivedEventArgs, CancellationToken, Task> acknowledgeHandler =
            async (args, cancellationToken) => { await Task.CompletedTask; };
        return new MqttApplicationMessageReceivedEventArgs(
            clientId: "test-client",
            applicationMessage: applicationMessage,
            publishPacket: publishPacket,
            acknowledgeHandler: acknowledgeHandler
        );
    }
}