using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class Workspace : SoftDeleteEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Guid OwnerId { get; set; }
    
    public string? LogoUrl { get; set; }
    
    public Dictionary<string, object> Settings { get; set; } = new();
    
    // Navigation Properties
    public User Owner { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}