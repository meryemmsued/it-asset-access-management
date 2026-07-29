using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class AssetCategoryConfiguration
    : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.ToTable("asset_categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Description);

        builder.Property(x => x.AssetType)
            .HasConversion(
                value => value == AssetType.Physical
                    ? "PHYSICAL"
                    : "DIGITAL",
                value => value == "PHYSICAL"
                    ? AssetType.Physical
                    : AssetType.Digital)
            .HasColumnName("asset_type");

        builder.HasMany(x => x.Assets)
            .WithOne(x => x.AssetCategory)
            .HasForeignKey(x => x.AssetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}