using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<PollyPolicies> _logger; 

    public PollyPolicies(ILogger<PollyPolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
                durationOfBreak: durationOfBreak,
                onBreak: (outcome, timespan, context) =>
                {
                    _logger.LogWarning($"Circuit breaker open for {timespan.TotalSeconds} seconds due to consecutive failures. " +
                                      $"The remaining requests will be blocked! Reason: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                },
                onReset: (context) => {
                    _logger.LogInformation("Circuit breaker closed. The subsequent requests will be allowed!");
                },
                onHalfOpen: () => {
                    _logger.LogInformation("Circuit breaker half-open. Testing if service has recovered...");
                });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: retryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds! " +
                                      $"Reason: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(
            timeout,
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (context, timespan, task, exception) =>
            {
                _logger.LogWarning($"Request timed out after {timespan.TotalSeconds} seconds");
                return Task.CompletedTask;
            });

        return policy;
    }
}