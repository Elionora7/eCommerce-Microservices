using System.Net.Http.Json;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
public class ProductMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductMicroserviceClient(
        HttpClient httpClient,
        ILogger<ProductMicroserviceClient> logger,
        IDistributedCache distributedCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        try
        {
            string cacheKey = $"product:{productId}";
            string? cacheValue = await _distributedCache.GetStringAsync(cacheKey);
            if (cacheValue != null)
            {
                ProductDTO? productCache = JsonSerializer.Deserialize<ProductDTO?>(cacheValue);
                return productCache;
            }

            // Create request with authorization header
            var request = new HttpRequestMessage(HttpMethod.Get, $"/gateway/products/search/product-id/{productId}");

            // Forward the authorization header
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Add("Authorization", authorizationHeader.ToString());
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            // ..
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    ProductDTO? productFallback = await response.Content.ReadFromJsonAsync<ProductDTO>();
                    if (productFallback == null)
                    {
                        _logger.LogWarning("Fallback policy was not implemented for product {ProductId}", productId);
                        return null;
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
                    _logger.LogWarning("HTTP request failed with status code {StatusCode} for product {ProductId}", response.StatusCode, productId);
                    return null;
                }
            }

            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
            if (product == null)
            {
                throw new ArgumentException("Invalid product Id!");
            }

            // Write to cache
            string productJson = JsonSerializer.Serialize(product);
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(400))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            await _distributedCache.SetStringAsync(cacheKey, productJson, cacheOptions);
            return product;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Product service circuit breaker open for product {ProductId}. Returning null.", productId);
            return null;
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Product service timeout for product {ProductId}. Returning null.", productId);
            return null;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation is blocking the request since the request queue is full for product {ProductId}", productId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for product {ProductId}", productId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching product {ProductId}", productId);
            return null;
        }
    }
}