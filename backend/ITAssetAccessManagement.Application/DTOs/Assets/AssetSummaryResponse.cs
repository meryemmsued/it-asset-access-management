using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Application.DTOs.Assets;

public sealed class AssetSummaryResponse
{
    public Guid Id { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public AssetStatus Status { get; set; }
}