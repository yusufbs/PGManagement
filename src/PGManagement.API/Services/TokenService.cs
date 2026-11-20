using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PGManagement.Contracts.Responses.Auth;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.API.Services;

public interface ITokenService
{
    AuthResponse GenerateToken(ApplicationUser user);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponse GenerateToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "SUPER_SECRET_KEY_FOR_DEVELOPMENT_PURPOSES_MIN_32_CHARS!";
        var issuer = jwtSettings["Issuer"] ?? "PGManagementAPI";
        var audience = jwtSettings["Audience"] ?? "PGManagementClient";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", $"{user.FirstName} {user.LastName}")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new AuthResponse(
            Token: tokenString,
            RefreshToken: Guid.NewGuid().ToString("N"),
            ExpiresAt: expiresAt,
            Email: user.Email ?? string.Empty,
            FullName: $"{user.FirstName} {user.LastName}"
        );
    }
}
