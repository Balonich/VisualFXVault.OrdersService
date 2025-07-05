using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public ProductsMicroserviceClient(HttpClient httpClient,
        ILogger<ProductsMicroserviceClient> logger,
        IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var cacheKey = $"product:{productId}";
            var cachedProduct = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedProduct != null)
            {
                _logger.LogInformation($"Cache hit for product {productId}");
                return JsonSerializer.Deserialize<ProductResponseDto>(cachedProduct);
            }
            else
            {
                _logger.LogInformation($"Cache miss for product {productId}");
            }

            var response = await _httpClient.GetAsync($"api/gateway/products/search/product-id/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    var productFromFallback = await response.Content.ReadFromJsonAsync<ProductResponseDto>();

                    if (productFromFallback == null)
                    {
                        throw new NotImplementedException($"Fallback policy was not implemented");
                    }

                    return productFromFallback;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; // Product not found
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException($"Error fetching product: {response.ReasonPhrase}", null, response.StatusCode);
                }
                else
                {
                    throw new HttpRequestException($"Unexpected error: {response.ReasonPhrase}", null, response.StatusCode);
                }
            }

            var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>();

            if (product == null)
            {
                throw new ArgumentException($"Invalid product id: {productId}");
            }

            await _distributedCache.SetStringAsync(cacheKey, 
                JsonSerializer.Serialize(product), 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                });

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, $"Bulkhead isolation rejected the request for product {productId}. Returning fault data.");

            return new ProductResponseDto
            (
                ProductId: Guid.NewGuid(),
                ProductName: "Temporarily Unavailable (Bulkhead)",
                Category: "Temporarily Unavailable (Bulkhead)",
                UnitPrice: 0,
                QuantityInStock: 0
            );
        }
    }

    public async Task<IEnumerable<ProductResponseDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        var response = await _httpClient.GetAsync($"api/gateway/products/search/product-ids/{string.Join(",", productIds)}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<ProductResponseDto>(); // No products found
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException($"Error fetching products: {response.ReasonPhrase}", null, response.StatusCode);
            }
            else
            {
                throw new HttpRequestException($"Unexpected error: {response.ReasonPhrase}", null, response.StatusCode);
            }
        }

        var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductResponseDto>>();

        if (products == null || !products.Any())
        {
            throw new ArgumentException("Invalid product ids");
        }

        return products;
    }
}