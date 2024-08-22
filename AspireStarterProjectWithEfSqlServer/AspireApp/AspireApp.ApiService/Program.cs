using AspireApp.ApiService;
using AspireApp.ApiService.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.AddSqlServerDbContext<WeatherDbContext>("sqldb");

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    context.Database.EnsureCreated();

    if (!context.WeatherForecasts.Any())
    {
        foreach (var index in Enumerable.Range(1, 5))
        {
            context.WeatherForecasts.Add(new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            });

            context.SaveChanges();
        }
    }
}

app.MapGet("/weatherforecast", ([FromServices] WeatherDbContext context) =>
{
    return context.WeatherForecasts.ToArray();
});

app.MapDefaultEndpoints();

app.Run();
