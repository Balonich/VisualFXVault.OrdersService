using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        var response = await _httpClient.GetAsync($"api/products/{productId}");

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
}