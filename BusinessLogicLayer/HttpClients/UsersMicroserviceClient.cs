using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogicLayer.DTOs;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient,
        ILogger<UsersMicroserviceClient> logger,
        IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var cacheKey = $"user:{userId}";
            var cachedUser = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedUser != null)
            {
                _logger.LogInformation($"Cache hit for user {userId}");
                return JsonSerializer.Deserialize<UserResponseDto>(cachedUser);
            }
            else
            {
                _logger.LogInformation($"Cache miss for user {userId}");
            }

            var response = await _httpClient.GetAsync($"api/gateway/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; // User not found
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException($"Error fetching user: {response.ReasonPhrase}", null, response.StatusCode);
                }
                else
                {
                    // throw new HttpRequestException($"Unexpected error: {response.ReasonPhrase}", null, response.StatusCode);

                    // Returning fault data instead of throwing an exception
                    // This is useful for scenarios where the user data is not critical
                    // and we want to avoid breaking the flow of the application.
                    return new UserResponseDto(userId, "Temporarily Unavailable", "Temporarily Unavailable", "Unknown");
                }

            }

            var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();

            if (user == null)
            {
                throw new ArgumentException($"Invalid user id: {userId}");
            }

            await _distributedCache.SetStringAsync(cacheKey,
                JsonSerializer.Serialize(user),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                });

            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, $"Circuit breaker is open. Returning fault data for user {userId}.");

            return new UserResponseDto(userId, "Temporarily Unavailable (circuit breaker)", "Temporarily Unavailable (circuit breaker)", "Unknown");
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, $"Request timed out while fetching user {userId}. Returning fault data.");

            return new UserResponseDto(userId, "Temporarily Unavailable (timeout)", "Temporarily Unavailable (timeout)", "Unknown");
        }
    }
}