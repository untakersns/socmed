using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace socmed_front.Services;

/// Базовый класс для API-сервисов, обеспечивающий автоматическое обновление токенов
/// при получении 401 ошибки (Unauthorized). Это ключевой элемент для решения проблемы с 401 ошибками.
/// Подход аналогичен тому, что используется в AuthSample.Frontend - явная проверка 401 ошибок
/// и повторный вызов метода после обновления токена.
public abstract class BaseApiService
{
    protected readonly HttpClient _httpClient;
    protected readonly ITokenService _tokenService;
    protected readonly AuthService _authService;

    protected BaseApiService(HttpClient httpClient, ITokenService tokenService, AuthService authService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _authService = authService;
    }

    /// Выполняет API-вызов с автоматическим обновлением токена при 401 ошибке
    /// Сначала выполняет вызов, если получает 401, то обновляет токен и повторяет вызов
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    /// <param name="apiCall">Функция, выполняющая HTTP-запрос</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое при ошибке</param>
    /// <returns>Результат API-вызова или значение по умолчанию</returns>
    protected async Task<T?> ExecuteWithTokenRefreshAsync<T>(Func<Task<HttpResponseMessage>> apiCall, T? defaultValue = default)
    {
        // Сначала пробуем выполнить запрос
        var response = await apiCall();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        
        // Если получили 401, пробуем обновить токен и повторить
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Пытаемся обновить токен через AuthService
            if (await _authService.RefreshTokenAsync())
            {
                // После обновления токена пробуем снова
                var accessToken = await _tokenService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Устанавливаем обновленный токен в заголовок клиента
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    
                    response = await apiCall();
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return await response.Content.ReadFromJsonAsync<T>();
                    }
                }
            }
            
            // Если обновление токена не помогло, возвращаем значение по умолчанию
            return defaultValue;
        }

        // Для других статусов возвращаем значение по умолчанию
        return defaultValue;
    }

    /// Выполняет API-вызов, возвращающий boolean результат, с автоматическим обновлением токена
    /// при 401 ошибке
    /// <param name="apiCall">Функция, выполняющая HTTP-запрос</param>
    /// <returns>true, если запрос успешен, false в противном случае</returns>
    protected async Task<bool> ExecuteWithTokenRefreshAsync(Func<Task<HttpResponseMessage>> apiCall)
    {
        // Сначала пробуем выполнить запрос
        var response = await apiCall();

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        
        // Если получили 401, пробуем обновить токен и повторить
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Пытаемся обновить токен через AuthService
            if (await _authService.RefreshTokenAsync())
            {
                // После обновления токена пробуем снова
                var accessToken = await _tokenService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Устанавливаем обновленный токен в заголовок клиента
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    
                    response = await apiCall();
                    
                    return response.IsSuccessStatusCode;
                }
            }
            
            // Если обновление токена не помогло, возвращаем false
            return false;
        }

        // Для других статусов возвращаем false
        return false;
    }
}