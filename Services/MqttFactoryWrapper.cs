using MQTTnet;
using MQTTnet.Client;

namespace RulesEngine.Services;

public class MqttFactoryWrapper : IMqttFactory
{
    public IMqttClient CreateMqttClient()
    {
        return new MqttFactory().CreateMqttClient();
    }
}
