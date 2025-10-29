using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class Permission : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Resource { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public string? Category { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Navigation Properties
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    
    // Computed Properties
    public string FullPermission => $"{Module}.{Action}" + (string.IsNullOrEmpty(Resource) ? "" : $".{Resource}");
}

// Junction table for Many-to-Many relationship between Role and Permission
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    public bool IsGranted { get; set; } = true;
    
    public DateTime? GrantedAt { get; set; }
    
    public string? GrantedBy { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation Properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}