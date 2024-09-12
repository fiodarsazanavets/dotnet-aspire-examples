using AspireApp.ApiService;
using AspireApp.ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.AddAzureCosmosClient("cosmos");
builder.Services.AddSingleton<CosmosBootstrapper>();
builder.Services.AddHealthChecks()
    .Add(new("cosmos", sp => sp.GetRequiredService<CosmosBootstrapper>(), null, null));
builder.Services.AddHostedService(sp => sp.GetRequiredService<CosmosBootstrapper>());

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/weatherforecast", async ([FromServices] CosmosClient cosmosClient) =>
{
    var weatherForecasts = new List<WeatherForecast>();

    try
    {
        var database = cosmosClient.GetDatabase("weather");
        var container = database.GetContainer("forecasts");



        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<WeatherForecast>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            weatherForecasts.AddRange(response);
        }
    }
    catch { }

    return weatherForecasts.ToArray();
});

app.MapDefaultEndpoints();

app.Run();
