// GetWorkflowsQuery.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Queries.Workflows;

public record GetWorkflowsQuery : IRequest<ResponseDto<PagedResultDto<WorkflowDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public Guid? EnvironmentId { get; init; }
    public Guid? OwnerId { get; init; }
    public WorkflowStatus? Status { get; init; }
    public bool? IsPublished { get; init; }
    public string? Tags { get; init; }
}