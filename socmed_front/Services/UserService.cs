using socmed_front.Models;
using System.Net.Http.Headers;

namespace socmed_front.Services;

/// Сервис для работы с пользовательскими данными
/// Наследуется от BaseApiService, что обеспечивает автоматическую обработку 401 ошибок
/// и повторный вызов методов с обновленным токеном при необходимости
public class UserService : BaseApiService
{
    public UserService(HttpClient httpClient, ITokenService tokenService, AuthService authService) : base(httpClient, tokenService, authService)
    {
    }

    /// Получает профиль пользователя по ID
    /// Использует ExecuteWithTokenRefreshAsync для автоматической обработки 401 ошибок
    public async Task<UserDto?> GetUserProfile(string id)
    {
        return await ExecuteWithTokenRefreshAsync<UserDto>(async () => await _httpClient.GetAsync($"users/{id}"));
    }

    /// Обновляет профиль пользователя
    public async Task<bool> UpdateUserProfile(string id, UpdateUserProfileRequest request)
    {
        return await ExecuteWithTokenRefreshAsync(async () => await _httpClient.PutAsJsonAsync($"users/{id}", request));
    }

    /// Получает список подписчиков пользователя
    public async Task<List<UserDto>> GetFollowers(string id)
    {
        return await ExecuteWithTokenRefreshAsync<List<UserDto>>(async () => await _httpClient.GetAsync($"users/followers/{id}"))
               ?? new List<UserDto>();
    }

    /// Получает список, на кого подписан пользователь
    public async Task<List<UserDto>> GetFollowing(string id)
    {
        return await ExecuteWithTokenRefreshAsync<List<UserDto>>(async () => await _httpClient.GetAsync($"users/following/{id}"))
               ?? new List<UserDto>();
    }

    /// Получает список всех пользователей
    public async Task<List<UserDto>> GetAllUsers()
    {
        return await ExecuteWithTokenRefreshAsync<List<UserDto>>(async () => await _httpClient.GetAsync("users"))
               ?? new List<UserDto>();
    }

    /// Подписывается на пользователя
    public async Task<bool> FollowUser(string targetId)
    {
        return await ExecuteWithTokenRefreshAsync(async () => await _httpClient.PostAsJsonAsync($"users/follow/{targetId}", new { }));
    }

    /// Отписывается от пользователя
    public async Task<bool> UnfollowUser(string targetId)
    {
        return await ExecuteWithTokenRefreshAsync(async () => await _httpClient.DeleteAsync($"users/unfollow/{targetId}"));
    }
}