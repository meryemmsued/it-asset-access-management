using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITAssetAccessManagement.Persistence.Configurations;

public sealed class AccessRequestApprovalConfiguration
    : IEntityTypeConfiguration<AccessRequestApproval>
{
    public void Configure(
        EntityTypeBuilder<AccessRequestApproval> builder)
    {
        builder.ToTable("access_request_approvals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccessRequestId)
            .HasColumnName("access_request_id");

        builder.Property(x => x.ApproverUserId)
            .HasColumnName("approver_user_id");

        builder.Property(x => x.ApprovalOrder)
            .HasColumnName("approval_order");

        builder.Property(x => x.Decision)
            .HasColumnName("decision")
            .HasConversion(
                value => value.ToString().ToUpper(),
                value => Enum.Parse<ApprovalDecision>(value, true));

        builder.Property(x => x.Comment)
            .HasColumnName("decision_note")
            .HasMaxLength(1000);

        builder.Property(x => x.DecidedAt)
            .HasColumnName("decided_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.HasOne(x => x.AccessRequest)
            .WithMany(x => x.Approvals)
            .HasForeignKey(x => x.AccessRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ApproverUser)
            .WithMany(x => x.AccessRequestApprovals)
            .HasForeignKey(x => x.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}