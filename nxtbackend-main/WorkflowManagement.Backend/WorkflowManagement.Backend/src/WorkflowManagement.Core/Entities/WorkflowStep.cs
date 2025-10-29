using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class WorkflowStep : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public Guid WorkflowId { get; set; }
    
    public Guid ApiEndpointId { get; set; }
    
    public int Order { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool ContinueOnError { get; set; } = false;
    
    public int TimeoutSeconds { get; set; } = 300;
    
    public int RetryCount { get; set; } = 0;
    
    public int RetryDelaySeconds { get; set; } = 5;
    
    public Dictionary<string, object> InputMapping { get; set; } = new();
    
    public Dictionary<string, object> OutputMapping { get; set; } = new();
    
    public Dictionary<string, object> Conditions { get; set; } = new();
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    // Navigation Properties
    public Workflow Workflow { get; set; } = null!;
    public ApiEndpoint ApiEndpoint { get; set; } = null!;
}