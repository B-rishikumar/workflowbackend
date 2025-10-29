// WorkflowConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(w => w.Tags)
            .HasMaxLength(500);

        builder.Property(w => w.PublishedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(w => w.Environment)
            .WithMany(e => e.Workflows)
            .HasForeignKey(w => w.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Owner)
            .WithMany(u => u.OwnedWorkflows)
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Steps)
            .WithOne(s => s.Workflow)
            .HasForeignKey(s => s.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Versions)
            .WithOne(v => v.Workflow)
            .HasForeignKey(v => v.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Executions)
            .WithOne(e => e.Workflow)
            .HasForeignKey(e => e.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Approvals)
            .WithOne(a => a.Workflow)
            .HasForeignKey(a => a.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Schedules)
            .WithOne(s => s.Workflow)
            .HasForeignKey(s => s.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Metrics)
            .WithOne(m => m.Workflow)
            .HasForeignKey(m => m.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}