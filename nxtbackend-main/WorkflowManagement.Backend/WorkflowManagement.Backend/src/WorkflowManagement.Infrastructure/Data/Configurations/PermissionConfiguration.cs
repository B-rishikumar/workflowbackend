using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Resource)
            .HasMaxLength(50);

        builder.Property(p => p.Category)
            .HasMaxLength(50);

        // Create unique constraint on Module, Action, Resource combination
        builder.HasIndex(p => new { p.Module, p.Action, p.Resource })
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Module_Action_Resource");

        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Name");

        // Many-to-Many relationship with Role through RolePermission
        builder.HasMany(p => p.Roles)
            .WithMany(r => r.Permissions)
            .UsingEntity<RolePermission>(
                j => j.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(rp => rp.RoleId),
                j => j.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(rp => rp.PermissionId),
                j =>
                {
                    j.ToTable("RolePermissions");
                    j.HasKey(rp => rp.Id);
                    j.Property(rp => rp.GrantedBy).HasMaxLength(100);
                    j.Property(rp => rp.Notes).HasMaxLength(500);
                    j.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                     .IsUnique()
                     .HasDatabaseName("IX_RolePermissions_RoleId_PermissionId");
                });
    }
}