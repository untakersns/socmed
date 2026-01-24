using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;
using socmed.Services;
using System.Security.Claims;

namespace socmed.Mediator.Handler
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<LoginResponse?>;

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResponse?>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;

        public RefreshTokenHandler(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
        }

        public async Task<LoginResponse?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Извлекаем данные из старого (просроченного) токена
            var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email)) return null;

            // 2. Ищем пользователя
            var user = await _userManager.FindByEmailAsync(email);

            // 3. Проверяем: существует ли юзер, совпадает ли Refresh токен и не протух ли он
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            // 4. Генерируем новую пару
            var newAccessToken = _jwtProvider.GenerateToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            // 5. Обновляем данные в базе
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new LoginResponse(newAccessToken, newRefreshToken);
        }
    }
}