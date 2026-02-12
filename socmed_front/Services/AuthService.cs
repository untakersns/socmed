using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using socmed_front.Models;

namespace socmed_front.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly TokenProvider _tokenProvider;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;

        public AuthService(HttpClient httpClient, ProtectedLocalStorage localStorage, TokenProvider tokenProvider, CustomAuthStateProvider customAuthStateProvider, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _tokenProvider = tokenProvider;
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
                    await _localStorage.SetAsync("accessToken", authResponse.AccessToken);
                    await _localStorage.SetAsync("refreshToken", authResponse.RefreshToken);
                    
                    // Инициализируем TokenProvider сразу
                    _tokenProvider.Token = authResponse.AccessToken;
                    _tokenProvider.IsInitialized = true;
                    
                    // Обновляем AuthenticationState
                    _authStateProvider.NotifyUserAuthentication(authResponse.AccessToken);
                    
                    return authResponse;
                }
            }
            return null;
        }
        public async Task Logout()
        {
            await _localStorage.DeleteAsync("accessToken");

            _tokenProvider.Token = null;
            _tokenProvider.IsInitialized = false;
            if (_authStateProvider is CustomAuthStateProvider customAuthStateProvider)
            {
                customAuthStateProvider.NotifyUserLogout();
            }
            _navigationManager.NavigateTo("/");
        }
    }
}