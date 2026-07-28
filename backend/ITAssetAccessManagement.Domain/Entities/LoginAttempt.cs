using ITAssetAccessManagement.Domain.Common;

namespace ITAssetAccessManagement.Domain.Entities;

public class LoginAttempt : BaseEntity
{
    public Guid? UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsSuccessful { get; set; }

    public string? FailureReason { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}