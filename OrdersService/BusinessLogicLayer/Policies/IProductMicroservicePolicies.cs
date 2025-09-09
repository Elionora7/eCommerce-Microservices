using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IProductMicroservicePolicies

{
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
    IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}