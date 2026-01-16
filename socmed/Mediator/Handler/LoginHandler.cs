using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;
using socmed.Services;

namespace socmed.Mediator.Handler
{
    public record LoginCommand(string Email, string Password) : IRequest<string?>;
    public class LoginHandler:IRequestHandler<LoginCommand,string?>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;

        public LoginHandler(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
        }

        public async Task<string?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid) return null;

            return _jwtProvider.GenerateToken(user);
        }
    }
    
}
