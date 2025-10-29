// Workflow/WorkflowExecutionDto.cs
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.Workflow;

public class WorkflowExecutionDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public Guid ExecutedById { get; set; }
    public string ExecutedByName { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int FailedSteps { get; set; }
    public string? TriggerType { get; set; }
    public string? TriggerSource { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
}
