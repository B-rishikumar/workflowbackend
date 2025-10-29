// WorkflowExecutionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowExecutionConfiguration : IEntityTypeConfiguration<WorkflowExecution>
{
    public void Configure(EntityTypeBuilder<WorkflowExecution> builder)
    {
        builder.ToTable("WorkflowExecutions");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(w => w.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(w => w.TriggerType)
            .HasMaxLength(50);

        builder.Property(w => w.TriggerSource)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(w => w.Workflow)
            .WithMany(wf => wf.Executions)
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.ExecutedBy)
            .WithMany(u => u.WorkflowExecutions)
            .HasForeignKey(w => w.ExecutedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Logs)
            .WithOne(l => l.WorkflowExecution)
            .HasForeignKey(l => l.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
