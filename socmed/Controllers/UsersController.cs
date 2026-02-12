using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using socmed.Dto;
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            return await _mediator.Send(new GetAllUsersQuery());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id) => Ok(await _mediator.Send(new GetUserByIdQuery(id)));
        [HttpGet("followers/{id}")]
        public async Task<ActionResult<List<UserDtoFol>>> GetFollowers(string id)
        {
            return Ok(await _mediator.Send(new GetFollowersQuery(id)));
        }

        [HttpGet("following/{id}")]
        public async Task<ActionResult<List<UserDtoFol>>> GetFollowing(string id)
        {
            return Ok(await _mediator.Send(new GetFollowingQuery(id)));
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateUserProfileRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"DEBUG: Attempting to update ID {id}. Token ID is {userId}");
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
        [HttpPost("follow/{targetId}")]
        public async Task<IActionResult> Follow(string targetId)
        {
            var result = await _mediator.Send(new FollowUserCommand(targetId));
            return result ? Ok() : BadRequest("Не удалось подписаться");
        }

        [Authorize]
        [HttpDelete("unfollow/{targetId}")]
        public async Task<IActionResult> Unfollow(string targetId)
        {
            var result = await _mediator.Send(new UnfollowUserCommand(targetId));
            return result ? Ok() : BadRequest("Вы не подписаны на этого пользователя");
        }
    }
}