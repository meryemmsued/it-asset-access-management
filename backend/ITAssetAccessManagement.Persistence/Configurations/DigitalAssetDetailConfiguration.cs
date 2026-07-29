using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class DigitalAssetDetailConfiguration
    : IEntityTypeConfiguration<DigitalAssetDetail>
{
    public void Configure(EntityTypeBuilder<DigitalAssetDetail> builder)
    {
        builder.ToTable("digital_asset_details");

        builder.HasKey(detail => detail.AssetId);

        builder.Property(detail => detail.AssetId)
            .HasColumnName("asset_id")
            .ValueGeneratedNever();

        builder.Property(detail => detail.LicenseKey)
            .HasColumnName("license_key")
            .HasMaxLength(500);

        builder.Property(detail => detail.Version)
            .HasColumnName("version")
            .HasMaxLength(100);

        builder.Property(detail => detail.LicenseType)
            .HasColumnName("license_type")
            .HasConversion(
                licenseType => ToDatabaseValue(licenseType),
                value => FromDatabaseValue(value))
            .HasMaxLength(30);

        builder.Property(detail => detail.LicenseStartDate)
            .HasColumnName("license_start_date");

        builder.Property(detail => detail.LicenseExpirationDate)
            .HasColumnName("license_expiration_date");

        builder.Property(detail => detail.MaximumUsers)
            .HasColumnName("maximum_users");

        builder.HasOne(detail => detail.Asset)
            .WithOne(asset => asset.DigitalDetail)
            .HasForeignKey<DigitalAssetDetail>(detail => detail.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string? ToDatabaseValue(LicenseType? licenseType)
    {
        return licenseType switch
        {
            null => null,
            LicenseType.Perpetual => "PERPETUAL",
            LicenseType.Subscription => "SUBSCRIPTION",
            LicenseType.Trial => "TRIAL",
            LicenseType.OpenSource => "OPEN_SOURCE",
            _ => throw new ArgumentOutOfRangeException(
                nameof(licenseType),
                licenseType,
                "Unsupported license type.")
        };
    }

    private static LicenseType? FromDatabaseValue(string? value)
    {
        return value switch
        {
            null => null,
            "PERPETUAL" => LicenseType.Perpetual,
            "SUBSCRIPTION" => LicenseType.Subscription,
            "TRIAL" => LicenseType.Trial,
            "OPEN_SOURCE" => LicenseType.OpenSource,
            _ => throw new InvalidOperationException(
                $"Unknown license type: {value}")
        };
    }
}