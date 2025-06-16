using System.Net.Http.Json;
using BusinessLogicLayer.DTOs;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");

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

            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, $"Circuit breaker is open. Returning fault data for user {userId}.");

            return new UserResponseDto(userId, "Temporarily Unavailable", "Temporarily Unavailable", "Unknown");
        }
    }
}