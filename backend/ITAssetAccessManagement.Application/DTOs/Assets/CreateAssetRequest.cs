using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Application.DTOs.Assets;

public sealed class CreateAssetRequest
{
    public Guid AssetCategoryId { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public decimal? PurchasePrice { get; set; }

    public DateOnly? WarrantyExpirationDate { get; set; }

    // Physical
    public string? SerialNumber { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? Location { get; set; }

    public PhysicalAssetCondition? Condition { get; set; }

    // Digital
    public string? LicenseKey { get; set; }

    public string? Version { get; set; }

    public LicenseType? LicenseType { get; set; }

    public DateOnly? LicenseStartDate { get; set; }

    public DateOnly? LicenseExpirationDate { get; set; }

    public string RequestedAccessType { get; set; } = string.Empty;

    public int? MaximumUsers { get; set; }
}