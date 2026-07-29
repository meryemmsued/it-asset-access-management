namespace ITAssetAccessManagement.Application.DTOs.AccessRequests;

public sealed class CreateAccessRequestRequest
{
    public Guid AssetId { get; set; }

    public string RequestedAccessType { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime? RequestedStartDate { get; set; }

    public DateTime? RequestedEndDate { get; set; }
}