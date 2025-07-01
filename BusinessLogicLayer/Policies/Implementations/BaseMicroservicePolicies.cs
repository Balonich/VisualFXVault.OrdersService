using BusinessLogicLayer.Interfaces.Policies;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;

namespace BusinessLogicLayer.Policies.Implementations;

public class BaseMicroservicePolicies : IMicroservicePolicies
{
    private readonly ILogger<BaseMicroservicePolicies> _logger;

    public BaseMicroservicePolicies(ILogger<BaseMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public virtual IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy(int retryCount = 5, int initialDelaySeconds = 2)
    {
        return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: retryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(initialDelaySeconds, retryAttempt)),
                onRetry: (response, timeSpan, retryAttempt, context) =>
                {
                    _logger.LogInformation($"Retry №{retryAttempt} after {timeSpan.TotalSeconds} seconds due to: {response.Result?.StatusCode}. URL: {response.Result?.RequestMessage?.RequestUri}");
                }
            );
    }

    public virtual IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking = 3, int breakDurationSeconds = 30)
    {
        return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(breakDurationSeconds),
                onBreak: (response, timespan) =>
                {
                    _logger.LogWarning($"Circuit broken due to: {response.Result?.StatusCode}. URL: {response.Result?.RequestMessage?.RequestUri}. Break duration: {timespan.TotalSeconds} seconds.");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit reset.");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit is half-open, next call will test the circuit.");
                }
            );
    }

    public virtual IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds = 5)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(timeoutSeconds));
    }

    public virtual IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {
                _logger.LogWarning("Default fallback policy triggered, returning empty response.");
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            });
    }

    public virtual IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(int maxParallelization = 5, int maxQueuingActions = 50)
    {
        return Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: maxParallelization,
            maxQueuingActions: maxQueuingActions,
            onBulkheadRejectedAsync: async (context) =>
            {
                _logger.LogWarning("Bulkhead isolation policy triggered, request rejected due to too many concurrent requests.");

                throw new BulkheadRejectedException("Bulkhead queue is full, request rejected.");
            });
    }

    public virtual IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = GetExponentialRetryPolicy();
        var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        var timeoutPolicy = GetTimeoutPolicy();

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
    }
}
