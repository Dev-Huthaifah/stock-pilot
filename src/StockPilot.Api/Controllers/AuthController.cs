
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.Auth;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-temporary-password")]
    public async Task<IActionResult> ChangeTemporaryPassword(
        [FromBody] ChangeTemporaryPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var tokenUse = User.FindFirst("token_use")?.Value;

        if (tokenUse != "password_change")
        {
            return Forbid();
        }

        var userIdValue = User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangeTemporaryPasswordAsync(
            userId,
            request,
            cancellationToken);

        if (result is null)
        {
            return BadRequest(new
            {
                message = "Password change failed. Check your current password and requirements."
            });
        }

        return Ok(result);
    }
}
