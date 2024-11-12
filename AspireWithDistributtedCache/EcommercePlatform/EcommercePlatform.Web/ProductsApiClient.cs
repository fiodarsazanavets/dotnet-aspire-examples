using EcommercePlatform.ServiceDefaults.Dtos;

namespace EcommercePlatform.Web;

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

    public async Task<HttpResponseMessage> UpdateProduct(int productId,Product product)
    {
        return await httpClient.PutAsJsonAsync($"/api/products/{productId}", product);
    }
}
