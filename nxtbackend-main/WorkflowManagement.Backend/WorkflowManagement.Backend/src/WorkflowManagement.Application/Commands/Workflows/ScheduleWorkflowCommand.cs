// ScheduleWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Commands.Workflows;

public record ScheduleWorkflowCommand : IRequest<ResponseDto<WorkflowScheduleDto>>
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ScheduleFrequency Frequency { get; init; }
    public string? CronExpression { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
    public int MaxRuns { get; init; } = 0;
    public Dictionary<string, object> Parameters { get; init; } = new();
    public Dictionary<string, object> Configuration { get; init; } = new();
    public string CreatedBy { get; init; } = string.Empty;
}

public class WorkflowScheduleDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public ScheduleFrequency Frequency { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public int RunCount { get; set; }
    public int MaxRuns { get; set; }
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}