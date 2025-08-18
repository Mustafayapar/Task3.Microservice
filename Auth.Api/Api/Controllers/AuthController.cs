using Auth.Api.Application.Commands;
using Auth.Api.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        // REgister endpoint
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
            ), cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result.Error);
           

            return Ok("User registered successfully");
        }

        public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
        //
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
            => Ok(await _mediator.Send(cmd));


        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
            if (!result.Succeeded)
                return Unauthorized(result.Error);

            return Ok(new { AccessToken = result.AccessToken, RefreshToken = result.RefreshToken });
        }

        public record RefreshRequest(string RefreshToken);
        [Authorize] // sadece giriş yapmış kullanıcı logout olabilir
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _mediator.Send(new RevokeRefreshTokenCommand(Guid.Parse(userId)), cancellationToken);
            if (!result)
                return BadRequest("Logout failed");

            return Ok("Logged out successfully");
        }

       

    }
}
