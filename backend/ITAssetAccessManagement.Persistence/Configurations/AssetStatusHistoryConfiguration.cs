using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class AssetStatusHistoryConfiguration
    : IEntityTypeConfiguration<AssetStatusHistory>
{
    public void Configure(EntityTypeBuilder<AssetStatusHistory> builder)
    {
        builder.ToTable("asset_status_histories");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id)
            .HasColumnName("id");

        builder.Property(history => history.AssetId)
            .HasColumnName("asset_id")
            .IsRequired();

        builder.Property(history => history.OldStatus)
            .HasColumnName("old_status")
            .HasConversion(
                status => ToNullableDatabaseValue(status),
                value => FromNullableDatabaseValue(value))
            .HasMaxLength(30);

        builder.Property(history => history.NewStatus)
            .HasColumnName("new_status")
            .HasConversion(
                status => ToDatabaseValue(status),
                value => FromDatabaseValue(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(history => history.ChangedByUserId)
            .HasColumnName("changed_by_user_id")
            .IsRequired();

        builder.Property(history => history.ChangeReason)
            .HasColumnName("change_reason");

        builder.Property(history => history.ChangedAt)
            .HasColumnName("changed_at")
            .IsRequired();

        builder.HasOne(history => history.Asset)
            .WithMany(asset => asset.StatusHistories)
            .HasForeignKey(history => history.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(history => history.ChangedByUser)
            .WithMany(user => user.AssetStatusChanges)
            .HasForeignKey(history => history.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => history.AssetId);

        builder.HasIndex(history => history.ChangedAt);
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

    private static string? ToNullableDatabaseValue(AssetStatus? status)
    {
        return status.HasValue
            ? ToDatabaseValue(status.Value)
            : null;
    }

    private static AssetStatus? FromNullableDatabaseValue(string? value)
    {
        return value is null
            ? null
            : FromDatabaseValue(value);
    }
}