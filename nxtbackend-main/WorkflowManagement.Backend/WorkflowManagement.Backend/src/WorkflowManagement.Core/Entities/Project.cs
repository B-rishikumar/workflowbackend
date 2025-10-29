using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class Project : SoftDeleteEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Guid WorkspaceId { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public string? Color { get; set; }
    
    public Dictionary<string, object> Settings { get; set; } = new();
    
    // Navigation Properties
    public Workspace Workspace { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<WorkflowEnvironment> Environments { get; set; } = new List<WorkflowEnvironment>();
}