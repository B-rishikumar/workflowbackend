// PermissionConfiguration.cs
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

// RoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Color)
            .HasMaxLength(10);

        // Unique constraint on Name and NormalizedName
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.HasIndex(r => r.NormalizedName)
            .IsUnique()
            .HasDatabaseName("IX_Roles_NormalizedName");

        // Index on IsActive for performance
        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Roles_IsActive");

        // One-to-Many relationship with User
        builder.HasMany(r => r.Users)
            .WithOne(u => u.DetailedRole)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// Updated UserConfiguration.cs to include Role relationship
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Infrastructure.Data.Configurations;

public class UpdatedUserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.Department)
            .HasMaxLength(100);

        builder.Property(u => u.JobTitle)
            .HasMaxLength(100);

        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        // Unique constraints
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        // Performance indexes
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        // Relationship with Role
        builder.HasOne(u => u.DetailedRole)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        // Existing relationships
        builder.HasMany(u => u.OwnedWorkspaces)
            .WithOne(w => w.Owner)
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.OwnedProjects)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.OwnedWorkflows)
            .WithOne(w => w.Owner)
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.WorkflowExecutions)
            .WithOne(we => we.ExecutedBy)
            .HasForeignKey(we => we.ExecutedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.WorkflowApprovals)
            .WithOne(wa => wa.ApprovedBy)
            .HasForeignKey(wa => wa.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RequestedApprovals)
            .WithOne(wa => wa.RequestedBy)
            .HasForeignKey(wa => wa.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}