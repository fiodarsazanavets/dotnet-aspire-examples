using Microsoft.EntityFrameworkCore;
using OnlineStore.ServiceDefaults.Models;

namespace OnlineStore.ApiService;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}
