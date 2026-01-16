using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;

namespace socmed.Mediator.Query
{
    public record GetUserFollowersQuery(string UserId) : IRequest<List<UserDto>>;
    public record UserDto(string Id, string UserName, string? Bio);
    public class GetUserFollowersHandler : IRequestHandler<GetUserFollowersQuery, List<UserDto>>
    {
        private readonly SMDbContext _context;
        public GetUserFollowersHandler(SMDbContext context) => _context = context;

        public async Task<List<UserDto>> Handle(GetUserFollowersQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserFollowers
                .Where(f => f.TargetId == request.UserId)
                .Select(f => new UserDto(f.Observer.Id, f.Observer.UserName, f.Observer.Bio))
                .ToListAsync(cancellationToken);
        }
    }
}
