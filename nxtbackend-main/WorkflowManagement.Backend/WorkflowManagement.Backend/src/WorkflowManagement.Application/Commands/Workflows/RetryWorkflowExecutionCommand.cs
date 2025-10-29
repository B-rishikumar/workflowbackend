// RetryWorkflowExecutionCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record RetryWorkflowExecutionCommand : IRequest<ResponseDto<WorkflowExecutionDto>>
{
    public Guid ExecutionId { get; init; }
    public Guid RetryById { get; init; }
    public bool RetryFromFailedStep { get; init; } = true;
    public Dictionary<string, object> AdditionalInputData { get; init; } = new();
    public string? RetryReason { get; init; }
}