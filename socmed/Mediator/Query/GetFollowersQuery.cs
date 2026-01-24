using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetFollowersQuery(string UserId) : IRequest<List<UserDto>>;
    public record UserDto(string Id, string UserName);
    public class GetUserFollowersHandler : IRequestHandler<GetFollowersQuery, List<UserDto>>
    {
        private readonly SMDbContext _context;
        public GetUserFollowersHandler(SMDbContext context) => _context = context;

        public async Task<List<UserDto>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserFollowers
                .Where(f => f.TargetId == request.UserId)
                .Select(f => new UserDto(f.Follower.Id, f.Follower.UserName))
                .ToListAsync(cancellationToken);
        }
    }
}
