using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class AccessRequestConfiguration
    : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.ToTable("access_requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestedByUserId)
            .HasColumnName("requester_user_id");

        builder.Property(x => x.AssetId)
            .HasColumnName("asset_id");

        builder.Property(x => x.RequestedAccessType)
            .HasColumnName("requested_access_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.RequestedStartDate)
            .HasColumnName("requested_start_date");

        builder.Property(x => x.RequestedEndDate)
            .HasColumnName("requested_end_date");

        builder.Property(x => x.RequestedAt)
            .HasColumnName("requested_at");

        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(
                value => value.ToString().ToUpper(),
                value => Enum.Parse<AccessRequestStatus>(value, true));

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(x => x.RequestedByUser)
            .WithMany(x => x.AccessRequests)
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.AccessRequests)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}