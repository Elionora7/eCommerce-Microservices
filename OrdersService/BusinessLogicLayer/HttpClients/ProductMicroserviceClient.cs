

using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System.Net.Http.Json;
using Polly.Bulkhead;
using Microsoft.Extensions.Logging;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductMicroserviceClient
{
    public readonly HttpClient _httpClient;
    public readonly ILogger<ProductMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    public ProductMicroserviceClient(HttpClient httpClient,
                                     ILogger<ProductMicroserviceClient> logger,
                                     IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        try
        {
            
            //read from the cacheKey
            string cacheKey= $"product:{productId}";  
            string? cacheValue= await _distributedCache.GetStringAsync(cacheKey);
            if (cacheValue != null)
            {
               ProductDTO? productCache  = JsonSerializer.Deserialize<ProductDTO?>(cacheValue);
               return productCache;
            }
            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                //if error code =503 returned from the httpclient 
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    ProductDTO? productFallback = await response.Content.ReadFromJsonAsync<ProductDTO>();

                    if (productFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented!");
                    }

                    return productFallback;
                }

                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with status code {response.StatusCode}!");
                }
            }
            
            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();

            if (product == null)
            {
                throw new ArgumentException("Invalid product Id!");
            }


            // write into the cacheKey
            string productJson = JsonSerializer.Serialize(product);

            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
              .SetAbsoluteExpiration(TimeSpan.FromSeconds(400))
              .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            await _distributedCache.SetStringAsync(cacheKey, productJson, cacheOptions);
            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex,"Bulkhead isolation is blocking the request since the request queue is full!");

            return new ProductDTO(
              ProductID: Guid.NewGuid(),
              ProductName: "Temporarily Unavailable(Bulkhead)!",
              Category: "Temporarily Unavailable(Bulkhead)!",
              UnitPrice: 0,
              QuantityInStock: 0);
        }
    }
}
