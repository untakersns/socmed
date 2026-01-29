using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using socmed.Mediator.Handler;
using System.Net.Http;

namespace socmed_front.Services
{

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, ProtectedLocalStorage localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(string email, string password)
        {
            var loginModel = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    await _localStorage.SetAsync("accessToken", result.AccessToken);
                    await _localStorage.SetAsync("refreshToken", result.RefreshToken);
                    return true;
                }
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.AccessToken);
                return true;
            }
            return false;
        }

        public async Task Logout()
        {
            await _localStorage.DeleteAsync("accessToken");
            await _localStorage.DeleteAsync("refreshToken");
        }
    }
}