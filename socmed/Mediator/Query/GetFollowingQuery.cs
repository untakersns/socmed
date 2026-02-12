using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetFollowingQuery(string UserId) : IRequest<List<UserDtoFol>>;

    public class GetFollowingHandler : IRequestHandler<GetFollowingQuery, List<UserDtoFol>>
    {
        private readonly SMDbContext _context;

        public GetFollowingHandler(SMDbContext context) => _context = context;

        public async Task<List<UserDtoFol>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserFollowers
                .Where(f => f.FollowerId == request.UserId)
                .Select(f => new UserDtoFol(f.Target.Id, f.Target.UserName!))
                .ToListAsync();
        }
    }
}