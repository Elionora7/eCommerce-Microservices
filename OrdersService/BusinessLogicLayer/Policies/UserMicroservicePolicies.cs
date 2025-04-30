using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UserMicroservicePolicies : IUserMicroservicePolicies
{
    private readonly ILogger<UserMicroservicePolicies> _logger;
    private readonly IPollyPolicies _pollyPolicies;
    public UserMicroservicePolicies(ILogger<UserMicroservicePolicies> logger,IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(4);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(3));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromMilliseconds(1500));

        AsyncPolicyWrap<HttpResponseMessage> combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return combinedPolicy;
    }

}