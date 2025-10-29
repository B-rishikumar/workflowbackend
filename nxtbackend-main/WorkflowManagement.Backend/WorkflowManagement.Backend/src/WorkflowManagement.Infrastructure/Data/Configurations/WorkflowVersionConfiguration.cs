// WorkflowVersionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("WorkflowVersions");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.VersionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.ChangeDescription)
            .HasMaxLength(1000);

        builder.Property(w => w.PublishedBy)
            .HasMaxLength(100);

        builder.Property(w => w.WorkflowDefinition)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(w => w.Workflow)
            .WithMany(wf => wf.Versions)
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}