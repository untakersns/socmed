using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetUserByIdQuery(string Id) : IRequest<UserProfileResponse?>;
    public record UserProfileResponse(string Id, string UserName, string? Bio, int FollowersCount, int FollowingCount);

    public class GetUserProfileHandler : IRequestHandler<GetUserByIdQuery, UserProfileResponse?>
    {
        private readonly SMDbContext _context;

        public GetUserProfileHandler(SMDbContext context) => _context = context;

        public async Task<UserProfileResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.Id == request.Id)
                .Select(u => new UserProfileResponse(
                    u.Id,
                    u.UserName!,
                    u.Bio,
                    u.Followers.Count,
                    u.Following.Count))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}