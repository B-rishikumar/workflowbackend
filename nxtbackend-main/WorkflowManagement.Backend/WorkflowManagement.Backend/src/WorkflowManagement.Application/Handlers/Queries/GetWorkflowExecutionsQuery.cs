// GetWorkflowExecutionsQuery.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Queries.Workflows;

public record GetWorkflowExecutionsQuery : IRequest<ResponseDto<PagedResultDto<WorkflowExecutionDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? WorkflowId { get; init; }
    public Guid? ExecutedById { get; init; }
    public ExecutionStatus? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? TriggerType { get; init; }
}