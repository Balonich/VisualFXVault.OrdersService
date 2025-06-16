using BusinessLogicLayer.Interfaces.Policies;
using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies.Implementations;

public class MicroservicePolicies : IMicroservicePolicies
{
    private readonly ILogger<MicroservicePolicies> _logger;

    public MicroservicePolicies(ILogger<MicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy()
    {
        return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (response, timeSpan, retryAttempt, context) =>
                {
                    _logger.LogInformation($"Retry №{retryAttempt} after {timeSpan.TotalSeconds} seconds due to: {response.Result?.StatusCode}. URL: {response.Result?.RequestMessage?.RequestUri}");
                }
            );
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
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
}
