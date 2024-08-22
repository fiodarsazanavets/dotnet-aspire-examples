using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspireApp.ApiService.Models;

public class WeatherForecast
{
    [Key]
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

    [NotMapped]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
