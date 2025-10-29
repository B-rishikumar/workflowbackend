// UnpublishWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Commands.Workflows;

public record UnpublishWorkflowCommand : IRequest<ResponseDto<WorkflowDto>>
{
    public Guid WorkflowId { get; init; }
    public Guid UnpublishedById { get; init; }
    public string? UnpublishReason { get; init; }
}