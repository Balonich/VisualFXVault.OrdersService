using Microsoft.AspNetCore.Http;
using Polly;

namespace BusinessLogicLayer.Interfaces.Policies;

public interface IMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
}
