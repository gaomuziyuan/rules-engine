using MQTTnet.Client;

namespace RulesEngine.Services;

public interface IMqttFactory
{
    IMqttClient CreateMqttClient();
}