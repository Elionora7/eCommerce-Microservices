using System.Text;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductMicroservicePolicies : IProductMicroservicePolicies
{
    private readonly ILogger<ProductMicroservicePolicies> _logger;
    private readonly IPollyPolicies _pollyPolicies;

    public ProductMicroservicePolicies(ILogger<ProductMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(3); // Fewer retries for products
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(5, TimeSpan.FromSeconds(30)); // Shorter break
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5)); // Shorter timeout

        AsyncPolicyWrap<HttpResponseMessage> combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return combinedPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<BrokenCircuitException>()
            .Or<TimeoutRejectedException>()
            .FallbackAsync((context) =>
            {
                _logger.LogWarning("Fallback triggered: The request has failed, returning dummy data!");

                var product = new ProductDTO(
                                   ProductID: Guid.Empty,
                                   ProductName: "Temporarily Unavailable",
                                   Category: "Unavailable",
                                   UnitPrice: 0,
                                   QuantityInStock: 0,
                                   ImgUrl: "https://via.placeholder.com/300x300?text=Product+Unavailable"
                               );

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(product),
                        Encoding.UTF8,
                        "application/json")
                };

                return Task.FromResult(response);
            });
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 2, // Allow up to 2 concurrent requests
            maxQueuingActions: 30, // Queue up to 30 additional requests
            onBulkheadRejectedAsync: (context) => {
                _logger.LogWarning("Bulkhead Isolation triggered. Can't send any more requests since the queue is full!");
                throw new BulkheadRejectedException("Bulkhead queue is full!");
            }
        );

        return policy;
    }
}