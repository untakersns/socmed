using MediatR;
using Microsoft.AspNetCore.Identity;
using socmed.Entity;

namespace socmed.Mediator.Handler
{
    public record UpdateUserProfileRequest(string UserName, string? Bio, DateTime? BirthDate);
    public record UpdateUserProfileCommand(string UserId, string UserName, string? Bio, DateTime? BirthDate) : IRequest<bool>;

    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateUserProfileHandler(UserManager<ApplicationUser> userManager)
            => _userManager = userManager;

        public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return false;

            user.UserName = request.UserName;
            user.Bio = request.Bio;
            user.BirthDate = request.BirthDate;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}