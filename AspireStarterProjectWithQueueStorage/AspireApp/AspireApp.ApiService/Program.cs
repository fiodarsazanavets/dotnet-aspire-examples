using Azure.Storage.Queues;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);

builder.AddAzureQueueClient("queues");

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var queueServiceClient = scope.ServiceProvider.GetRequiredService<QueueServiceClient>();
    var queueClient = queueServiceClient.GetQueueClient("weather-forecasts");
    queueClient.CreateIfNotExists();

    for (int i = 1; i < 6; i++)
    {
        var message = new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );

        queueClient.SendMessage(JsonSerializer.Serialize(message));
    }
}

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
