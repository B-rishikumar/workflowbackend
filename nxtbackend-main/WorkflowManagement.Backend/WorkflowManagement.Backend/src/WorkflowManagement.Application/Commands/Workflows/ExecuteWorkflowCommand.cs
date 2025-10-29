// ExecuteWorkflowCommand.cs// ExecuteWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record ExecuteWorkflowCommand : IRequest<ResponseDto<WorkflowExecutionDto>>
{
    public Guid WorkflowId { get; init; }
    public Guid ExecutedById { get; init; }
    public Dictionary<string, object> InputData { get; init; } = new();
    public string TriggerType { get; init; } = "manual";
    public string? TriggerSource { get; init; }
    public Dictionary<string, object> Context { get; init; } = new();
    public bool WaitForCompletion { get; init; } = false;
}

