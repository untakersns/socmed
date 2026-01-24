using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;
using socmed.Entity;
using System.Security.Claims;

namespace socmed.Mediator.Handler
{
    public record FollowUserCommand(string TargetId) : IRequest<bool>;

    public class FollowUserHandler : IRequestHandler<FollowUserCommand, bool>
    {
        private readonly SMDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FollowUserHandler(SMDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(FollowUserCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || currentUserId == request.TargetId) return false;

            // Проверяем, существует ли тот, на кого подписываемся
            var targetUser = await _context.Users.AnyAsync(u => u.Id == request.TargetId);
            if (!targetUser) return false;

            // Проверяем, нет ли уже подписки
            var isAlreadyFollowing = await _context.UserFollowers
                .AnyAsync(f => f.FollowerId == currentUserId && f.TargetId == request.TargetId);

            if (isAlreadyFollowing) return true;

            _context.UserFollowers.Add(new UserFollower { FollowerId = currentUserId, TargetId = request.TargetId });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
