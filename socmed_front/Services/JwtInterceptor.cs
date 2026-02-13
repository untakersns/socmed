using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net;
using System.Net.Http.Headers;

namespace socmed_front.Services;

/// HTTP-интерцептор, который добавляет JWT-токен к каждому исходящему запросу
/// Важно: Не обрабатывает 401 ошибки здесь, т.к. это может привести к проблемам с повторным использованием HttpRequestMessage
/// Обработка 401 ошибок осуществляется в BaseApiService для каждого конкретного вызова
public class JwtInterceptor : DelegatingHandler
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ITokenService _tokenService;

    public JwtInterceptor(HttpMessageHandler innerHandler, ProtectedLocalStorage localStorage, ITokenService tokenService) : base(innerHandler)
    {
        _localStorage = localStorage;
        _tokenService = tokenService;
    }

    public JwtInterceptor(ProtectedLocalStorage localStorage, ITokenService tokenService)
    {
        _localStorage = localStorage;
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Получаем токен из TokenService
            string? token = await _tokenService.GetAccessTokenAsync();

            // Если токен есть, добавляем его в заголовок Authorization
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // В случае любой ошибки просто продолжаем без токена
            // Это предотвращает падение приложения при проблемах с доступом к токенам
        }

        return await base.SendAsync(request, cancellationToken);
    }
}