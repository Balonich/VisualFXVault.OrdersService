using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies.Implementations;

public class UsersMicroservicePolicies : BaseMicroservicePolicies
{
    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : base(logger)
    {
    }

    public override IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = GetExponentialRetryPolicy();
        var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        var timeoutPolicy = GetTimeoutPolicy();

        return Policy.WrapAsync(
            retryPolicy,
            circuitBreakerPolicy,
            timeoutPolicy
        );
    }
}
