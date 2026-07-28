using ITAssetAccessManagement.Domain.Entities;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}