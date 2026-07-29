using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Application.DTOs.AccessRequests;

public sealed class AccessRequestResponse
{
    public Guid Id { get; set; }

    public Guid AssetId { get; set; }

    public string AssetName { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime? RequestedStartDate { get; set; }

    public DateTime? RequestedEndDate { get; set; }

    public AccessRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ApprovalComment { get; set; }

    public DateTime? DecidedAt { get; set; }
}