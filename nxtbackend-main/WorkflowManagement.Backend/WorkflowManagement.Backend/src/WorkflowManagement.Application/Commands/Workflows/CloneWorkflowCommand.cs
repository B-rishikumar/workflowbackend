// CloneWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record CloneWorkflowCommand : IRequest<ResponseDto<WorkflowDto>>
{
    public Guid SourceWorkflowId { get; init; }
    public string NewName { get; init; } = string.Empty;
    public string? NewDescription { get; init; }
    public Guid? TargetEnvironmentId { get; init; }
    public Guid OwnerId { get; init; }
    public bool IncludeSteps { get; init; } = true;
    public bool IncludeSchedules { get; init; } = false;
    public string CreatedBy { get; init; } = string.Empty;
}