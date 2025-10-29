// ExecutionLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class ExecutionLogConfiguration : IEntityTypeConfiguration<ExecutionLog>
{
    public void Configure(EntityTypeBuilder<ExecutionLog> builder)
    {
        builder.ToTable("ExecutionLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Level)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.StepStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Exception)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Source)
            .HasMaxLength(200);

        builder.Property(e => e.RequestData)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ResponseData)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(e => e.WorkflowExecution)
            .WithMany(w => w.Logs)
            .HasForeignKey(e => e.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkflowStep)
            .WithMany()
            .HasForeignKey(e => e.WorkflowStepId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}