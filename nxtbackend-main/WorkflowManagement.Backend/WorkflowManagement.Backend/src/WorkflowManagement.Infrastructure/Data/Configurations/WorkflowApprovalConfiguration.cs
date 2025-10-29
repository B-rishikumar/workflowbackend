// WorkflowApprovalConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowApprovalConfiguration : IEntityTypeConfiguration<WorkflowApproval>
{
    public void Configure(EntityTypeBuilder<WorkflowApproval> builder)
    {
        builder.ToTable("WorkflowApprovals");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(w => w.RequestReason)
            .HasMaxLength(1000);

        builder.Property(w => w.ApprovalComment)
            .HasMaxLength(1000);

        builder.Property(w => w.ApprovalType)
            .IsRequired()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(w => w.Workflow)
            .WithMany(wf => wf.Approvals)
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.RequestedBy)
            .WithMany(u => u.RequestedApprovals)
            .HasForeignKey(w => w.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ApprovedBy)
            .WithMany(u => u.WorkflowApprovals)
            .HasForeignKey(w => w.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}