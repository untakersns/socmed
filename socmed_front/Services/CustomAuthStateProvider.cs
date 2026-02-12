using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace socmed_front.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public CustomAuthStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var storageResult = await _localStorage.GetAsync<string>("accessToken");
            var token = storageResult.Success ? storageResult.Value : null;

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

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var token = _tokenHandler.ReadJsonWebToken(jwt);
        return token.Claims;
    }
}