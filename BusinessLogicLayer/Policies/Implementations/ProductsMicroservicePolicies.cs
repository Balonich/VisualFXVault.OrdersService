using System.Text;
using System.Text.Json;
using BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;

namespace BusinessLogicLayer.Policies.Implementations;

public class ProductsMicroservicePolicies : BaseMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;

    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) : base(logger)
    {
        _logger = logger;
    }

    public override IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {
                _logger.LogWarning("Fallback policy triggered due to an unsuccessful response, returning dummy data.");

                var product = new ProductResponseDto(
                    ProductId: Guid.Empty,
                    ProductName: "Fallback Product",
                    Category: "Fallback Category",
                    UnitPrice: 0,
                    QuantityInStock: 0);

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                };

                return response;
            });
    }

    public override IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var fallbackPolicy = GetFallbackPolicy();
        var bulkheadIsolationPolicy = GetBulkheadIsolationPolicy(maxParallelization: 2, maxQueuingActions: 40);

        return Policy.WrapAsync(
            fallbackPolicy,
            bulkheadIsolationPolicy
        );
    }
}
