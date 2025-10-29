// PublishWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record PublishWorkflowCommand : IRequest<ResponseDto<WorkflowDto>>
{
    public Guid WorkflowId { get; init; }
    public Guid PublishedById { get; init; }
    public string? PublishNotes { get; init; }
    public bool RequireApproval { get; init; } = true;
}
