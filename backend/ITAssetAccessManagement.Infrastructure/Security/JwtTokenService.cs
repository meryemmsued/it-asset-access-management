using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ITAssetAccessManagement.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is missing.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("JWT audience is missing.");

        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is missing.");

        var expirationMinutesText = jwtSection["AccessTokenExpirationMinutes"];

        if (!int.TryParse(expirationMinutesText, out var expirationMinutes))
        {
            throw new InvalidOperationException(
                "JWT access token expiration is invalid."
            );
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        claims.AddRange(
            roles.Select(role => new Claim(ClaimTypes.Role, role))
        );

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }
}