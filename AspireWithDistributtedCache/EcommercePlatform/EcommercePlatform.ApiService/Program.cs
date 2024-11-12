using System.Text.Json;
using EcommercePlatform.ServiceDefaults.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.AddRedisDistributedCache(connectionName: "cache");

WebApplication app = builder.Build();

app.UseExceptionHandler();

List<Product> products = new()
{
    new(1, "Mouse", 9.99M),
    new(2, "Keyboard", 15.99M),
    new(3, "Printer", 199.99M),
    new(4, "PC Monitor", 399.99M),
    new(5, "Laptop Computer", 899.99M),
    new(6, "Gaming PC", 1699.99M),
};

app.MapGet("/api/products/", () =>
{
    return Results.Ok(products);
});

app.MapGet("/api/products/{productId}", async (int productId, IDistributedCache cache) =>
{
    if (!products.Any(p => p.Id == productId))
    {
        return Results.NotFound($"Product {productId} not found");
    }

    string cacheKey = $"Product_{productId}";
    string? cachedProduct = await cache.GetStringAsync(cacheKey);

    if (cachedProduct is not null)
    {
        return Results.Ok(JsonSerializer.Deserialize<Product>(cachedProduct));
    }

    // Simulate a database call
    Product product = await GetProductFromDatabaseAsync(productId);
    string serializedProduct = JsonSerializer.Serialize(product);

    // Store product in Redis with a 5-minute expiration
    await cache.SetStringAsync(cacheKey, serializedProduct, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });

    return Results.Ok(product);
});

async Task<Product> GetProductFromDatabaseAsync(int productId)
{
    // Simulate database access
    await Task.Delay(100); // Simulated delay
    return products.Single(p => p.Id == productId);
}

app.MapPut("/api/products/{productId}", async (int productId, Product updatedProduct, IDistributedCache cache, IConnectionMultiplexer redis) =>
{
    if (!products.Any(p => p.Id == productId))
    {
        return Results.NotFound($"Product {productId} not found");
    }

    string lockKey = $"ProductLock_{productId}";
    var db = redis.GetDatabase();

    bool lockAcquired = await db.LockTakeAsync(lockKey, Environment.MachineName, TimeSpan.FromSeconds(10));

    if (!lockAcquired)
    {
        return Results.StatusCode(423); // Locked by another process
    }

    try
    {
        // Perform the update (e.g., database update)
        await UpdateProductInDatabaseAsync(productId, updatedProduct);

        // Invalidate the cache
        string cacheKey = $"Product_{productId}";
        await cache.RemoveAsync(cacheKey);

        return Results.NoContent();
    }
    finally
    {
        // Release the lock
        await db.LockReleaseAsync(lockKey, Environment.MachineName);
    }
});

async Task UpdateProductInDatabaseAsync(int productId, Product updatedProduct)
{
    await Task.Delay(100);
    var oldProduct = products.Single(p => p.Id == productId);

    products.Remove(oldProduct);
    products.Add(updatedProduct);
}

app.MapDefaultEndpoints();

app.Run();
