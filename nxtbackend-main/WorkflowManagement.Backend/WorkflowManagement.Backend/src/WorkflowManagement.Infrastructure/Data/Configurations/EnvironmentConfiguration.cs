// EnvironmentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class EnvironmentConfiguration : IEntityTypeConfiguration<Core.Entities.WorkflowEnvironment>
{
    public void Configure(EntityTypeBuilder<Core.Entities.WorkflowEnvironment> builder)
    {
        builder.ToTable("Environments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Color)
            .HasMaxLength(10);

        // Relationships
        builder.HasOne(e => e.Project)
            .WithMany(p => p.Environments)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Workflows)
            .WithOne(w => w.Environment)
            .HasForeignKey(w => w.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}