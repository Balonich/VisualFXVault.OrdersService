using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public UsersMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
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
                throw new HttpRequestException($"Unexpected error: {response.ReasonPhrase}", null, response.StatusCode);
            }

        }

        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();

        if (user == null)
        {
            throw new ArgumentException($"Invalid user id: {userId}");
        }

        return user;
    }
}