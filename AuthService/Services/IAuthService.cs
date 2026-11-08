
using AuthService.DTOs;

namespace AuthService.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);

    Task<(bool Success, string Message)> RegisterAsync(
        RegisterDto dto);

    Task<UserProfileDto?> GetCurrentUserAsync(string userId);

    Task<(bool Success, string Message)> ChangePasswordAsync(
        string userId,
        ChangePasswordDto dto);
}
