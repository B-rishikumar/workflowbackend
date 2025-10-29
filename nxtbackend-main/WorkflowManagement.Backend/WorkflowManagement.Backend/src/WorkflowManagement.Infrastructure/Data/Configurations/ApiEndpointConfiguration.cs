// ApiEndpointConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class ApiEndpointConfiguration : IEntityTypeConfiguration<ApiEndpoint>
{
    public void Configure(EntityTypeBuilder<ApiEndpoint> builder)
    {
        builder.ToTable("ApiEndpoints");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Url)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.Method)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.RequestContentType)
            .HasMaxLength(100);

        builder.Property(a => a.ResponseContentType)
            .HasMaxLength(100);

        builder.Property(a => a.SwaggerDefinition)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.SoapWsdl)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasMany(a => a.Parameters)
            .WithOne(p => p.ApiEndpoint)
            .HasForeignKey(p => p.ApiEndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.WorkflowSteps)
            .WithOne(s => s.ApiEndpoint)
            .HasForeignKey(s => s.ApiEndpointId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
