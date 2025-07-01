using Polly;

namespace BusinessLogicLayer.Interfaces.Policies;

public interface IMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy(int retryCount = 5, int initialDelaySeconds = 2);
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking = 3, int breakDurationSeconds = 30);
    IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds = 5);
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
    IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(int maxParallelization = 5, int maxQueuingActions = 50);
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}
