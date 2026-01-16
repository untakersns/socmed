using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed.Mediator.Handler;
using socmed.Mediator.Query;
using System.Security.Claims;

namespace socmed.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id) => Ok(await _mediator.Send(new GetUserByIdQuery(id)));

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateUserProfileRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != currentUserId) return Forbid();
            var command = new UpdateUserProfileCommand(
                id,
                request.UserName,
                request.Bio,
                request.BirthDate
            );
            var result = await _mediator.Send(command);
            return result ? NoContent() : BadRequest();
        }

        [HttpGet("{id}/followers")]
        public async Task<IActionResult> GetFollowers(string id) => Ok(await _mediator.Send(new GetUserFollowersQuery(id)));

        [HttpGet("{id}/following")]
        public async Task<IActionResult> GetFollowing(string id) => Ok(await _mediator.Send(new GetUserFollowingQuery(id)));
    }
}
