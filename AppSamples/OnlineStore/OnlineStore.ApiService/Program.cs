using Microsoft.Extensions.Caching.Distributed;
using OnlineStore.ApiService;
using OnlineStore.ServiceDefaults.Models;
using System.Text.Json;

const string CacheKey = "cart";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.AddSqlServerDbContext<ProductsDbContext>("sqldb");
builder.AddRedisDistributedCache(connectionName: "cache");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    context.Database.EnsureCreated();

    if (!context.Products.Any())
    {
        var products = new List<Product>
        {
            new("Laptop", 999.99m, "High-performance laptop"),
            new("Smartphone", 499.99m, "Latest model smartphone"),
            new("Headphones", 149.99m, "Noise-cancelling headphones"),
            new("Monitor", 249.99m, "4k Monitor"),
            new("Desktop", 1299.99m, "Gaming desktop PC"),
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}

app.UseExceptionHandler();

app.MapGet("api/products", (ProductsDbContext context) =>
    context.Products.ToList());

app.MapGet("api/products/{productId}", (int productId, ProductsDbContext context) =>
{
    Product? product = context.Products.FirstOrDefault(p => p.Id == productId);

    if (product is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(product);
});

app.MapPost("api/cart", async (CartItem cartItem, IDistributedCache cache) =>
{  
    List<CartItem> cachedItems = await GetCachedItems(cache);

    int productId = cartItem.ProductId;
    int quantity = 1;

    if (cachedItems.Any(i => i.ProductId == productId))
    {
        quantity = cachedItems.First(i => i.ProductId == productId).Quantity;
        quantity++;
        cachedItems.RemoveAll(c => c.ProductId == productId);
    }
    
    cachedItems.Add(new CartItem(productId, quantity));

    await UpdateCache(cachedItems, cache);

    return Results.Ok(cachedItems);
});

app.MapGet("api/cart", async (IDistributedCache cache) =>
{
    List<CartItem> cachedItems = await GetCachedItems(cache);

    return Results.Ok(cachedItems);
});

app.MapDelete("api/cart/{productId}", async (int productId, IDistributedCache cache) =>
{
    List<CartItem> cachedItems = await GetCachedItems(cache);

    cachedItems.RemoveAll(c => c.ProductId == productId);

    await UpdateCache(cachedItems, cache);

    return Results.Ok(cachedItems);
});

async static Task<List<CartItem>> GetCachedItems(IDistributedCache cache)
{
    string? cachedData = await cache.GetStringAsync(CacheKey);

    List<CartItem> cachedItems = string.IsNullOrWhiteSpace(cachedData) ?
        [] : JsonSerializer.Deserialize<List<CartItem>>(cachedData)!;

    return cachedItems;
}

async static Task UpdateCache(List<CartItem> cachedItems, IDistributedCache cache)
{
    await cache.SetStringAsync(CacheKey,
        JsonSerializer.Serialize(cachedItems),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
}

app.MapDefaultEndpoints();

app.Run();
