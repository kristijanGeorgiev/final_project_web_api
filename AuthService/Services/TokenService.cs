
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(
        ApplicationUser user,
        IList<string> roles,
        out DateTime expiresAt)
    {
        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience missing.");

        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key missing.");

        if (Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException(
                "JWT signing key must be at least 32 bytes.");

        int minutes = _configuration.GetValue(
            "Jwt:ExpirationMinutes", 15);

        if (minutes < 1 || minutes > 60)
            throw new InvalidOperationException(
                "JWT lifetime must be between 1 and 60 minutes.");

        var now = DateTime.UtcNow;
        expiresAt = now.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("fullName", user.FullName),
            new("securityStamp",
                user.SecurityStamp ?? "")
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}