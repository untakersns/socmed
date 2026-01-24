using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;
using System.Security.Claims;

namespace socmed.Mediator.Handler
{
    public record LogoutCommand() : IRequest<bool>;

    public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoutHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}