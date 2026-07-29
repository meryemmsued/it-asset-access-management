using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");

        builder.HasKey(asset => asset.Id);

        builder.Property(asset => asset.Id)
            .HasColumnName("id");

        builder.Property(asset => asset.AssetCategoryId)
            .HasColumnName("asset_category_id")
            .IsRequired();

        builder.Property(asset => asset.AssetCode)
            .HasColumnName("asset_code")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(asset => asset.AssetCode)
            .IsUnique();

        builder.Property(asset => asset.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(asset => asset.Description)
            .HasColumnName("description");

        builder.Property(asset => asset.Status)
            .HasColumnName("status")
            .HasConversion(
                status => ToDatabaseValue(status),
                value => FromDatabaseValue(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(asset => asset.PurchaseDate)
            .HasColumnName("purchase_date");

        builder.Property(asset => asset.PurchasePrice)
            .HasColumnName("purchase_price")
            .HasPrecision(18, 2);

        builder.Property(asset => asset.WarrantyExpirationDate)
            .HasColumnName("warranty_expiration_date");

        builder.Property(asset => asset.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(asset => asset.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(asset => asset.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(asset => asset.AssetCategory)
            .WithMany(category => category.Assets)
            .HasForeignKey(asset => asset.AssetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(asset => asset.CreatedByUser)
            .WithMany(user => user.CreatedAssets)
            .HasForeignKey(asset => asset.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(asset => asset.PhysicalDetail)
            .WithOne(detail => detail.Asset)
            .HasForeignKey<PhysicalAssetDetail>(detail => detail.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(asset => asset.DigitalDetail)
            .WithOne(detail => detail.Asset)
            .HasForeignKey<DigitalAssetDetail>(detail => detail.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(asset => asset.Assignments)
            .WithOne(assignment => assignment.Asset)
            .HasForeignKey(assignment => assignment.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(asset => asset.StatusHistories)
            .WithOne(history => history.Asset)
            .HasForeignKey(history => history.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string ToDatabaseValue(AssetStatus status)
    {
        return status switch
        {
            AssetStatus.Available => "AVAILABLE",
            AssetStatus.Assigned => "ASSIGNED",
            AssetStatus.InRepair => "IN_REPAIR",
            AssetStatus.Retired => "RETIRED",
            AssetStatus.Lost => "LOST",
            AssetStatus.Decommissioned => "DECOMMISSIONED",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported asset status.")
        };
    }

    private static AssetStatus FromDatabaseValue(string value)
    {
        return value switch
        {
            "AVAILABLE" => AssetStatus.Available,
            "ASSIGNED" => AssetStatus.Assigned,
            "IN_REPAIR" => AssetStatus.InRepair,
            "RETIRED" => AssetStatus.Retired,
            "LOST" => AssetStatus.Lost,
            "DECOMMISSIONED" => AssetStatus.Decommissioned,
            _ => throw new InvalidOperationException(
                $"Unknown asset status: {value}")
        };
    }
}