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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // Register endpoint
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Register attempt for {Email}", request.Email);

            var result = await _mediator.Send(new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
            ), cancellationToken);

            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for {Email}. Reason: {Error}", request.Email, result.Error);
                return BadRequest(result.Error);
            }

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok("User registered successfully");
        }

        public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
        {
            _logger.LogInformation("Login attempt for {Email}", cmd.Email);
            var result = await _mediator.Send(cmd);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for {Email}", cmd.Email);
                return Unauthorized("Invalid credentials");
            }

            _logger.LogInformation("Login successful for {Email}", cmd.Email);
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refresh token attempt");

            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Refresh token failed: {Error}", result.Error);
                return Unauthorized(result.Error);
            }

            _logger.LogInformation("Refresh token successful");
            return Ok(new { AccessToken = result.AccessToken, RefreshToken = result.RefreshToken });
        }

        public record RefreshRequest(string RefreshToken);

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Logout attempt failed: No user id found in claims");
                return Unauthorized();
            }

            _logger.LogInformation("Logout attempt for UserId {UserId}", userId);
            var result = await _mediator.Send(new RevokeRefreshTokenCommand(Guid.Parse(userId)), cancellationToken);
            if (!result)
            {
                _logger.LogError("Logout failed for UserId {UserId}", userId);
                return BadRequest("Logout failed");
            }

            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return Ok("Logged out successfully");
        }
    


}
}
