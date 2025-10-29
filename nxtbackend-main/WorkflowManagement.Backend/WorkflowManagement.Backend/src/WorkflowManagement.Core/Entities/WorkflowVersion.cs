using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class WorkflowVersion : BaseEntity
{
    public Guid WorkflowId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string VersionNumber { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? ChangeDescription { get; set; }
    
    public bool IsActive { get; set; } = false;
    
    public bool IsPublished { get; set; } = false;
    
    public DateTime? PublishedAt { get; set; }
    
    public string? PublishedBy { get; set; }
    
    public string WorkflowDefinition { get; set; } = string.Empty; // JSON
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Navigation Properties
    public Workflow Workflow { get; set; } = null!;
}