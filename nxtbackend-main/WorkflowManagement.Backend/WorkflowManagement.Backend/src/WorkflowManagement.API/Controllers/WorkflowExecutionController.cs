using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowExecutionController : ControllerBase
    {
        private readonly IWorkflowExecutionService _workflowExecutionService;
        private readonly ILogger<WorkflowExecutionController> _logger;

        public WorkflowExecutionController(
            IWorkflowExecutionService workflowExecutionService,
            ILogger<WorkflowExecutionController> logger)
        {
            _workflowExecutionService = workflowExecutionService;
            _logger = logger;
        }

        /// <summary>
        /// Get execution by ID
        /// </summary>
        [HttpGet("{executionId}")]
        public async Task<ActionResult<ResponseDto<WorkflowExecutionDto>>> GetExecution(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<WorkflowExecutionDto>.Failure("An error occurred while retrieving the execution"));
            }
        }

        /// <summary>
        /// Get executions with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<PagedResultDto<WorkflowExecutionDto>>>> GetExecutions(
            [FromQuery] int? workflowId = null,
            [FromQuery] ExecutionStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "ExecutedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionsAsync(
                    workflowId, status, startDate, endDate, page, pageSize);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions");
                return StatusCode(500, ResponseDto<PagedResultDto<WorkflowExecutionDto>>.Failure("An error occurred while retrieving executions"));
            }
        }

        /// <summary>
        /// Cancel a running execution
        /// </summary>
        [HttpPost("{executionId}/cancel")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> CancelExecution(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.CancelExecutionAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while cancelling the execution"));
            }
        }

        /// <summary>
        /// Retry a failed execution
        /// </summary>
        [HttpPost("{executionId}/retry")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> RetryExecution(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.RetryExecutionAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying execution {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while retrying the execution"));
            }
        }

        /// <summary>
        /// Get execution logs
        /// </summary>
        [HttpGet("{executionId}/logs")]
        public async Task<ActionResult<ResponseDto<List<ExecutionLogDto>>>> GetExecutionLogs(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionLogsAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution logs for {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<List<ExecutionLogDto>>.Failure("An error occurred while retrieving execution logs"));
            }
        }

        /// <summary>
        /// Get execution details including step-by-step progress
        /// </summary>
        [HttpGet("{executionId}/detail")]
        public async Task<ActionResult<ResponseDto<ExecutionDetailDto>>> GetExecutionDetail(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionDetailAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution detail for {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<ExecutionDetailDto>.Failure("An error occurred while retrieving execution details"));
            }
        }

        /// <summary>
        /// Get execution statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ResponseDto<ExecutionStatisticsDto>>> GetExecutionStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? workflowId = null,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionStatisticsAsync(
                    startDate, endDate, workflowId, projectId, environmentId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution statistics");
                return StatusCode(500, ResponseDto<ExecutionStatisticsDto>.Failure("An error occurred while retrieving execution statistics"));
            }
        }

        /// <summary>
        /// Get running executions
        /// </summary>
        [HttpGet("running")]
        public async Task<ActionResult<ResponseDto<List<WorkflowExecutionDto>>>> GetRunningExecutions(
            [FromQuery] int? workflowId = null,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null)
        {
            try
            {
                var result = await _workflowExecutionService.GetRunningExecutionsAsync(workflowId, projectId, environmentId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting running executions");
                return StatusCode(500, ResponseDto<List<WorkflowExecutionDto>>.Failure("An error occurred while retrieving running executions"));
            }
        }

        /// <summary>
        /// Get recent executions
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<ResponseDto<List<WorkflowExecutionDto>>>> GetRecentExecutions(
            [FromQuery] int count = 10,
            [FromQuery] int? workflowId = null,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null,
            [FromQuery] ExecutionStatus? status = null)
        {
            try
            {
                var result = await _workflowExecutionService.GetRecentExecutionsAsync(
                    count, workflowId, projectId, environmentId, status);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent executions");
                return StatusCode(500, ResponseDto<List<WorkflowExecutionDto>>.Failure("An error occurred while retrieving recent executions"));
            }
        }

        /// <summary>
        /// Get execution timeline for visualization
        /// </summary>
        [HttpGet("{executionId}/timeline")]
        public async Task<ActionResult<ResponseDto<ExecutionTimelineDto>>> GetExecutionTimeline(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionTimelineAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution timeline for {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<ExecutionTimelineDto>.Failure("An error occurred while retrieving execution timeline"));
            }
        }

        /// <summary>
        /// Get step execution details
        /// </summary>
        [HttpGet("{executionId}/steps/{stepId}")]
        public async Task<ActionResult<ResponseDto<StepExecutionDetailDto>>> GetStepExecutionDetail(
            string executionId, 
            int stepId)
        {
            try
            {
                var result = await _workflowExecutionService.GetStepExecutionDetailAsync(executionId, stepId);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting step execution detail for execution {ExecutionId}, step {StepId}", executionId, stepId);
                return StatusCode(500, ResponseDto<StepExecutionDetailDto>.Failure("An error occurred while retrieving step execution details"));
            }
        }

        /// <summary>
        /// Bulk cancel executions
        /// </summary>
        [HttpPost("bulk-cancel")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<BulkExecutionActionResultDto>>> BulkCancelExecutions(
            [FromBody] BulkExecutionActionDto actionDto)
        {
            try
            {
                var result = await _workflowExecutionService.BulkCancelExecutionsAsync(actionDto.ExecutionIds);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk cancelling executions");
                return StatusCode(500, ResponseDto<BulkExecutionActionResultDto>.Failure("An error occurred while bulk cancelling executions"));
            }
        }

        /// <summary>
        /// Bulk retry failed executions
        /// </summary>
        [HttpPost("bulk-retry")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<BulkExecutionActionResultDto>>> BulkRetryExecutions(
            [FromBody] BulkExecutionActionDto actionDto)
        {
            try
            {
                var result = await _workflowExecutionService.BulkRetryExecutionsAsync(actionDto.ExecutionIds);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk retrying executions");
                return StatusCode(500, ResponseDto<BulkExecutionActionResultDto>.Failure("An error occurred while bulk retrying executions"));
            }
        }

        /// <summary>
        /// Get execution performance metrics
        /// </summary>
        [HttpGet("{executionId}/performance")]
        public async Task<ActionResult<ResponseDto<ExecutionPerformanceDto>>> GetExecutionPerformance(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionPerformanceAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution performance for {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<ExecutionPerformanceDto>.Failure("An error occurred while retrieving execution performance"));
            }
        }

        /// <summary>
        /// Export execution data
        /// </summary>
        [HttpPost("export")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult> ExportExecutions([FromBody] ExportExecutionsDto exportDto)
        {
            try
            {
                var result = await _workflowExecutionService.ExportExecutionsAsync(exportDto);
                
                if (!result.Success)
                {
                    return BadRequest(ResponseDto<object>.Failure(result.Message ?? "Export failed"));
                }

                var contentType = exportDto.Format.ToLower() switch
                {
                    "csv" => "text/csv",
                    "json" => "application/json",
                    "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };

                var fileName = $"executions_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportDto.Format.ToLower()}";

                return File(result.Data!, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting executions");
                return StatusCode(500, "An error occurred while exporting executions");
            }
        }

        /// <summary>
        /// Get execution queue status
        /// </summary>
        [HttpGet("queue")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<ExecutionQueueStatusDto>>> GetExecutionQueueStatus()
        {
            try
            {
                var result = await _workflowExecutionService.GetExecutionQueueStatusAsync();
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution queue status");
                return StatusCode(500, ResponseDto<ExecutionQueueStatusDto>.Failure("An error occurred while retrieving execution queue status"));
            }
        }

        /// <summary>
        /// Pause execution (if supported)
        /// </summary>
        [HttpPost("{executionId}/pause")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> PauseExecution(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.PauseExecutionAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing execution {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while pausing the execution"));
            }
        }

        /// <summary>
        /// Resume paused execution
        /// </summary>
        [HttpPost("{executionId}/resume")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> ResumeExecution(string executionId)
        {
            try
            {
                var result = await _workflowExecutionService.ResumeExecutionAsync(executionId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming execution {ExecutionId}", executionId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while resuming the execution"));
            }
        }
    }

    /// <summary>
    /// Execution DTOs
    /// </summary>
    public class ExecutionDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public ExecutionStatus Status { get; set; }
        public DateTime ExecutedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public string? ExecutionContext { get; set; }
        public string? InputParameters { get; set; }
        public string? OutputParameters { get; set; }
        public string? ErrorMessage { get; set; }
        public List<StepExecutionSummaryDto> Steps { get; set; } = new();
        public List<ExecutionLogDto> Logs { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class StepExecutionSummaryDto
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int Order { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsOptional { get; set; }
        public int RetryCount { get; set; }
    }

    public class StepExecutionDetailDto
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public string? InputParameters { get; set; }
        public string? OutputParameters { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RequestDetails { get; set; }
        public string? ResponseDetails { get; set; }
        public int RetryCount { get; set; }
        public bool IsOptional { get; set; }
        public List<ExecutionLogDto> Logs { get; set; } = new();
    }

    public class ExecutionStatisticsDto
    {
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int RunningExecutions { get; set; }
        public int CancelledExecutions { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public TimeSpan? MedianExecutionTime { get; set; }
        public TimeSpan? MinExecutionTime { get; set; }
        public TimeSpan? MaxExecutionTime { get; set; }
        public Dictionary<ExecutionStatus, int> StatusDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyExecutionTrend { get; set; } = new();
        public Dictionary<int, int> HourlyExecutionDistribution { get; set; } = new();
        public List<TopFailingWorkflowDto> TopFailingWorkflows { get; set; } = new();
        public List<SlowestExecutionDto> SlowestExecutions { get; set; } = new();
    }

    public class TopFailingWorkflowDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public int TotalExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double FailureRate { get; set; }
        public string? MostCommonError { get; set; }
    }

    public class SlowestExecutionDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public ExecutionStatus Status { get; set; }
    }

    public class ExecutionTimelineDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? TotalDuration { get; set; }
        public List<TimelineEventDto> Events { get; set; } = new();
        public List<StepTimelineDto> Steps { get; set; } = new();
    }

    public class TimelineEventDto
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string Severity { get; set; } = "Info"; // Info, Warning, Error
    }

    public class StepTimelineDto
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public ExecutionStatus Status { get; set; }
        public List<TimelineEventDto> Events { get; set; } = new();
    }

    public class ExecutionPerformanceDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan NetworkTime { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public TimeSpan WaitTime { get; set; }
        public List<StepPerformanceDto> StepPerformances { get; set; } = new();
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
        public List<string> PerformanceInsights { get; set; } = new();
        public List<string> Bottlenecks { get; set; } = new();
    }

    public class StepPerformanceDto
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public TimeSpan NetworkLatency { get; set; }
        public int ResponseSize { get; set; }
        public int RetryCount { get; set; }
        public double PerformanceScore { get; set; }
    }

    public class BulkExecutionActionDto
    {
        public List<string> ExecutionIds { get; set; } = new();
    }

    public class BulkExecutionActionResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessfulActions { get; set; }
        public int FailedActions { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> ProcessedExecutionIds { get; set; } = new();
        public List<string> FailedExecutionIds { get; set; } = new();
    }

    public class ExportExecutionsDto
    {
        public string Format { get; set; } = "csv"; // csv, json, xlsx
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? WorkflowIds { get; set; }
        public List<ExecutionStatus>? Statuses { get; set; }
        public bool IncludeLogs { get; set; } = false;
        public bool IncludeStepDetails { get; set; } = false;
        public int MaxRecords { get; set; } = 10000;
    }

    public class ExecutionQueueStatusDto
    {
        public int QueuedExecutions { get; set; }
        public int RunningExecutions { get; set; }
        public int CompletedExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int MaxConcurrentExecutions { get; set; }
        public int AvailableCapacity { get; set; }
        public TimeSpan? AverageQueueTime { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public List<QueuedExecutionDto> QueuedItems { get; set; } = new();
        public List<RunningExecutionSummaryDto> RunningItems { get; set; } = new();
        public Dictionary<string, object> SystemResources { get; set; } = new();
    }

    public class QueuedExecutionDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime QueuedAt { get; set; }
        public TimeSpan QueueTime { get; set; }
        public int Priority { get; set; }
        public string? ExecutionContext { get; set; }
    }

    public class RunningExecutionSummaryDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public TimeSpan RunningTime { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string? CurrentStepName { get; set; }
        public double ProgressPercentage { get; set; }
    }
}