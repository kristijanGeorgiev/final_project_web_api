
using AuthService.DTOs;
using AuthService.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);

        if (user is null)
        {
            // Avoid exposing whether a username exists.
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        var token = _tokenService.GenerateAccessToken(
            user,
            roles,
            out var expiresAt);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            FullName = user.FullName,
            Roles = roles
        };
    }

    public async Task<(bool Success, string Message)> RegisterAsync(
        RegisterDto dto)
    {
        if (dto.Role is not ("Admin" or "User"))
            return (false, "Invalid role.");

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return (false, "Role is not configured.");

        if (await _userManager.FindByNameAsync(dto.Username) is not null)
            return (false, "Username already exists.");

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return (false, "Email already exists.");

        var user = new ApplicationUser
        {
            FullName = dto.FullName.Trim(),
            UserName = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(
            user,
            dto.Password);

        if (!createResult.Succeeded)
            return (false, FormatErrors(createResult));

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            dto.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return (false, FormatErrors(roleResult));
        }

        return (true, "User registered successfully.");
    }

    public async Task<UserProfileDto?> GetCurrentUserAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            UserId = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            FullName = user.FullName,
            Roles = roles
        };
    }

    public async Task<(bool Success, string Message)>
        ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return (false, "User not found.");

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword);

        if (!result.Succeeded)
            return (false, FormatErrors(result));

        return (true, "Password changed successfully.");
    }

    private static string FormatErrors(IdentityResult result)
    {
        return string.Join(
            "; ",
            result.Errors.Select(e => e.Description));
    }
}
