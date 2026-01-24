using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;
using socmed.Services;

namespace socmed.Mediator.Handler
{
    public record LoginResponse(string AccessToken, string RefreshToken);
    public record LoginCommand(string Email, string Password) : IRequest<LoginResponse?>;

    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse?>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;

        public LoginHandler(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
        }

        public async Task<LoginResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid) return null;

            var accessToken = _jwtProvider.GenerateToken(user);

            var refreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new LoginResponse(accessToken, refreshToken);
        }
    }
}