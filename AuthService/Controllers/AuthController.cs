
using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ============================================================
    // LOGIN
    // POST: api/auth/login
    // PUBLIC ENDPOINT
    // ============================================================

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _authService.LoginAsync(dto);

        if (response is null)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials or account unavailable."
            });
        }

        return Ok(response);
    }

    // ============================================================
    // REGISTER USER
    // POST: api/auth/register
    // ADMIN ONLY
    // ============================================================

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message
        });
    }

    // ============================================================
    // CURRENT USER PROFILE
    // GET: api/auth/me
    // AUTHENTICATED USERS
    // ============================================================

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "Invalid or missing user identity."
            });
        }

        var profile = await _authService.GetCurrentUserAsync(
            userId);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(profile);
    }

    // ============================================================
    // CHANGE PASSWORD
    // POST: api/auth/change-password
    // AUTHENTICATED USERS
    // ============================================================

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "Invalid or missing user identity."
            });
        }

        var result = await _authService.ChangePasswordAsync(
            userId,
            dto);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message
        });
    }
}