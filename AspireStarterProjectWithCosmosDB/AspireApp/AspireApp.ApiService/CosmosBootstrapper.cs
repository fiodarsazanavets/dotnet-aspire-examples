using AspireApp.ApiService.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;

namespace AspireApp.ApiService;

public class CosmosBootstrapper(CosmosClient cosmosClient, ILogger<CosmosBootstrapper> logger) : BackgroundService, IHealthCheck
{
    private bool _dbCreated;
    private bool _dbCreationFailed;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var status = _dbCreated
            ? HealthCheckResult.Healthy()
            : _dbCreationFailed
                ? HealthCheckResult.Unhealthy("Database creation failed.")
                : HealthCheckResult.Degraded("Database creation is still in progress.");
        return Task.FromResult(status);
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = 60,
                BackoffType = DelayBackoffType.Constant,
                OnRetry = args =>
                {
                    logger.LogWarning("""
                        Issue during database creation after {AttemptDuration} on attempt {AttemptNumber}. Will retry in {RetryDelay}.
                        Exception:
                            {ExceptionMessage}
                            {InnerExceptionMessage}
                        """,
                        args.Duration,
                        args.AttemptNumber,
                        args.RetryDelay,
                        args.Outcome.Exception?.Message ?? "[none]",
                        args.Outcome.Exception?.InnerException?.Message ?? string.Empty);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
        await retry.ExecuteAsync(async ct =>
        {
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("weather");
            var container = await database.Database.CreateContainerIfNotExistsAsync("forecasts", "/id");

            // Check if data exists
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
            var iterator = container.Container.GetItemQueryIterator<int>(query);
            var count = (await iterator.ReadNextAsync()).FirstOrDefault();

            if (count == 0)
            {
                var summaries = new[]
                {
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
                };

                var weatherForecasts = new List<WeatherForecast>();

                foreach (var index in Enumerable.Range(1, 5))
                {
                    var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
                    var temperatureC = Random.Shared.Next(-20, 55);
                    var summary = summaries[Random.Shared.Next(summaries.Length)];

                    var weatherForecast = new WeatherForecast
                    {
                        id = Guid.NewGuid().ToString(), // Ensure a unique ID
                        Date = date,
                        TemperatureC = temperatureC,
                        Summary = summary
                    };

                    weatherForecasts.Add(weatherForecast);
                }

                foreach (var weatherForecast in weatherForecasts)
                {
                    await container.Container.CreateItemAsync(weatherForecast, new PartitionKey(weatherForecast.id));
                }
            }
            _dbCreated = true;
        }, cancellationToken);

        _dbCreationFailed = !_dbCreated;
    }
}
