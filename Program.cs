using MQTTnet;
using RulesEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add services to the container
builder.Services.AddScoped<IRulesEngineService, RulesEngineService>();
builder.Services.AddSingleton<IMqttFactory, MqttFactoryWrapper>();
builder.Services.AddScoped<IMqttService, MqttService>();
builder.Services.AddSingleton<MqttFactory>();

// Add logging to the container
builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Information);
});


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();