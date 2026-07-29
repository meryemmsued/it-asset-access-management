using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class AccessRequest
{
    public Guid Id { get; set; }

    public Guid RequestedByUserId { get; set; }

    public Guid AssetId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime? RequestedStartDate { get; set; }

    public DateTime? RequestedEndDate { get; set; }

    public AccessRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User RequestedByUser { get; set; } = null!;

    public Asset Asset { get; set; } = null!;

    public string RequestedAccessType { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public ICollection<AccessRequestApproval> Approvals { get; set; }
        = new List<AccessRequestApproval>();
}