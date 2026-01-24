using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;
using System.Security.Claims;

namespace socmed.Mediator.Handler
{
    public record UnfollowUserCommand(string TargetId) : IRequest<bool>;

    public class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand, bool>
    {
        private readonly SMDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnfollowUserHandler(SMDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return false;

            var followRecord = await _context.UserFollowers
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.TargetId == request.TargetId);

            if (followRecord == null) return false;

            _context.UserFollowers.Remove(followRecord);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}