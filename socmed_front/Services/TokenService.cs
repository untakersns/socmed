using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace socmed_front.Services
{
    /// Сервис для управления JWT-токенами (access, refresh) и ID пользователя
    /// Важно: Не кэширует токены в памяти, всегда читает из ProtectedLocalStorage
    /// для обеспечения актуальности данных при обновлении токенов
    public interface ITokenService
    {
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task<Guid?> GetUserIdAsync();
        Task SetTokensAsync(string accessToken, string refreshToken, Guid userId);
        Task SetTokensAsync(string accessToken, string refreshToken); // Для обратной совместимости
        Task ClearTokensAsync();
        Task<bool> HasValidTokensAsync();
        event Func<Task>? OnTokenChanged;
    }

    public class TokenService : ITokenService
    {
        private readonly ProtectedLocalStorage _localStorage;

        public event Func<Task>? OnTokenChanged;

        public TokenService(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
        }

        /// Получает access-токен из защищенного хранилища браузера
        /// Важно: Всегда читает из хранилища, а не использует кэшированное значение
        public async Task<string?> GetAccessTokenAsync()
        {
            var result = await _localStorage.GetAsync<string>("accessToken");
            return result.Success ? result.Value : null;
        }

        /// Получает refresh-токен из защищенного хранилища браузера
        public async Task<string?> GetRefreshTokenAsync()
        {
            var result = await _localStorage.GetAsync<string>("refreshToken");
            return result.Success ? result.Value : null;
        }

        /// Получает ID пользователя из защищенного хранилища браузера
        public async Task<Guid?> GetUserIdAsync()
        {
            var result = await _localStorage.GetAsync<Guid>("userId");
            return result.Success ? result.Value : (Guid?)null;
        }

        /// Сохраняет все токены и ID пользователя в защищенное хранилище
        /// Вызывает событие OnTokenChanged для уведомления подписчиков об изменении токенов
        public async Task SetTokensAsync(string accessToken, string refreshToken, Guid userId)
        {
            await _localStorage.SetAsync("accessToken", accessToken);
            await _localStorage.SetAsync("refreshToken", refreshToken);
            await _localStorage.SetAsync("userId", userId);

            if (OnTokenChanged != null)
            {
                await OnTokenChanged.Invoke();
            }
        }

        /// Сохраняет только access и refresh токены (для обратной совместимости)
        public async Task SetTokensAsync(string accessToken, string refreshToken)
        {
            await _localStorage.SetAsync("accessToken", accessToken);
            await _localStorage.SetAsync("refreshToken", refreshToken);

            if (OnTokenChanged != null)
            {
                await OnTokenChanged.Invoke();
            }
        }

        /// Очищает все токены из защищенного хранилища
        /// Вызывает событие OnTokenChanged для уведомления подписчиков
        public async Task ClearTokensAsync()
        {
            await _localStorage.DeleteAsync("accessToken");
            await _localStorage.DeleteAsync("refreshToken");
            await _localStorage.DeleteAsync("userId");

            if (OnTokenChanged != null)
            {
                await OnTokenChanged.Invoke();
            }
        }

        /// Проверяет, существуют ли валидные токены (и access, и refresh)
        public async Task<bool> HasValidTokensAsync()
        {
            var accessToken = await GetAccessTokenAsync();
            var refreshToken = await GetRefreshTokenAsync();
            
            return !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken);
        }
    }
}