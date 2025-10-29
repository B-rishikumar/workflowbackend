// CreateWorkflowVersionCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record CreateWorkflowVersionCommand : IRequest<ResponseDto<WorkflowVersionDto>>
{
    public Guid WorkflowId { get; init; }
    public string VersionNumber { get; init; } = string.Empty;
    public string? ChangeDescription { get; init; }
    public string WorkflowDefinition { get; init; } = string.Empty;
    public Dictionary<string, object> Configuration { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
    public string CreatedBy { get; init; } = string.Empty;
}