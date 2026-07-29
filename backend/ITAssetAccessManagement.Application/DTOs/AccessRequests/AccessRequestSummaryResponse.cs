using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Application.DTOs.AccessRequests;

public sealed class AccessRequestSummaryResponse
{
    public Guid Id { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public AccessRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}