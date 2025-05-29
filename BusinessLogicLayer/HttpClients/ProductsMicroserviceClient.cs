using System.Net.Http.Json;
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(Guid productId)
    {
        var response = await _httpClient.GetAsync($"api/products/search/product-id/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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

        return product;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        var response = await _httpClient.GetAsync($"api/products/search/product-ids/{string.Join(",", productIds)}");

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