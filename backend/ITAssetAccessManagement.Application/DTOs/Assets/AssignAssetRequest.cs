namespace ITAssetAccessManagement.Application.DTOs.Assets;

public sealed class AssignAssetRequest
{
    public Guid AssignedToUserId { get; set; }

    public string? Notes { get; set; }
}