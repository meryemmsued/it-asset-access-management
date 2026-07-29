using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class DigitalAssetDetail
{
    public Guid AssetId { get; set; }

    public string? LicenseKey { get; set; }

    public string? Version { get; set; }

    public LicenseType? LicenseType { get; set; }

    public DateOnly? LicenseStartDate { get; set; }

    public DateOnly? LicenseExpirationDate { get; set; }

    public int? MaximumUsers { get; set; }

    public Asset Asset { get; set; } = null!;
}