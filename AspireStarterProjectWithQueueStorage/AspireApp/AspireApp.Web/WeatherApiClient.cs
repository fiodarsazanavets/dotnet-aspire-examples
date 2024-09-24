using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;

namespace AspireApp.Web;

public class WeatherApiClient(QueueServiceClient queueServiceClient) : BackgroundService
{
    private readonly List<WeatherForecast> forecasts = new List<WeatherForecast>();

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var queueClient = queueServiceClient.GetQueueClient("weather-forecasts");
        await queueClient.CreateIfNotExistsAsync();

        QueueMessage[] messages = queueClient.ReceiveMessages(maxMessages: 10);

        foreach (QueueMessage message in messages)
        {
            forecasts.Add(JsonSerializer.Deserialize<WeatherForecast>(message.MessageText));
        }
    }

    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        return forecasts.Take(maxItems).ToArray();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
