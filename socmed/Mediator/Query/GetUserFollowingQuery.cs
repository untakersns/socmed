using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetUserFollowingQuery(string UserId) : IRequest<List<UserDto>>;
    public class GetUserFollowingHandler : IRequestHandler<GetUserFollowingQuery, List<UserDto>>
    {
        private readonly SMDbContext _context;

        public async Task<List<UserDto>> Handle(GetUserFollowingQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserFollowers
                .Where(f => f.ObserverId == request.UserId)
                .Select(f => new UserDto(f.Target.Id, f.Target.UserName, f.Target.Bio))
                .ToListAsync(cancellationToken);
        }
    }
}
