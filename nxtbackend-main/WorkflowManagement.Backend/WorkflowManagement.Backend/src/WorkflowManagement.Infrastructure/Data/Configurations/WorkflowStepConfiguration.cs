// WorkflowStepConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(w => w.Workflow)
            .WithMany(wf => wf.Steps)
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.ApiEndpoint)
            .WithMany(a => a.WorkflowSteps)
            .HasForeignKey(w => w.ApiEndpointId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}