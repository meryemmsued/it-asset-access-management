using ITAssetAccessManagement.Domain.Common;
using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class AssetStatusHistory : BaseEntity
{
    public Guid AssetId { get; set; }

    public AssetStatus? OldStatus { get; set; }

    public AssetStatus NewStatus { get; set; }

    public Guid ChangedByUserId { get; set; }

    public string? ChangeReason { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Asset Asset { get; set; } = null!;

    public User ChangedByUser { get; set; } = null!;
}