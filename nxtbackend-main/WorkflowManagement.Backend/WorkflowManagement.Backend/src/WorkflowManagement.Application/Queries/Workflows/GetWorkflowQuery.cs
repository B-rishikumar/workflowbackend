using MediatR;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Queries.Workflows
{
    /// <summary>
    /// Query to get a single workflow by ID
    /// </summary>
    public class GetWorkflowQuery : IRequest<ResponseDto<WorkflowDto>>
    {
        public int WorkflowId { get; set; }
        public bool IncludeSteps { get; set; } = true;
        public bool IncludeVersions { get; set; } = false;

        public GetWorkflowQuery(int workflowId, bool includeSteps = true, bool includeVersions = false)
        {
            WorkflowId = workflowId;
            IncludeSteps = includeSteps;
            IncludeVersions = includeVersions;
        }
    }

    /// <summary>
    /// Query to get workflow with detailed information
    /// </summary>
    public class GetWorkflowDetailQuery : IRequest<ResponseDto<WorkflowDetailDto>>
    {
        public int WorkflowId { get; set; }
        public int? VersionId { get; set; }

        public GetWorkflowDetailQuery(int workflowId, int? versionId = null)
        {
            WorkflowId = workflowId;
            VersionId = versionId;
        }
    }

    /// <summary>
    /// Query to get workflow by name
    /// </summary>
    public class GetWorkflowByNameQuery : IRequest<ResponseDto<WorkflowDto>>
    {
        public string WorkflowName { get; set; }
        public int ProjectId { get; set; }

        public GetWorkflowByNameQuery(string workflowName, int projectId)
        {
            WorkflowName = workflowName;
            ProjectId = projectId;
        }
    }

    /// <summary>
    /// Query to get workflow version history
    /// </summary>
    public class GetWorkflowVersionsQuery : IRequest<ResponseDto<List<WorkflowVersionDto>>>
    {
        public int WorkflowId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public GetWorkflowVersionsQuery(int workflowId, int page = 1, int pageSize = 20)
        {
            WorkflowId = workflowId;
            Page = page;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// Query to get specific workflow version
    /// </summary>
    public class GetWorkflowVersionQuery : IRequest<ResponseDto<WorkflowVersionDto>>
    {
        public int WorkflowId { get; set; }
        public int VersionId { get; set; }

        public GetWorkflowVersionQuery(int workflowId, int versionId)
        {
            WorkflowId = workflowId;
            VersionId = versionId;
        }
    }

    /// <summary>
    /// Query to check if workflow can be executed
    /// </summary>
    public class GetWorkflowExecutabilityQuery : IRequest<ResponseDto<WorkflowExecutabilityDto>>
    {
        public int WorkflowId { get; set; }
        public int? VersionId { get; set; }

        public GetWorkflowExecutabilityQuery(int workflowId, int? versionId = null)
        {
            WorkflowId = workflowId;
            VersionId = versionId;
        }
    }
}