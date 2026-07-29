using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class AccessRequestApproval
{
    public Guid Id { get; set; }

    public Guid AccessRequestId { get; set; }

    public Guid ApproverUserId { get; set; }

    public int ApprovalOrder { get; set; }

    public ApprovalDecision Decision { get; set; }

    public string? Comment { get; set; }

    public DateTime? DecidedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public AccessRequest AccessRequest { get; set; } = null!;

    public User ApproverUser { get; set; } = null!;

}