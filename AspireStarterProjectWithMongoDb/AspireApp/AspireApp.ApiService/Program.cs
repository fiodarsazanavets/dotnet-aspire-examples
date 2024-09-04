using AspireApp.ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddMongoDBClient("mongodb");

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();

    var database = mongoClient.GetDatabase("WeatherDB");
    var collection = database.GetCollection<WeatherForecast>("WeatherForecasts");

    // Check if data exists
    var count = collection.CountDocuments(new BsonDocument());

    if (count == 0)
    {
        var weatherForecasts = new List<WeatherForecast>();

        foreach (var index in Enumerable.Range(1, 5))
        {
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
            var temperatureC = Random.Shared.Next(-20, 55);
            var summary = summaries[Random.Shared.Next(summaries.Length)];

            weatherForecasts.Add(new WeatherForecast
            {
                Id = Guid.NewGuid().ToString(),
                Date = date,
                TemperatureC = temperatureC,
                Summary = summary
            });
        }

        collection.InsertMany(weatherForecasts);
    }
}

app.MapGet("/weatherforecast", ([FromServices] IMongoClient mongoClient) =>
{
    var database = mongoClient.GetDatabase("WeatherDB");
    var collection = database.GetCollection<WeatherForecast>("WeatherForecasts");

    var weatherForecasts = collection.Find(new BsonDocument()).ToList();

    return weatherForecasts.ToArray();
});

app.MapDefaultEndpoints();

app.Run();
