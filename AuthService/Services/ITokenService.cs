
using AuthService.Entities;

namespace AuthService.Services;

public interface ITokenService
{
    string GenerateAccessToken(
        ApplicationUser user,
        IList<string> roles,
        out DateTime expiresAt);
}
