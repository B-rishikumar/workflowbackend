// CancelWorkflowExecutionCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record CancelWorkflowExecutionCommand : IRequest<ResponseDto>
{
    public Guid ExecutionId { get; init; }
    public string CancelledBy { get; init; } = string.Empty;
    public string? CancellationReason { get; init; }
}