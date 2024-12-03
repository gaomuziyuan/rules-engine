using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using RulesEngine.Models;

namespace RulesEngine.Services;

public class MqttService : IMqttService
{
    private readonly ILogger<MqttService> _logger;
    private readonly IRulesEngineService _rulesEngineService;
    private readonly IMqttClient _mqttClient;
    private const string Broker = "test.mosquitto.org";
    private const int Port = 1883;

    public MqttService(ILogger<MqttService> logger, IRulesEngineService rulesEngineService, IMqttFactory mqttFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rulesEngineService = rulesEngineService ?? throw new ArgumentNullException(nameof(rulesEngineService));
        _mqttClient = mqttFactory?.CreateMqttClient() ?? throw new ArgumentNullException(nameof(mqttFactory));
        _mqttClient.ConnectedAsync += HandleConnectedAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
    }

    public async Task StartAsync(string topicId)
    {
        try
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(Broker, Port)
                .WithClientId(Guid.NewGuid().ToString())
                .WithCleanSession(true)
                .Build();

            await _mqttClient.ConnectAsync(options);
            _logger.LogInformation($"MQTT client connected to broker at {Broker}:{Port}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker");
            throw;
        }
    }


    private async Task HandleConnectedAsync(MqttClientConnectedEventArgs e)
    {
        try
        {
            var topic = "BRE/calculateWinterSupplementInput/#";
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);
            _logger.LogInformation($"Subscribed to topic: {topic}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while subscribing to topic");
        }
    }

    private Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogWarning("Disconnected from MQTT broker");
        return Task.CompletedTask;
    }

    private async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            _logger.LogInformation($"Message received: {payload}");

            var inputData = JsonSerializer.Deserialize<InputData>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (inputData == null)
            {
                _logger.LogWarning("Failed to deserialize input data");
                return;
            }

            var result = _rulesEngineService.CalculateSupplement(inputData);

            var outputTopic = $"BRE/calculateWinterSupplementOutput/{e.ApplicationMessage.Topic.Split('/').Last()}";
            var outputPayload = JsonSerializer.Serialize(result);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(outputTopic)
                .WithPayload(Encoding.UTF8.GetBytes(outputPayload))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(message);
            _logger.LogInformation($"Published result to topic: {outputTopic}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing message");
        }
    }

    public async Task StopAsync(string topicId)
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation($"Disconnected from MQTT broker and stopped listening to topic: {topicId}");
        }
    }
}