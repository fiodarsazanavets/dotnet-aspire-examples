using AspireApp.ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddAzureCosmosClient("cosmos");

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();

    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("WeatherDB");
    var container = await database.Database.CreateContainerIfNotExistsAsync("WeatherForecasts", "/id");

    // Check if data exists
    var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
    var iterator = container.Container.GetItemQueryIterator<int>(query);
    var count = (await iterator.ReadNextAsync()).FirstOrDefault();

    if (count == 0)
    {
        var weatherForecasts = new List<WeatherForecast>();

        foreach (var index in Enumerable.Range(1, 5))
        {
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
            var temperatureC = Random.Shared.Next(-20, 55);
            var summary = summaries[Random.Shared.Next(summaries.Length)];

            var weatherForecast = new WeatherForecast
            {
                Id = Guid.NewGuid().ToString(), // Ensure a unique ID
                Date = date,
                TemperatureC = temperatureC,
                Summary = summary
            };

            weatherForecasts.Add(weatherForecast);
        }

        foreach (var weatherForecast in weatherForecasts)
        {
            await container.Container.CreateItemAsync(weatherForecast, new PartitionKey(weatherForecast.Id));
        }
    }
}

app.MapGet("/weatherforecast", async ([FromServices] CosmosClient cosmosClient) =>
{
    var database = cosmosClient.GetDatabase("WeatherDB");
    var container = database.GetContainer("WeatherForecasts");

    var weatherForecasts = new List<WeatherForecast>();

    var query = new QueryDefinition("SELECT * FROM c");
    var iterator = container.GetItemQueryIterator<WeatherForecast>(query);

    while (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();
        weatherForecasts.AddRange(response);
    }

    return weatherForecasts.ToArray();
});

app.MapDefaultEndpoints();

app.Run();
