using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class WorkflowExecution : SoftDeleteEntity
{
    public Guid WorkflowId { get; set; }
    
    public Guid ExecutedById { get; set; }
    
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public int TotalSteps { get; set; }
    
    public int CompletedSteps { get; set; }
    
    public int FailedSteps { get; set; }
    
    public Dictionary<string, object> InputData { get; set; } = new();
    
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    public Dictionary<string, object> Context { get; set; } = new();
    
    public string? TriggerType { get; set; } // manual, scheduled, webhook
    
    public string? TriggerSource { get; set; }
    
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue 
        ? CompletedAt.Value - StartedAt.Value 
        : null;
    
    // Navigation Properties
    public Workflow Workflow { get; set; } = null!;
    public User ExecutedBy { get; set; } = null!;
    public ICollection<ExecutionLog> Logs { get; set; } = new List<ExecutionLog>();
}