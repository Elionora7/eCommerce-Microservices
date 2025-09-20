using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
public class UserMicroserviceClient : IUserMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserMicroserviceClient(
        HttpClient httpClient,
        ILogger<UserMicroserviceClient> logger,
        IDistributedCache distributedCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserDTO?> GetUserByUserId(Guid userId)
    {
        try
        {
            string cacheKey = $"user:{userId}";
            // Read from cache
            string? cacheValue = await _distributedCache.GetStringAsync(cacheKey);
            if (cacheValue != null)
            {
                UserDTO? userCache = JsonSerializer.Deserialize<UserDTO?>(cacheValue);
                return userCache;
            }

            // Create request with authorization header
            var request = new HttpRequestMessage(HttpMethod.Get, $"{userId}");

            // Forward the authorization header from the original request
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Add("Authorization", authorizationHeader.ToString());
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userFallBack = await response.Content.ReadFromJsonAsync<UserDTO>();
                    if (userFallBack == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented!");
                    }

                    return userFallBack;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad Request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    return new UserDTO(Name: "Temporarily Unavailable!",
                                       Email: "Temporarily Unavailable!",
                                       Gender: "Temporarily Unavailable!",
                                       UserID: Guid.Empty
                                       );
                }
            }
            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User Id!");
            }

            //write to cache
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(400))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            string userJson = JsonSerializer.Serialize(user);
            await _distributedCache.SetStringAsync(cacheKey, userJson, cacheOptions);
            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because circuit breaker is in Open state. Returning null!");
            return null;
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Request failed because of timeout. Returning null!");
            return null;
        }
    }
}

       


