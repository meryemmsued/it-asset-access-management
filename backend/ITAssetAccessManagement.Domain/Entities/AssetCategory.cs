using ITAssetAccessManagement.Domain.Common;
using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class AssetCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public AssetType AssetType { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}