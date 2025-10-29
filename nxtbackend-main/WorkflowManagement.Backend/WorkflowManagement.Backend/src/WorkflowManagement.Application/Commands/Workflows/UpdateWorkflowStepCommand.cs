// UpdateWorkflowStepCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record UpdateWorkflowStepCommand : IRequest<ResponseDto<WorkflowStepDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; }
    public bool ContinueOnError { get; init; }
    public int TimeoutSeconds { get; init; }
    public int RetryCount { get; init; }
    public int RetryDelaySeconds { get; init; }
    public Dictionary<string, object> InputMapping { get; init; } = new();
    public Dictionary<string, object> OutputMapping { get; init; } = new();
    public Dictionary<string, object> Conditions { get; init; } = new();
    public Dictionary<string, object> Configuration { get; init; } = new();
    public string UpdatedBy { get; init; } = string.Empty;
}