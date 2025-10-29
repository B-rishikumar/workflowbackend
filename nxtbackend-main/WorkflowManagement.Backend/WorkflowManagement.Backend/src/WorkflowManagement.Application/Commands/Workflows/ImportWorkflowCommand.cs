// ImportWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record ImportWorkflowCommand : IRequest<ResponseDto<WorkflowDto>>
{
    public string WorkflowDefinition { get; init; } = string.Empty;
    public string Format { get; init; } = "json"; // json, xml, yaml
    public Guid EnvironmentId { get; init; }
    public Guid OwnerId { get; init; }
    public string? NewName { get; init; }
    public bool OverrideExisting { get; init; } = false;
    public string ImportedBy { get; init; } = string.Empty;
}