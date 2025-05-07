using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductMicroservicePolicies : IProductMicroservicePolicies
{
    private readonly ILogger<ProductMicroservicePolicies> _logger;

    public ProductMicroservicePolicies(ILogger<ProductMicroservicePolicies> logger)
    {
        _logger = logger;
    }


    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .FallbackAsync(async (context) =>
          {
              _logger.LogWarning("Fallback triggered:The request has failed, returning dummy data!");

            ProductDTO product = new ProductDTO(ProductID: Guid.Empty,
            ProductName: "Temporarily Unavailable (fallback)!",
            Category: "Temporarily Unavailable (fallback)!",
            UnitPrice: 0,
            QuantityInStock: 0
            );
              //change the status code from ok(200) to service unavailable(503)
              //in order to exclude the cache result from the fallback response
              var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
              {
                  Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
              };

              return response;
          });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
          maxParallelization:2, //Allow up to 2 concurrent requests
          maxQueuingActions:30, //Queue up to 30 additional requests
          onBulkheadRejectedAsync:(context) => {
              _logger.LogWarning("Bulkhead Isolation triggered.Can't send any more requests since the queue is full!");

              throw new BulkheadRejectedException("Bulkhead queue is full!");
          }
          );

        return policy;
    }
}