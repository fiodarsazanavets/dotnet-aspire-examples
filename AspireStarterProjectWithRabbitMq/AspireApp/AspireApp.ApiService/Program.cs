using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);

await Task.Delay(10000);

builder.AddRabbitMQClient("rabbitmq");

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
    var connection = scope.ServiceProvider.GetRequiredService<IConnection>();

    using IModel channel = connection.CreateModel();
    {
        // Declare a queue (if the queue doesn't exist, it will be created)
        channel.QueueDeclare(queue: "weather",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        for (int i = 1; i < 6; i++)
        {
            var message = new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // Publish the message to the queue
            channel.BasicPublish(exchange: "",
                                 routingKey: "weather",
                                 basicProperties: null,
                                 body: body);
        }
    }
}

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
