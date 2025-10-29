// UpdateWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record UpdateWorkflowCommand : IRequest<ResponseDto<WorkflowDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public int TimeoutMinutes { get; init; }
    public int RetryCount { get; init; }
    public Dictionary<string, object> GlobalVariables { get; init; } = new();
    public Dictionary<string, object> Configuration { get; init; } = new();
    public string UpdatedBy { get; init; } = string.Empty;
    public string? ChangeReason { get; init; }
}