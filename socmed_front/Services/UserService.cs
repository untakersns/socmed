using socmed_front.Models;
using System.Net.Http.Headers;

namespace socmed_front.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDto?> GetUserProfile(string id)
    {
        var response = await _httpClient.GetAsync($"users/{id}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        return null;
    }
    public async Task<bool> UpdateUserProfile(string id, UpdateUserProfileRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"users/{id}", request);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> UpdateUserProfileManual(string id, UpdateUserProfileRequest request, string token)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"users/{id}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);

        return response.IsSuccessStatusCode;
    }
    public async Task<List<UserDto>> GetFollowers(string id)
    {
        return await _httpClient.GetFromJsonAsync<List<UserDto>>($"users/followers/{id}")
               ?? new List<UserDto>();
    }

    public async Task<List<UserDto>> GetFollowing(string id)
    {
        return await _httpClient.GetFromJsonAsync<List<UserDto>>($"users/following/{id}")
               ?? new List<UserDto>();
    }
    public async Task<List<UserDto>> GetAllUsers()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("users") ?? new();
        }
        catch
        {
            return new List<UserDto>();
        }
    }
    public async Task<bool> FollowUser(string targetId)
    {
        var response = await _httpClient.PostAsJsonAsync($"users/follow/{targetId}", new { });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnfollowUser(string targetId)
    {
        var response = await _httpClient.DeleteAsync($"users/unfollow/{targetId}");
        return response.IsSuccessStatusCode;
    }
}