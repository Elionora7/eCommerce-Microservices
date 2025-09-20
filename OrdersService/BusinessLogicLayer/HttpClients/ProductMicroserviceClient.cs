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
public class ProductMicroserviceClient : IProductMicroserviceClient
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
                return JsonSerializer.Deserialize<ProductDTO?>(cacheValue);
            }

            // Use the correct path that matches Ocelot's upstream template
            var requestUri = $"search/product-id/{productId}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            _logger.LogInformation("Sending request to {Uri}",
            _httpClient.BaseAddress != null ? new Uri(_httpClient.BaseAddress, requestUri).ToString()
                  : $"BaseAddress is null, requestUri: {requestUri}");

            _logger.LogInformation("BaseAddress: {BaseAddress}", _httpClient.BaseAddress);

            // Forward the Authorization header if present
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.Add("Authorization", authHeader.ToString());
            }

            // Send request
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        return null;
                    case System.Net.HttpStatusCode.BadRequest:
                        throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                    case System.Net.HttpStatusCode.ServiceUnavailable:
                        var fallback = await response.Content.ReadFromJsonAsync<ProductDTO>();
                        return fallback;
                    default:
                        _logger.LogWarning("HTTP request failed with status code {StatusCode} for product {ProductId}", response.StatusCode, productId);
                        return null;
                }
            }

            var product = await response.Content.ReadFromJsonAsync<ProductDTO>();
            if (product == null) throw new ArgumentException("Invalid product Id!");

            // Cache the result
            var productJson = JsonSerializer.Serialize(product);
            var cacheOptions = new DistributedCacheEntryOptions()
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
            _logger.LogError(ex, "Bulkhead isolation blocking request for product {ProductId}", productId);
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