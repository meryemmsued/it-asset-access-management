using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class PhysicalAssetDetailConfiguration
    : IEntityTypeConfiguration<PhysicalAssetDetail>
{
    public void Configure(EntityTypeBuilder<PhysicalAssetDetail> builder)
    {
        builder.ToTable("physical_asset_details");

        builder.HasKey(detail => detail.AssetId);

        builder.Property(detail => detail.AssetId)
            .HasColumnName("asset_id")
            .ValueGeneratedNever();

        builder.Property(detail => detail.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(200);

        builder.HasIndex(detail => detail.SerialNumber)
            .IsUnique();

        builder.Property(detail => detail.Manufacturer)
            .HasColumnName("manufacturer")
            .HasMaxLength(150);

        builder.Property(detail => detail.Model)
            .HasColumnName("model")
            .HasMaxLength(150);

        builder.Property(detail => detail.Location)
            .HasColumnName("location")
            .HasMaxLength(250);

        builder.Property(detail => detail.Condition)
            .HasColumnName("condition")
            .HasConversion(
                condition => ToDatabaseValue(condition),
                value => FromDatabaseValue(value))
            .HasMaxLength(20);

        builder.HasOne(detail => detail.Asset)
            .WithOne(asset => asset.PhysicalDetail)
            .HasForeignKey<PhysicalAssetDetail>(detail => detail.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string? ToDatabaseValue(
        PhysicalAssetCondition? condition)
    {
        return condition switch
        {
            null => null,
            PhysicalAssetCondition.New => "NEW",
            PhysicalAssetCondition.Good => "GOOD",
            PhysicalAssetCondition.Fair => "FAIR",
            PhysicalAssetCondition.Poor => "POOR",
            PhysicalAssetCondition.Damaged => "DAMAGED",
            _ => throw new ArgumentOutOfRangeException(
                nameof(condition),
                condition,
                "Unsupported physical asset condition.")
        };
    }

    private static PhysicalAssetCondition? FromDatabaseValue(string? value)
    {
        return value switch
        {
            null => null,
            "NEW" => PhysicalAssetCondition.New,
            "GOOD" => PhysicalAssetCondition.Good,
            "FAIR" => PhysicalAssetCondition.Fair,
            "POOR" => PhysicalAssetCondition.Poor,
            "DAMAGED" => PhysicalAssetCondition.Damaged,
            _ => throw new InvalidOperationException(
                $"Unknown physical asset condition: {value}")
        };
    }
}