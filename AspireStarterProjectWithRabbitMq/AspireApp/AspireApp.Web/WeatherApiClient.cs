using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AspireApp.Web;

public class WeatherApiClient(IConnection connection) : BackgroundService
{
    private readonly List<WeatherForecast> forecasts = new List<WeatherForecast>();

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using IModel channel = connection.CreateModel();
        
        // Declare the queue (must match the publisher's queue)
        channel.QueueDeclare(queue: "weather",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            // Create a consumer
            var consumer = new EventingBasicConsumer(channel);

            // Set up a callback to handle received messages
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");

                forecasts.Add(JsonSerializer.Deserialize<WeatherForecast>(message));
            };

            // Start consuming messages from the queue
            channel.BasicConsume(queue: "weather",
                                 autoAck: true,
                                 consumer: consumer);
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
