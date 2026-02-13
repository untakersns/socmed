using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using socmed_front.Models;

namespace socmed_front.Services
{
    /// Сервис аутентификации, обеспечивающий регистрацию, логин, логаут и обновление токенов
    /// Ключевая функция - RefreshTokenAsync, которая позволяет автоматически обновлять
    /// истекшие access-токены с помощью refresh-токенов
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly ITokenService _tokenService;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;

        public AuthService(HttpClient httpClient, ProtectedLocalStorage localStorage, ITokenService tokenService, CustomAuthStateProvider customAuthStateProvider, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _tokenService = tokenService;
            _authStateProvider = customAuthStateProvider;
            _navigationManager = navigationManager;
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<AuthResponse?> Login(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await _tokenService.SetTokensAsync(authResponse.AccessToken, authResponse.RefreshToken);

                    // Обновляем AuthenticationState
                    await _authStateProvider.NotifyUserAuthenticationAsync(authResponse.AccessToken, authResponse.RefreshToken);

                    return authResponse;
                }
            }
            return null;
        }

        public async Task Logout()
        {
            // Попробуем отправить logout-запрос на сервер
            try
            {
                var userId = await _tokenService.GetUserIdAsync();
                var refreshToken = await _tokenService.GetRefreshTokenAsync();

                if (userId.HasValue && !string.IsNullOrEmpty(refreshToken))
                {
                    var logoutRequest = new LogoutRequest
                    {
                        AccessToken = await _tokenService.GetAccessTokenAsync() ?? string.Empty,
                        RefreshToken = refreshToken
                    };
                    
                    await _httpClient.PostAsJsonAsync("auth/logout", logoutRequest);
                }
            }
            catch
            {
                // Игнорируем ошибки при logout, просто очищаем локальные токены
            }
            finally
            {
                await _tokenService.ClearTokensAsync();
                await _authStateProvider.NotifyUserLogoutAsync();
                _navigationManager.NavigateTo("/");
            }
        }

        public async Task<bool> HasValidTokensAsync()
        {
            return await _tokenService.HasValidTokensAsync();
        }

        /// Обновляет истекший access-токен с помощью refresh-токена
        /// Вызывается при получении 401 ошибки в BaseApiService
        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var userId = await _tokenService.GetUserIdAsync();
                var refreshToken = await _tokenService.GetRefreshTokenAsync();

                // Если у нас нет необходимых данных для обновления токена, возвращаем false
                if (!userId.HasValue || string.IsNullOrEmpty(refreshToken))
                    return false;

                var refreshRequest = new RefreshTokenRequest(
                    await _tokenService.GetAccessTokenAsync() ?? string.Empty,
                    refreshToken
                );

                // Отправляем запрос на обновление токена на сервер
                var response = await _httpClient.PostAsJsonAsync("auth/refresh-token", refreshRequest);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authResponse != null)
                    {
                        // Обновляем токены в хранилище
                        await _tokenService.SetTokensAsync(authResponse.AccessToken, authResponse.RefreshToken);
                        
                        // Обновляем состояние аутентификации в системе Blazor
                        await _authStateProvider.NotifyUserAuthenticationAsync(authResponse.AccessToken, authResponse.RefreshToken);
                        
                        return true;
                    }
                }

                // Если обновление токена не удалось, очищаем все токены и сбрасываем аутентификацию
                await _tokenService.ClearTokensAsync();
                await _authStateProvider.NotifyUserLogoutAsync();
                
                return false;
            }
            catch
            {
                // В случае ошибки сети или других исключений тоже очищаем токены
                await _tokenService.ClearTokensAsync();
                await _authStateProvider.NotifyUserLogoutAsync();
                
                return false;
            }
        }
    }
}