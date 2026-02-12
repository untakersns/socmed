using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace socmed_front.Services;

public class JwtInterceptor : DelegatingHandler
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly TokenProvider _tokenProvider;

    public JwtInterceptor(HttpMessageHandler innerHandler, ProtectedLocalStorage localStorage, TokenProvider tokenProvider) : base(innerHandler)
    {
        _localStorage = localStorage;
        _tokenProvider = tokenProvider;
    }

    public JwtInterceptor(ProtectedLocalStorage localStorage, TokenProvider tokenProvider)
    {
        _localStorage = localStorage;
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Проверяем, есть ли токен в TokenProvider
        if (!string.IsNullOrEmpty(_tokenProvider.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
        }
        else
        {
            try
            {
                var tokenResult = await _localStorage.GetAsync<string>("accessToken");
                if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
                {
                    _tokenProvider.Token = tokenResult.Value;
                    _tokenProvider.IsInitialized = true;
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения токена из localStorage: {ex.Message}");
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}