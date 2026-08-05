namespace ITAssetAccessManagement.Application.DTOs.AssetCategories;

public sealed class AssetCategoryResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AssetType { get; set; } = string.Empty;
}