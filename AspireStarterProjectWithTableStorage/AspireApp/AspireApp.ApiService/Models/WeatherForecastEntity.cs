using Azure.Data.Tables;
using Azure;

namespace AspireApp.ApiService.Models;

public class WeatherForecastEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int TemperatureC { get; set; }
    public string Summary { get; set; } = string.Empty;

    public ETag ETag { get; set; } = ETag.All;
    public DateTimeOffset? Timestamp { get; set; }
}
