using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class WorkflowEnvironment : SoftDeleteEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Guid ProjectId { get; set; }
    
    public string? Color { get; set; }
    
    public Dictionary<string, string> Variables { get; set; } = new();
    
    public Dictionary<string, object> Settings { get; set; } = new();
    
    // Navigation Properties
    public Project Project { get; set; } = null!;
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}