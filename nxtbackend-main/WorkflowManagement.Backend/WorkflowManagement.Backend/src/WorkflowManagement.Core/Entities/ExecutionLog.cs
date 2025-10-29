using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class ExecutionLog : BaseEntity
{
    public Guid WorkflowExecutionId { get; set; }
    
    public Guid? WorkflowStepId { get; set; }
    
    public LogLevel Level { get; set; } = LogLevel.Information;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? Exception { get; set; }
    
    public string? Source { get; set; }
    
    public int StepOrder { get; set; }
    
    public ExecutionStatus StepStatus { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, object> Data { get; set; } = new();
    
    public string? RequestData { get; set; }
    
    public string? ResponseData { get; set; }
    
    public int? ResponseStatusCode { get; set; }
    
    public TimeSpan? Duration { get; set; }
    
    // Navigation Properties
    public WorkflowExecution WorkflowExecution { get; set; } = null!;
    public WorkflowStep? WorkflowStep { get; set; }
}