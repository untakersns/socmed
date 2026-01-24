using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetFollowingQuery(string UserId) : IRequest<List<UserDto>>;

    public class GetFollowingHandler : IRequestHandler<GetFollowingQuery, List<UserDto>>
    {
        private readonly SMDbContext _context;

        public GetFollowingHandler(SMDbContext context) => _context = context;

        public async Task<List<UserDto>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserFollowers
                .Where(f => f.FollowerId == request.UserId)
                .Select(f => new UserDto(f.Target.Id, f.Target.UserName!))
                .ToListAsync();
        }
    }
}
