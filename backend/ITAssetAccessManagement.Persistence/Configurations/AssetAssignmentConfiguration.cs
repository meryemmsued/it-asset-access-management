using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class AssetAssignmentConfiguration
    : IEntityTypeConfiguration<AssetAssignment>
{
    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.ToTable("asset_assignments");

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.Id)
            .HasColumnName("id");

        builder.Property(assignment => assignment.AssetId)
            .HasColumnName("asset_id")
            .IsRequired();

        builder.Property(assignment => assignment.AssignedToUserId)
            .HasColumnName("assigned_to_user_id")
            .IsRequired();

        builder.Property(assignment => assignment.AssignedByUserId)
            .HasColumnName("assigned_by_user_id")
            .IsRequired();

        builder.Property(assignment => assignment.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.Property(assignment => assignment.ReturnedAt)
            .HasColumnName("returned_at");

        builder.Property(assignment => assignment.Notes)
            .HasColumnName("notes");

        builder.Property(assignment => assignment.Status)
            .HasColumnName("status")
            .HasConversion(
                status => ToDatabaseValue(status),
                value => FromDatabaseValue(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(assignment => assignment.Asset)
            .WithMany(asset => asset.Assignments)
            .HasForeignKey(assignment => assignment.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.AssignedToUser)
            .WithMany(user => user.AssetAssignments)
            .HasForeignKey(assignment => assignment.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.AssignedByUser)
            .WithMany(user => user.AssignedAssets)
            .HasForeignKey(assignment => assignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(assignment => assignment.AssetId);

        builder.HasIndex(assignment => assignment.AssignedToUserId);
    }

    private static string ToDatabaseValue(
        AssetAssignmentStatus status)
    {
        return status switch
        {
            AssetAssignmentStatus.Active => "ACTIVE",
            AssetAssignmentStatus.Returned => "RETURNED",
            AssetAssignmentStatus.Lost => "LOST",
            AssetAssignmentStatus.Damaged => "DAMAGED",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported asset assignment status.")
        };
    }

    private static AssetAssignmentStatus FromDatabaseValue(string value)
    {
        return value switch
        {
            "ACTIVE" => AssetAssignmentStatus.Active,
            "RETURNED" => AssetAssignmentStatus.Returned,
            "LOST" => AssetAssignmentStatus.Lost,
            "DAMAGED" => AssetAssignmentStatus.Damaged,
            _ => throw new InvalidOperationException(
                $"Unknown asset assignment status: {value}")
        };
    }
}