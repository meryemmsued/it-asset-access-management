using ITAssetAccessManagement.Application.DTOs.Auth;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                user => user.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            _dbContext.LoginAttempts.Add(new LoginAttempt
            {
                UserId = null,
                Email = normalizedEmail,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = false,
                FailureReason = "User not found",
                AttemptedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (!user.IsActive)
        {
            _dbContext.LoginAttempts.Add(new LoginAttempt
            {
                UserId = user.Id,
                Email = normalizedEmail,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = false,
                FailureReason = "User is inactive",
                AttemptedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            _dbContext.LoginAttempts.Add(new LoginAttempt
            {
                UserId = user.Id,
                Email = normalizedEmail,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = false,
                FailureReason = "Invalid password",
                AttemptedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToList();

        var accessToken = _tokenService.GenerateAccessToken(user, roles);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

        var now = DateTime.UtcNow;

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = now.AddDays(7),
            CreatedByIp = ipAddress
        };

        var loginAttempt = new LoginAttempt
        {
            UserId = user.Id,
            Email = normalizedEmail,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = true,
            FailureReason = null,
            AttemptedAt = now
        };

        user.LastLoginAt = now;

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        _dbContext.LoginAttempts.Add(loginAttempt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = now.AddHours(1),
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }

    public async Task<LoginResponse?> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _dbContext.Users.AnyAsync(
            user => user.Email.ToLower() == normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            return null;
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var loginRequest = new LoginRequest
        {
            Email = normalizedEmail,
            Password = request.Password
        };

        return await LoginAsync(
            loginRequest,
            ipAddress,
            userAgent,
            cancellationToken);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return null;
        }

        var refreshTokenHash = _tokenService.HashRefreshToken(
            request.RefreshToken);

        var existingRefreshToken = await _dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .ThenInclude(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == refreshTokenHash,
                cancellationToken);

        if (existingRefreshToken is null)
        {
            return null;
        }

        if (existingRefreshToken.RevokedAt is not null)
        {
            return null;
        }

        if (existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        if (!existingRefreshToken.User.IsActive)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        existingRefreshToken.RevokedAt = now;
        existingRefreshToken.RevokedByIp = ipAddress;

        var roles = existingRefreshToken.User.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToList();

        var newAccessToken = _tokenService.GenerateAccessToken(
            existingRefreshToken.User,
            roles);

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(
            newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = existingRefreshToken.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = now.AddDays(7),
            CreatedByIp = ipAddress
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        existingRefreshToken.ReplacedByTokenId =
            newRefreshTokenEntity.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = now.AddHours(1),
            RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt
        };
    }


    public async Task LogoutAsync(
        LogoutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var refreshTokenHash =
            _tokenService.HashRefreshToken(request.RefreshToken);

        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == refreshTokenHash,
                cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        if (refreshToken.RevokedAt is not null)
        {
            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}