using MediatR;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Queries.Workflows
{
    /// <summary>
    /// Query to get paginated list of workflows
    /// </summary>
    public class GetWorkflowsQuery : IRequest<ResponseDto<PagedResultDto<WorkflowDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? ProjectId { get; set; }
        public int? EnvironmentId { get; set; }
        public WorkflowStatus? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;

        public GetWorkflowsQuery() { }

        public GetWorkflowsQuery(int page, int pageSize, string? searchTerm = null,
            int? projectId = null, int? environmentId = null, WorkflowStatus? status = null,
            string? createdBy = null, string? sortBy = "CreatedAt", bool sortDescending = true)
        {
            Page = page;
            PageSize = pageSize;
            SearchTerm = searchTerm;
            ProjectId = projectId;
            EnvironmentId = environmentId;
            Status = status;
            CreatedBy = createdBy;
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }

    /// <summary>
    /// Query to get workflows by project
    /// </summary>
    public class GetWorkflowsByProjectQuery : IRequest<ResponseDto<List<WorkflowDto>>>
    {
        public int ProjectId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public WorkflowStatus? Status { get; set; }

        public GetWorkflowsByProjectQuery(int projectId, bool includeInactive = false, WorkflowStatus? status = null)
        {
            ProjectId = projectId;
            IncludeInactive = includeInactive;
            Status = status;
        }
    }

    /// <summary>
    /// Query to get workflows by environment
    /// </summary>
    public class GetWorkflowsByEnvironmentQuery : IRequest<ResponseDto<List<WorkflowDto>>>
    {
        public int EnvironmentId { get; set; }
        public bool IncludeInactive { get; set; } = false;

        public GetWorkflowsByEnvironmentQuery(int environmentId, bool includeInactive = false)
        {
            EnvironmentId = environmentId;
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query to get active workflows (for scheduling, execution, etc.)
    /// </summary>
    public class GetActiveWorkflowsQuery : IRequest<ResponseDto<List<WorkflowDto>>>
    {
        public int? ProjectId { get; set; }
        public int? EnvironmentId { get; set; }
        public bool IncludeScheduled { get; set; } = true;

        public GetActiveWorkflowsQuery(int? projectId = null, int? environmentId = null, bool includeScheduled = true)
        {
            ProjectId = projectId;
            EnvironmentId = environmentId;
            IncludeScheduled = includeScheduled;
        }
    }

    /// <summary>
    /// Query to get workflows by status
    /// </summary>
    public class GetWorkflowsByStatusQuery : IRequest<ResponseDto<List<WorkflowDto>>>
    {
        public WorkflowStatus Status { get; set; }
        public int? ProjectId { get; set; }
        public int? EnvironmentId { get; set; }

        public GetWorkflowsByStatusQuery(WorkflowStatus status, int? projectId = null, int? environmentId = null)
        {
            Status = status;
            ProjectId = projectId;
            EnvironmentId = environmentId;
        }
    }

    /// <summary>
    /// Query to get workflows created by specific user
    /// </summary>
    public class GetWorkflowsByUserQuery : IRequest<ResponseDto<PagedResultDto<WorkflowDto>>>
    {
        public int UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public WorkflowStatus? Status { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;

        public GetWorkflowsByUserQuery(int userId, int page = 1, int pageSize = 10, 
            WorkflowStatus? status = null, string? sortBy = "CreatedAt", bool sortDescending = true)
        {
            UserId = userId;
            Page = page;
            PageSize = pageSize;
            Status = status;
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }

    /// <summary>
    /// Query to search workflows with advanced filters
    /// </summary>
    public class SearchWorkflowsQuery : IRequest<ResponseDto<PagedResultDto<WorkflowDto>>>
    {
        public string? SearchTerm { get; set; }
        public List<int>? ProjectIds { get; set; }
        public List<int>? EnvironmentIds { get; set; }
        public List<WorkflowStatus>? Statuses { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? LastExecutedAfter { get; set; }
        public DateTime? LastExecutedBefore { get; set; }
        public bool? HasSchedule { get; set; }
        public bool? RequiresApproval { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// Query to get workflow summary statistics
    /// </summary>
    public class GetWorkflowsSummaryQuery : IRequest<ResponseDto<WorkflowsSummaryDto>>
    {
        public int? ProjectId { get; set; }
        public int? EnvironmentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public GetWorkflowsSummaryQuery(int? projectId = null, int? environmentId = null, 
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            ProjectId = projectId;
            EnvironmentId = environmentId;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }

    /// <summary>
    /// Query to get recently executed workflows
    /// </summary>
    public class GetRecentWorkflowExecutionsQuery : IRequest<ResponseDto<List<WorkflowExecutionDto>>>
    {
        public int? ProjectId { get; set; }
        public int? EnvironmentId { get; set; }
        public int Count { get; set; } = 10;
        public ExecutionStatus? Status { get; set; }

        public GetRecentWorkflowExecutionsQuery(int? projectId = null, int? environmentId = null, 
            int count = 10, ExecutionStatus? status = null)
        {
            ProjectId = projectId;
            EnvironmentId = environmentId;
            Count = count;
            Status = status;
        }
    }
}