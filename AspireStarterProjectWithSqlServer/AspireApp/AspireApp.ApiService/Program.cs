using AspireApp.ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.AddSqlServerClient("sqldb");

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var connection = scope.ServiceProvider.GetRequiredService<SqlConnection>();
    connection.Open();

    // Ensure the table exists
    var createTableCommand = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WeatherForecasts' and xtype='U')
            CREATE TABLE WeatherForecasts (
                Id INT PRIMARY KEY IDENTITY,
                Date DATE NOT NULL,
                TemperatureC INT NOT NULL,
                Summary NVARCHAR(100) NOT NULL
            )", connection);

    createTableCommand.ExecuteNonQuery();

    // Check if data exists
    var checkDataCommand = new SqlCommand("SELECT COUNT(*) FROM WeatherForecasts", connection);
    var count = (int)checkDataCommand.ExecuteScalar();

    if (count == 0)
    {
        foreach (var index in Enumerable.Range(1, 5))
        {
            var date = DateTime.Now.AddDays(index).Date;
            var temperatureC = Random.Shared.Next(-20, 55);
            var summary = summaries[Random.Shared.Next(summaries.Length)];

            var insertCommand = new SqlCommand(@"
                    INSERT INTO WeatherForecasts (Date, TemperatureC, Summary)
                    VALUES (@Date, @TemperatureC, @Summary)", connection);

            insertCommand.Parameters.AddWithValue("@Date", date);
            insertCommand.Parameters.AddWithValue("@TemperatureC", temperatureC);
            insertCommand.Parameters.AddWithValue("@Summary", summary);

            insertCommand.ExecuteNonQuery();
        }
    }
}

app.MapGet("/weatherforecast", ([FromServices] SqlConnection connection) =>
{
    connection.Open();

    var command = new SqlCommand("SELECT Date, TemperatureC, Summary FROM WeatherForecasts", connection);
    var weatherForecasts = new List<WeatherForecast>();

    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            weatherForecasts.Add(new WeatherForecast
            {
                Date = DateOnly.FromDateTime(reader.GetDateTime(0)),
                TemperatureC = reader.GetInt32(1),
                Summary = reader.GetString(2)
            });
        }

        return weatherForecasts.ToArray();
    }
});

app.MapDefaultEndpoints();

app.Run();
