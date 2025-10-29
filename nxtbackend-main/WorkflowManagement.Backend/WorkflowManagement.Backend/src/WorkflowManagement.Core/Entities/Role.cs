using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class Role : SoftDeleteEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string NormalizedName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystemRole { get; set; } = false;
    
    public int Priority { get; set; } = 0;
    
    public string? Color { get; set; }
    
    public Dictionary<string, object> Settings { get; set; } = new();
    
    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    
    // Helper Methods
    public void SetNormalizedName()
    {
        NormalizedName = Name.ToUpperInvariant();
    }
    
    public bool HasPermission(string permissionName)
    {
        return Permissions.Any(p => p.FullPermission == permissionName && p.IsActive);
    }
}