// CreateWorkflowCommand.cs/ CreateWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record CreateWorkflowCommand : IRequest<ResponseDto<WorkflowDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid EnvironmentId { get; init; }
    public Guid OwnerId { get; init; }
    public string? Tags { get; init; }
    public int TimeoutMinutes { get; init; } = 30;
    public int RetryCount { get; init; } = 0;
    public Dictionary<string, object> GlobalVariables { get; init; } = new();
    public Dictionary<string, object> Configuration { get; init; } = new();
    public string CreatedBy { get; init; } = string.Empty;
}