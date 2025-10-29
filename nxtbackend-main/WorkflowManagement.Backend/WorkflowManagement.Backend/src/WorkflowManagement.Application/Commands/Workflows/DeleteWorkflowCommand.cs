// DeleteWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record DeleteWorkflowCommand : IRequest<ResponseDto>
{
    public Guid Id { get; init; }
    public string DeletedBy { get; init; } = string.Empty;
    public string? DeleteReason { get; init; }
    public bool ForceDelete { get; init; } = false;
}