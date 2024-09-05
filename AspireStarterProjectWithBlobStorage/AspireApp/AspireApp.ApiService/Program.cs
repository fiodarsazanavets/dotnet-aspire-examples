using AspireApp.ApiService.Models;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddAzureBlobClient("blobs");

var app = builder.Build();

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

using (var scope = app.Services.CreateScope())
{
    var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();

    var containerClient = blobServiceClient.GetBlobContainerClient("weather");

    // Create the container if it doesn't exist
    await containerClient.CreateIfNotExistsAsync();

    var blobClient = containerClient.GetBlobClient("weather-forecasts.csv");

    // Check if the blob already exists
    if (await blobClient.ExistsAsync())
    {
        return;
    }

    var weatherForecasts = new List<WeatherForecast>();

    foreach (var index in Enumerable.Range(1, 5))
    {
        var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
        var temperatureC = Random.Shared.Next(-20, 55);
        var summary = summaries[Random.Shared.Next(summaries.Length)];

        weatherForecasts.Add(new WeatherForecast
        {
            Date = date,
            TemperatureC = temperatureC,
            Summary = summary
        });
    }

    // Write the data to a CSV file in memory
    using (var memoryStream = new MemoryStream())
    using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
    using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
    {
        csv.WriteRecords(weatherForecasts);
        writer.Flush();

        // Upload the CSV to the blob
        memoryStream.Position = 0;
        await blobClient.UploadAsync(memoryStream, overwrite: true);
    }
}

app.MapGet("/weatherforecast", async (BlobServiceClient blobServiceClient) =>
{
    var containerClient = blobServiceClient.GetBlobContainerClient("weather");
    var blobClient = containerClient.GetBlobClient("weather-forecasts.csv");

    if (!await blobClient.ExistsAsync())
    {
        return new List<WeatherForecast>().ToArray();
    }

    var weatherForecasts = new List<WeatherForecast>();

    // Download the CSV from the blob
    var downloadResponse = await blobClient.DownloadAsync();
    using (var stream = downloadResponse.Value.Content)
    using (var reader = new StreamReader(stream))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        weatherForecasts = csv.GetRecords<WeatherForecast>().ToList();
    }

    return weatherForecasts.ToArray();
});

app.MapDefaultEndpoints();

app.Run();
