using MediatR;
using Microsoft.EntityFrameworkCore;
using socmed.Data;
using socmed.Dto;
namespace socmed.Mediator.Query
{
    public record GetAllUsersQuery() : IRequest<List<UserDto>>;
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly SMDbContext _context;
        public GetAllUsersHandler(SMDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Users 
                .Select(u => new UserDto(u.Id, u.UserName, u.Email, u.Bio)
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Bio = u.Bio
                })
                .ToListAsync(cancellationToken);
        }
    }
}
