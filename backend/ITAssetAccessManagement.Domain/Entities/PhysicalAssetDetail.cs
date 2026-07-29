using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class PhysicalAssetDetail
{
    public Guid AssetId { get; set; }

    public string? SerialNumber { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? Location { get; set; }

    public PhysicalAssetCondition? Condition { get; set; }

    public Asset Asset { get; set; } = null!;
}