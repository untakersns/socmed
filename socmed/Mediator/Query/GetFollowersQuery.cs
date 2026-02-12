using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetFollowersQuery(string UserId) : IRequest<List<UserDtoFol>>;
    public record UserDtoFol(string Id, string UserName);

    public class GetUserFollowersHandler : IRequestHandler<GetFollowersQuery, List<UserDtoFol>>
    {
        private readonly SMDbContext _context;

        public GetUserFollowersHandler(SMDbContext context) => _context = context;

        public async Task<List<UserDtoFol>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserFollowers
                .Where(f => f.TargetId == request.UserId)
                .Select(f => new UserDtoFol(f.Follower.Id, f.Follower.UserName))
                .ToListAsync(cancellationToken);
        }
    }
}