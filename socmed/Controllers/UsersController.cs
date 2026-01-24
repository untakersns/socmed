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

        [HttpGet("{id}/followers")]
        [HttpGet("{id}/following")]
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

        [Authorize]
        [HttpPost("/follow/{targetId}")]
        public async Task<IActionResult> Follow(string targetId)
        {
            var result = await _mediator.Send(new FollowUserCommand(targetId));
            return result ? Ok() : BadRequest("Не удалось подписаться");
        }

        [Authorize]
        [HttpDelete("/unfollow/{targetId}")]
        public async Task<IActionResult> Unfollow(string targetId)
        {
            var result = await _mediator.Send(new UnfollowUserCommand(targetId));
            return result ? Ok() : BadRequest("Вы не подписаны на этого пользователя");
        }

        [HttpGet("/followers/{id}")]
        public async Task<ActionResult<List<UserDto>>> GetFollowers(string id)
        {
            return Ok(await _mediator.Send(new GetFollowersQuery(id)));
        }

        [HttpGet("/following/{id}")]
        public async Task<ActionResult<List<UserDto>>> GetFollowing(string id)
        {
            return Ok(await _mediator.Send(new GetFollowingQuery(id)));
        }
    }
}