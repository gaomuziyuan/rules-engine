using Microsoft.AspNetCore.Mvc;
using RulesEngine.Services;

namespace RulesEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MqttController : ControllerBase
{
    private readonly IMqttService _mqttService;

    public MqttController(IMqttService mqttService)
    {
        _mqttService = mqttService;
    }

    [HttpGet("start/{topicId}")]
    public async Task<IActionResult> StartMqtt(string topicId)
    {
        await _mqttService.StartAsync(topicId);
        return Ok($"MQTT service started for topic: {topicId}");
    }
}