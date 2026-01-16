using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;
using socmed.Services;
using System.Security.Claims;
using System.Security.Cryptography;

namespace socmed.Mediator.Handler
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<TokenResponse?>;
    public record TokenResponse(string AccessToken, string RefreshToken);

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, TokenResponse?>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;

        public async Task<TokenResponse?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            var newAccessToken = _jwtProvider.GenerateToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new TokenResponse(newAccessToken, newRefreshToken);
        }
    }
}
