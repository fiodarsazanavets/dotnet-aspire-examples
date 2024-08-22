using AspireApp.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
}
