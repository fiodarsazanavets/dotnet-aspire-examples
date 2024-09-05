using AspireApp.ApiService.Models;
using Azure.Data.Tables;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddAzureTableClient("tables");

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var tableServiceClient = scope.ServiceProvider.GetRequiredService<TableServiceClient>();

    var tableClient = tableServiceClient.GetTableClient("weather");

    // Ensure the table exists
    await tableClient.CreateIfNotExistsAsync();

    // Check if data exists by querying the first entity
    var queryResult = tableClient.QueryAsync<WeatherForecastEntity>(filter: $"PartitionKey eq 'weather'");
    bool hasData = queryResult.ToBlockingEnumerable().Any();

    if (!hasData)
    {
        var weatherForecasts = new List<WeatherForecastEntity>();

        foreach (var index in Enumerable.Range(1, 5))
        {
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
            var temperatureC = Random.Shared.Next(-20, 55);
            var summary = summaries[Random.Shared.Next(summaries.Length)];

            var weatherForecast = new WeatherForecastEntity
            {
                PartitionKey = "weather",   // PartitionKey (can be "weather" or another identifier)
                RowKey = Guid.NewGuid().ToString(), // Unique RowKey for each entity
                Date = date.ToString(),
                TemperatureC = temperatureC,
                Summary = summary
            };

            weatherForecasts.Add(weatherForecast);
        }

        foreach (var weatherForecast in weatherForecasts)
        {
            await tableClient.AddEntityAsync(weatherForecast);
        }
    }
}

app.MapGet("/weatherforecast", async (TableServiceClient tableServiceClient) =>
{
    var tableClient = tableServiceClient.GetTableClient("weather");

    var weatherForecasts = new List<WeatherForecastEntity>();

    // Query the table for all entities
    var entities = tableClient.QueryAsync<WeatherForecastEntity>();

    await foreach (var entity in entities)
    {
        weatherForecasts.Add(entity);
    }

    return weatherForecasts.ToArray();
});

app.MapDefaultEndpoints();

app.Run();
