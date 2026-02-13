using System.Net.Http.Headers;
using AuthSample.Frontend.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AuthSample.Frontend.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedLocalStorage _localStorage;

    public AuthService(
        HttpClient httpClient,
        ProtectedLocalStorage localStorage
    )
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/Auth/register",
                request
            );

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await SaveTokensAsync(authResponse);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/Auth/login",
                request
            );

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await SaveTokensAsync(authResponse);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var userId = await _localStorage.GetAsync<Guid>("userId");
            var refreshToken = await _localStorage.GetAsync<string>("refreshToken");

            if (!userId.Success || !refreshToken.Success)
                return false;

            var request = new RefreshRequest
            {
                UserId = userId.Value,
                RefreshToken = refreshToken.Value,
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/Auth/refresh",
                request
            );
            
            Console.WriteLine($"Refresh Response: {response.StatusCode.ToString()}");
            Console.WriteLine($"Refresh Message: {response.Content.ReadAsStringAsync().Result}");

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await SaveTokensAsync(authResponse);
                    return true;
                }
            }

            await LogoutAsync();
            return false;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var userId = await _localStorage.GetAsync<Guid>("userId");
            var refreshToken = await _localStorage.GetAsync<string>("refreshToken");

            if (userId.Success && refreshToken.Success)
            {
                var request = new LogoutRequest
                {
                    UserId = userId.Value,
                    RefreshToken = refreshToken.Value,
                };

                await _httpClient.PostAsJsonAsync("/api/Auth/logout", request);
            }
        }
        catch
        {
            // Игнорируем ошибки при выходе
        }
        finally
        {
            await ClearTokensAsync();
        }
    }

    public async Task<UserInfoResponse?> GetCurrentUserAsync()
    {
        try
        {
            var accessToken = await _localStorage.GetAsync<string>(
                "accessToken"
            );

            if (!accessToken.Success)
                return null;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.Value);

            var response = await _httpClient.GetAsync("/user");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfoResponse>();
            }

            // Если 401, пробуем обновить токен
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokenAsync())
                {
                    return await GetCurrentUserAsync(); // Повторная попытка
                }
            }

            return null;
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public async Task<List<UserInfoResponse>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/user/list");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<UserInfoResponse>>()
                       ?? new List<UserInfoResponse>();
            }

            return new List<UserInfoResponse>();
        }
        catch
        {
            return new List<UserInfoResponse>();
        }
    }

    public async Task<List<string>> GetFollowingAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/user/{userId}/following");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ShortUserInfoResponse>>()
                             ?? new List<ShortUserInfoResponse>();
                return result.Select(u => u.Login ?? "").ToList();
            }

            return new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<List<string>> GetFollowersAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/user/{userId}/followers");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ShortUserInfoResponse>>()
                             ?? new List<ShortUserInfoResponse>();
                return result.Select(u => u.Login ?? "").ToList();
            }

            return new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<bool> FollowUserAsync(Guid userId)
    {
        try
        {
            var accessToken = await _localStorage.GetAsync<string>("accessToken");

            if (!accessToken.Success)
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.Value);

            var response = await _httpClient.PostAsync($"/user/{userId}/follow", null);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokenAsync())
                {
                    return await FollowUserAsync(userId);
                }
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UnfollowUserAsync(Guid userId)
    {
        try
        {
            var accessToken = await _localStorage.GetAsync<string>("accessToken");

            if (!accessToken.Success)
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.Value);

            var response = await _httpClient.DeleteAsync($"/user/{userId}/follow");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokenAsync())
                {
                    return await UnfollowUserAsync(userId);
                }
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var accessToken = await _localStorage.GetAsync<string>(
            "accessToken"
        );
        return !string.IsNullOrEmpty(accessToken.Value);
    }

    private async Task SaveTokensAsync(AuthResponse authResponse)
    {
        await _localStorage.SetAsync("accessToken", authResponse.AccessToken);
        await _localStorage.SetAsync("refreshToken", authResponse.RefreshToken);
        await _localStorage.SetAsync("userId", authResponse.UserId);
    }

    private async Task ClearTokensAsync()
    {
        await _localStorage.DeleteAsync("accessToken");
        await _localStorage.DeleteAsync("refreshToken");
        await _localStorage.DeleteAsync("userId");
    }
}