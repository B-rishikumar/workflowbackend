using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Core.Entities;

public class WorkflowMetrics : SoftDeleteEntity
{
    public Guid WorkflowId { get; set; }
    
    public DateTime Date { get; set; }
    
    public int TotalExecutions { get; set; }
    
    public int SuccessfulExecutions { get; set; }
    
    public int FailedExecutions { get; set; }
    
    public TimeSpan AverageExecutionTime { get; set; }
    
    public TimeSpan MinExecutionTime { get; set; }
    
    public TimeSpan MaxExecutionTime { get; set; }
    
    public double SuccessRate { get; set; }
    
    public Dictionary<string, int> ErrorCounts { get; set; } = new();
    
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
    
    // Navigation Properties
    public Workflow Workflow { get; set; } = null!;
}