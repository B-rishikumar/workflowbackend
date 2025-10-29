using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WorkflowManagement.Application.Commands.Workflows;
using WorkflowManagement.Application.Queries.Workflows;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowExecutionService _workflowExecutionService;
        private readonly ILogger<WorkflowsController> _logger;

        public WorkflowsController(
            IMediator mediator,
            IWorkflowService workflowService,
            IWorkflowExecutionService workflowExecutionService,
            ILogger<WorkflowsController> logger)
        {
            _mediator = mediator;
            _workflowService = workflowService;
            _workflowExecutionService = workflowExecutionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all workflows with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<PagedResultDto<WorkflowDto>>>> GetWorkflows(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null,
            [FromQuery] WorkflowStatus? status = null,
            [FromQuery] string? createdBy = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var query = new GetWorkflowsQuery(page, pageSize, searchTerm, projectId, environmentId, status, createdBy, sortBy, sortDescending)
                {
                    CreatedAfter = createdAfter,
                    CreatedBefore = createdBefore
                };

                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows");
                return StatusCode(500, ResponseDto<PagedResultDto<WorkflowDto>>.Failure("An error occurred while retrieving workflows"));
            }
        }

        /// <summary>
        /// Get workflow by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<WorkflowDto>>> GetWorkflow(
            int id,
            [FromQuery] bool includeSteps = true,
            [FromQuery] bool includeVersions = false)
        {
            try
            {
                var query = new GetWorkflowQuery(id, includeSteps, includeVersions);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<WorkflowDto>.Failure("An error occurred while retrieving the workflow"));
            }
        }

        /// <summary>
        /// Get workflow with detailed information
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ResponseDto<WorkflowDetailDto>>> GetWorkflowDetail(
            int id,
            [FromQuery] int? versionId = null)
        {
            try
            {
                var query = new GetWorkflowDetailQuery(id, versionId);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow detail {WorkflowId}", id);
                return StatusCode(500, ResponseDto<WorkflowDetailDto>.Failure("An error occurred while retrieving workflow details"));
            }
        }

        /// <summary>
        /// Get workflows by project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ResponseDto<List<WorkflowDto>>>> GetWorkflowsByProject(
            int projectId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] WorkflowStatus? status = null)
        {
            try
            {
                var query = new GetWorkflowsByProjectQuery(projectId, includeInactive, status);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows by project {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<List<WorkflowDto>>.Failure("An error occurred while retrieving project workflows"));
            }
        }

        /// <summary>
        /// Get workflows by environment
        /// </summary>
        [HttpGet("environment/{environmentId}")]
        public async Task<ActionResult<ResponseDto<List<WorkflowDto>>>> GetWorkflowsByEnvironment(
            int environmentId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetWorkflowsByEnvironmentQuery(environmentId, includeInactive);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows by environment {EnvironmentId}", environmentId);
                return StatusCode(500, ResponseDto<List<WorkflowDto>>.Failure("An error occurred while retrieving environment workflows"));
            }
        }

        /// <summary>
        /// Get active workflows
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResponseDto<List<WorkflowDto>>>> GetActiveWorkflows(
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null,
            [FromQuery] bool includeScheduled = true)
        {
            try
            {
                var query = new GetActiveWorkflowsQuery(projectId, environmentId, includeScheduled);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active workflows");
                return StatusCode(500, ResponseDto<List<WorkflowDto>>.Failure("An error occurred while retrieving active workflows"));
            }
        }

        /// <summary>
        /// Search workflows with advanced filters
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<ResponseDto<PagedResultDto<WorkflowDto>>>> SearchWorkflows([FromBody] SearchWorkflowsQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching workflows");
                return StatusCode(500, ResponseDto<PagedResultDto<WorkflowDto>>.Failure("An error occurred while searching workflows"));
            }
        }

        /// <summary>
        /// Create a new workflow
        /// </summary>
        [HttpPost]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<WorkflowDto>>> CreateWorkflow([FromBody] CreateWorkflowDto createWorkflowDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var command = new CreateWorkflowCommand
                {
                    Name = createWorkflowDto.Name,
                    Description = createWorkflowDto.Description,
                    ProjectId = createWorkflowDto.ProjectId,
                    EnvironmentId = createWorkflowDto.EnvironmentId,
                    RequiresApproval = createWorkflowDto.RequiresApproval,
                    Tags = createWorkflowDto.Tags,
                    CreatedByUserId = currentUserId
                };

                var result = await _mediator.Send(command);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetWorkflow), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow");
                return StatusCode(500, ResponseDto<WorkflowDto>.Failure("An error occurred while creating the workflow"));
            }
        }

        /// <summary>
        /// Update workflow information
        /// </summary>
        [HttpPut("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<WorkflowDto>>> UpdateWorkflow(int id, [FromBody] UpdateWorkflowDto updateWorkflowDto)
        {
            try
            {
                var command = new UpdateWorkflowCommand
                {
                    Id = id,
                    Name = updateWorkflowDto.Name,
                    Description = updateWorkflowDto.Description,
                    RequiresApproval = updateWorkflowDto.RequiresApproval,
                    Tags = updateWorkflowDto.Tags
                };

                var result = await _mediator.Send(command);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<WorkflowDto>.Failure("An error occurred while updating the workflow"));
            }
        }

        /// <summary>
        /// Delete workflow (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteWorkflow(int id)
        {
            try
            {
                var result = await _workflowService.DeleteWorkflowAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the workflow"));
            }
        }

        /// <summary>
        /// Publish workflow
        /// </summary>
        [HttpPost("{id}/publish")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> PublishWorkflow(int id)
        {
            try
            {
                var result = await _workflowService.PublishWorkflowAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while publishing the workflow"));
            }
        }

        /// <summary>
        /// Unpublish workflow
        /// </summary>
        [HttpPost("{id}/unpublish")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> UnpublishWorkflow(int id)
        {
            try
            {
                var result = await _workflowService.UnpublishWorkflowAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while unpublishing the workflow"));
            }
        }

        /// <summary>
        /// Execute workflow
        /// </summary>
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<ResponseDto<WorkflowExecutionDto>>> ExecuteWorkflow(
            int id,
            [FromBody] ExecuteWorkflowRequestDto? executeRequest = null)
        {
            try
            {
                var initialInputs = executeRequest?.InitialInputs ?? new Dictionary<string, object>();
                var executionContext = executeRequest?.ExecutionContext ?? "Manual Execution";

                var result = await _workflowExecutionService.ExecuteWorkflowAsync(id, initialInputs, executionContext);
                
                if (result.Success)
                    return Accepted(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<WorkflowExecutionDto>.Failure("An error occurred while executing the workflow"));
            }
        }

        /// <summary>
        /// Test workflow
        /// </summary>
        [HttpPost("{id}/test")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> TestWorkflow(
            int id,
            [FromBody] TestWorkflowRequestDto? testRequest = null)
        {
            try
            {
                var testInputs = testRequest?.TestInputs ?? new Dictionary<string, object>();
                var result = await _workflowExecutionService.TestWorkflowAsync(id, testInputs);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while testing the workflow"));
            }
        }

        /// <summary>
        /// Clone workflow
        /// </summary>
        [HttpPost("{id}/clone")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<WorkflowDto>>> CloneWorkflow(int id, [FromBody] CloneWorkflowDto cloneDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _workflowService.CloneWorkflowAsync(id, cloneDto.Name, cloneDto.Description, currentUserId);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetWorkflow), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning workflow {WorkflowId}", id);
                return StatusCode(500, ResponseDto<WorkflowDto>.Failure("An error occurred while cloning the workflow"));
            }
        }

        /// <summary>
        /// Get workflow versions
        /// </summary>
        [HttpGet("{workflowId}/versions")]
        public async Task<ActionResult<ResponseDto<List<WorkflowVersionDto>>>> GetWorkflowVersions(
            int workflowId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = new GetWorkflowVersionsQuery(workflowId, page, pageSize);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting versions for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<List<WorkflowVersionDto>>.Failure("An error occurred while retrieving workflow versions"));
            }
        }

        /// <summary>
        /// Get specific workflow version
        /// </summary>
        [HttpGet("{workflowId}/versions/{versionId}")]
        public async Task<ActionResult<ResponseDto<WorkflowVersionDto>>> GetWorkflowVersion(int workflowId, int versionId)
        {
            try
            {
                var query = new GetWorkflowVersionQuery(workflowId, versionId);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version {VersionId} for workflow {WorkflowId}", versionId, workflowId);
                return StatusCode(500, ResponseDto<WorkflowVersionDto>.Failure("An error occurred while retrieving the workflow version"));
            }
        }

        /// <summary>
        /// Check workflow executability
        /// </summary>
        [HttpGet("{workflowId}/executability")]
        public async Task<ActionResult<ResponseDto<WorkflowExecutabilityDto>>> GetWorkflowExecutability(
            int workflowId,
            [FromQuery] int? versionId = null)
        {
            try
            {
                var query = new GetWorkflowExecutabilityQuery(workflowId, versionId);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking executability for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<WorkflowExecutabilityDto>.Failure("An error occurred while checking workflow executability"));
            }
        }

        /// <summary>
        /// Get workflow summary statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ResponseDto<WorkflowsSummaryDto>>> GetWorkflowsSummary(
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = new GetWorkflowsSummaryQuery(projectId, environmentId, fromDate, toDate);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows summary");
                return StatusCode(500, ResponseDto<WorkflowsSummaryDto>.Failure("An error occurred while retrieving workflows summary"));
            }
        }

        /// <summary>
        /// Get recent workflow executions
        /// </summary>
        [HttpGet("executions/recent")]
        public async Task<ActionResult<ResponseDto<List<WorkflowExecutionDto>>>> GetRecentExecutions(
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null,
            [FromQuery] int count = 10,
            [FromQuery] ExecutionStatus? status = null)
        {
            try
            {
                var query = new GetRecentWorkflowExecutionsQuery(projectId, environmentId, count, status);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent workflow executions");
                return StatusCode(500, ResponseDto<List<WorkflowExecutionDto>>.Failure("An error occurred while retrieving recent executions"));
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
        }

        #endregion
    }

    /// <summary>
    /// Request DTOs
    /// </summary>
    public class ExecuteWorkflowRequestDto
    {
        public Dictionary<string, object> InitialInputs { get; set; } = new();
        public string? ExecutionContext { get; set; }
    }

    public class TestWorkflowRequestDto
    {
        public Dictionary<string, object> TestInputs { get; set; } = new();
    }

    public class CloneWorkflowDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}