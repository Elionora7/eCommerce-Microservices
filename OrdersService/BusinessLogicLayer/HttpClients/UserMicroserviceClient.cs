using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UserMicroserviceClient
{
    public readonly HttpClient _httpClient;
    private readonly ILogger<UserMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UserMicroserviceClient(HttpClient httpClient, ILogger<UserMicroserviceClient> logger,IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<UserDTO?> GetUserByUserId(Guid userId)
    {
        try
        {
            string cacheKey = $"user:{userId}";
            //read from cache
            string? cacheValue = await _distributedCache.GetStringAsync(cacheKey);
            if (cacheValue != null)
            { 
                UserDTO? userCache =  JsonSerializer.Deserialize<UserDTO?>(cacheValue);
                return userCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}");

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
                                       UserID: Guid.Empty);
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
            _logger.LogError(ex,"Request failed because of circuit breaker is in Open state.Returning dummy data!");

            return new UserDTO(
                    Name: "Temporarily Unavailable (ciruit breaker)!",
                    Email: "Temporarily Unavailable (ciruit breaker)!",
                    Gender: "Temporarily Unavailable (ciruit breaker)!",
                    UserID: Guid.Empty);

        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex,"Request failed because of the occured Timeout.Returning dummy data!");

            return new UserDTO(
                    Name: "Temporarily Unavailable (timeout)!",
                    Email: "Temporarily Unavailable (timeout)!",
                    Gender: "Temporarily Unavailable (timeout)!",
                    UserID: Guid.Empty);

        }
    }
}
