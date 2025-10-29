// WorkflowMetricsConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowMetricsConfiguration : IEntityTypeConfiguration<WorkflowMetrics>
{
    public void Configure(EntityTypeBuilder<WorkflowMetrics> builder)
    {
        builder.ToTable("WorkflowMetrics");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(w => w.SuccessRate)
            .HasPrecision(5, 2);

        // Relationships
        builder.HasOne(w => w.Workflow)
            .WithMany(wf => wf.Metrics)
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index for workflow and date
        builder.HasIndex(w => new { w.WorkflowId, w.Date })
            .IsUnique()
            .HasDatabaseName("IX_WorkflowMetrics_WorkflowId_Date");
    }
}