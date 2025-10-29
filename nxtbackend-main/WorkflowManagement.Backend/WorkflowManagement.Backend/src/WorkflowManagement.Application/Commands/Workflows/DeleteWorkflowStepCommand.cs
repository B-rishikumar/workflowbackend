// DeleteWorkflowStepCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record DeleteWorkflowStepCommand : IRequest<ResponseDto>
{
    public Guid Id { get; init; }
    public string DeletedBy { get; init; } = string.Empty;
    public string? DeleteReason { get; init; }
}
