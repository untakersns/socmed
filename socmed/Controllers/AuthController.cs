using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed.Mediator.Handler;
using socmed.Mediator.Query;
using System.Security.Claims;

namespace socmed.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")] public async Task<IActionResult> Register([FromBody] RegisterCommand cmd) => Ok(await _mediator.Send(cmd));
        [HttpPost("login")] public async Task<IActionResult> Login([FromBody] LoginCommand cmd) => Ok(await _mediator.Send(cmd));
        [HttpPost("refresh-token")] public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand cmd) => Ok(await _mediator.Send(cmd));

        [Authorize]
        [HttpPost("logout")] public async Task<IActionResult> Logout() => Ok(await _mediator.Send(new LogoutCommand()));
    }
}
