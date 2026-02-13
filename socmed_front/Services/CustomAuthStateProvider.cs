using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace socmed_front.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ITokenService _tokenService;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public CustomAuthStateProvider(ProtectedLocalStorage localStorage, ITokenService tokenService)
    {
        _localStorage = localStorage;
        _tokenService = tokenService;
        _tokenService.OnTokenChanged += OnTokensChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenService.GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            // Проверяем формат токена перед чтением
            if (!_tokenHandler.CanReadToken(token))
                return new AuthenticationState(_anonymous);

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task NotifyUserAuthenticationAsync(string accessToken, string refreshToken)
    {
        // Получаем userId из токена, если он есть
        Guid userId = Guid.Empty;
        if (_tokenHandler.CanReadToken(accessToken))
        {
            var token = _tokenHandler.ReadJsonWebToken(accessToken);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameidentifier" || c.Type == "sub");
            if (Guid.TryParse(userIdClaim?.Value, out Guid parsedId))
            {
                userId = parsedId;
            }
        }

        if (userId != Guid.Empty)
        {
            await _tokenService.SetTokensAsync(accessToken, refreshToken, userId);
        }
        else
        {
            await _tokenService.SetTokensAsync(accessToken, refreshToken);
        }

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        NotifyAuthenticationStateChanged(authState);
    }

    public async Task NotifyUserLogoutAsync()
    {
        await _tokenService.ClearTokensAsync();

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private async Task OnTokensChanged()
    {
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var token = _tokenHandler.ReadJsonWebToken(jwt);
        return token.Claims;
    }
}