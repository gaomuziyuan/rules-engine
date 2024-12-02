namespace RulesEngine.Services;

public interface IMqttService
{
    Task StartAsync(string topicId);
    Task StopAsync(string topicId);
}