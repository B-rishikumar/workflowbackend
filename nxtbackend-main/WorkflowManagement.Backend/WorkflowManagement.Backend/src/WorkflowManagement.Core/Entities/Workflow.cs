using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class Workflow : SoftDeleteEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    
    public Guid EnvironmentId { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public string? Tags { get; set; }
    
    public int TimeoutMinutes { get; set; } = 30;
    
    public bool IsPublished { get; set; } = false;
    
    public DateTime? PublishedAt { get; set; }
    
    public string? PublishedBy { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public Dictionary<string, object> GlobalVariables { get; set; } = new();
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    // Navigation Properties
    public WorkflowEnvironment Environment { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
    public ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
    public ICollection<WorkflowApproval> Approvals { get; set; } = new List<WorkflowApproval>();
    public ICollection<WorkflowSchedule> Schedules { get; set; } = new List<WorkflowSchedule>();
    public ICollection<WorkflowMetrics> Metrics { get; set; } = new List<WorkflowMetrics>();


}