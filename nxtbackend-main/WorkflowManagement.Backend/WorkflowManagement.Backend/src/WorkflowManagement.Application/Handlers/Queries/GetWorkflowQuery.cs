// GetWorkflowQuery.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;

namespace WorkflowManagement.Application.Queries.Workflows;

public record GetWorkflowQuery : IRequest<ResponseDto<WorkflowDto>>
{
    public Guid Id { get; init; }
    public bool IncludeSteps { get; init; } = false;
    public bool IncludeVersions { get; init; } = false;
}
