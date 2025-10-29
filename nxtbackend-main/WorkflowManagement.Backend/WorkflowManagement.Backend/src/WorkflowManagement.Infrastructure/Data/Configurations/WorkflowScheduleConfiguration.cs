// WorkflowScheduleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class WorkflowScheduleConfiguration : IEntityTypeConfiguration<WorkflowSchedule>
{
    public void Configure(EntityTypeBuilder<WorkflowSchedule> builder)
    {
        builder.ToTable("WorkflowSchedules");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.Frequency)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(w => w.CronExpression)
            .HasMaxLength(100);

        builder.Property(w => w.TimeZone)
            .HasConversion(
                v => v.Id,
                v => TimeZoneInfo.FindSystemTimeZoneById(v));

        // Relationships
        builder.HasOne(w => w.Workflow)
            .WithMany(wf => wf.Schedules)
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
