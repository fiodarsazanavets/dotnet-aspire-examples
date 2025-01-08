using OnlineStore.ServiceDefaults.Models;

namespace OnlineStore.Web;

public class ProductsApiClient(HttpClient httpClient)
{
    public async Task<Product[]> GetProductsAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<Product>? products = null;

        await foreach (var product in httpClient.GetFromJsonAsAsyncEnumerable<Product>("/api/products", cancellationToken))
        {
            if (products?.Count >= maxItems)
            {
                break;
            }
            if (product is not null)
            {
                products ??= [];
                products.Add(product);
            }
        }

        return products?.ToArray() ?? [];
    }

    public async Task<Product?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Product>($"/api/products/{productId}", cancellationToken);
    }

    public async Task<CartItem[]> GetCartItemsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CartItem[]>("/api/cart", cancellationToken) ?? [];
    }

    public async Task AddToCartAsync(int productId, CancellationToken cancellationToken = default)
    {
        var cartItem = new CartItem(productId, 1);
        await httpClient.PostAsJsonAsync("api/cart", cartItem, cancellationToken);
    }

    public async Task DeleteItemFromCartAsync(int productId, CancellationToken cancellationToken = default)
    {
        await httpClient.DeleteAsync($"api/cart/{productId}", cancellationToken);
    }
}