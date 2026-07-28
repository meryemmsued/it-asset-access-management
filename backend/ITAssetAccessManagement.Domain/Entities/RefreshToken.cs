using ITAssetAccessManagement.Domain.Common;

namespace ITAssetAccessManagement.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    public string? RevokedByIp { get; set; }

    public string? CreatedByIp { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public User User { get; set; } = null!;

    public RefreshToken? ReplacedByToken { get; set; }
}