// ApiParameterConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class ApiParameterConfiguration : IEntityTypeConfiguration<ApiParameter>
{
    public void Configure(EntityTypeBuilder<ApiParameter> builder)
    {
        builder.ToTable("ApiParameters");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.DefaultValue)
            .HasMaxLength(1000);

        builder.Property(a => a.ValidationRules)
            .HasMaxLength(2000);

        builder.Property(a => a.Location)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(a => a.ApiEndpoint)
            .WithMany(e => e.Parameters)
            .HasForeignKey(a => a.ApiEndpointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}