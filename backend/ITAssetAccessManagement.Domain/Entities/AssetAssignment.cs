using ITAssetAccessManagement.Domain.Common;
using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class AssetAssignment : BaseEntity
{
    public Guid AssetId { get; set; }

    public Guid AssignedToUserId { get; set; }

    public Guid AssignedByUserId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnedAt { get; set; }

    public string? Notes { get; set; }

    public AssetAssignmentStatus Status { get; set; }
        = AssetAssignmentStatus.Active;

    public Asset Asset { get; set; } = null!;

    public User AssignedToUser { get; set; } = null!;

    public User AssignedByUser { get; set; } = null!;
}