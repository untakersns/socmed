using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;

namespace socmed.Mediator.Handler
{
    public record RegisterCommand(string Email, string Password) : IRequest<bool>;

    public class RegisterHandler : IRequestHandler<RegisterCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);
            return result.Succeeded;
        }
    }
}