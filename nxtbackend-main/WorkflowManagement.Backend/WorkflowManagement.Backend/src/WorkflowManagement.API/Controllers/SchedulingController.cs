using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;
using WorkflowManagement.Application.Services;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchedulingController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;
        private readonly ILogger<SchedulingController> _logger;

        public SchedulingController(ISchedulingService schedulingService, ILogger<SchedulingController> logger)
        {
            _schedulingService = schedulingService;
            _logger = logger;
        }

        /// <summary>
        /// Create a schedule for a workflow
        /// </summary>
        [HttpPost("workflow/{workflowId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<WorkflowScheduleDto>>> CreateSchedule(
            int workflowId,
            [FromBody] CreateScheduleDto scheduleDto)
        {
            try
            {
                var result = await _schedulingService.CreateScheduleAsync(
                    workflowId,
                    scheduleDto.CronExpression,
                    scheduleDto.TimeZone,
                    scheduleDto.StartDate,
                    scheduleDto.EndDate,
                    scheduleDto.Description);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<WorkflowScheduleDto>.Failure("An error occurred while creating the schedule"));
            }
        }

        /// <summary>
        /// Update a workflow schedule
        /// </summary>
        [HttpPut("workflow/{workflowId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<WorkflowScheduleDto>>> UpdateSchedule(
            int workflowId,
            [FromBody] UpdateScheduleDto scheduleDto)
        {
            try
            {
                var result = await _schedulingService.UpdateScheduleAsync(
                    workflowId,
                    scheduleDto.CronExpression,
                    scheduleDto.TimeZone,
                    scheduleDto.StartDate,
                    scheduleDto.EndDate,
                    scheduleDto.Description);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<WorkflowScheduleDto>.Failure("An error occurred while updating the schedule"));
            }
        }

        /// <summary>
        /// Get workflow schedule
        /// </summary>
        [HttpGet("workflow/{workflowId}")]
        public async Task<ActionResult<ResponseDto<WorkflowScheduleDto>>> GetSchedule(int workflowId)
        {
            try
            {
                var result = await _schedulingService.GetScheduleAsync(workflowId);

                if (result.Success)
                    return Ok(result);

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<WorkflowScheduleDto>.Failure("An error occurred while retrieving the schedule"));
            }
        }

        /// <summary>
        /// Enable workflow schedule
        /// </summary>
        [HttpPost("workflow/{workflowId}/enable")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> EnableSchedule(int workflowId)
        {
            try
            {
                var result = await _schedulingService.EnableScheduleAsync(workflowId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while enabling the schedule"));
            }
        }

        /// <summary>
        /// Disable workflow schedule
        /// </summary>
        [HttpPost("workflow/{workflowId}/disable")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> DisableSchedule(int workflowId)
        {
            try
            {
                var result = await _schedulingService.DisableScheduleAsync(workflowId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while disabling the schedule"));
            }
        }

        /// <summary>
        /// Delete workflow schedule
        /// </summary>
        [HttpDelete("workflow/{workflowId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteSchedule(int workflowId)
        {
            try
            {
                var result = await _schedulingService.DeleteScheduleAsync(workflowId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the schedule"));
            }
        }

        /// <summary>
        /// Get all active schedules
        /// </summary>
        [HttpGet("active")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<List<WorkflowScheduleDto>>>> GetActiveSchedules()
        {
            try
            {
                var result = await _schedulingService.GetActiveSchedulesAsync();

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active schedules");
                return StatusCode(500, ResponseDto<List<WorkflowScheduleDto>>.Failure("An error occurred while retrieving active schedules"));
            }
        }

        /// <summary>
        /// Get schedules due for execution
        /// </summary>
        [HttpGet("due")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<List<WorkflowScheduleDto>>>> GetDueSchedules()
        {
            try
            {
                var result = await _schedulingService.GetDueSchedulesAsync();

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due schedules");
                return StatusCode(500, ResponseDto<List<WorkflowScheduleDto>>.Failure("An error occurred while retrieving due schedules"));
            }
        }

        /// <summary>
        /// Get next run times for a workflow
        /// </summary>
        [HttpGet("workflow/{workflowId}/next-runs")]
        public async Task<ActionResult<ResponseDto<List<DateTime>>>> GetNextRunTimes(
            int workflowId,
            [FromQuery] int count = 5)
        {
            try
            {
                var result = await _schedulingService.GetNextRunTimesAsync(workflowId, count);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next run times for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<List<DateTime>>.Failure("An error occurred while retrieving next run times"));
            }
        }

        /// <summary>
        /// Execute scheduled workflows (typically called by background service)
        /// </summary>
        [HttpPost("execute-scheduled")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<bool>>> ExecuteScheduledWorkflows()
        {
            try
            {
                var result = await _schedulingService.ExecuteScheduledWorkflowsAsync();

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled workflows");
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while executing scheduled workflows"));
            }
        }

        /// <summary>
        /// Validate cron expression
        /// </summary>
        [HttpPost("validate-cron")]
        public async Task<ActionResult<ResponseDto<bool>>> ValidateCronExpression([FromBody] ValidateCronDto cronDto)
        {
            try
            {
                var result = await _schedulingService.ValidateCronExpressionAsync(cronDto.CronExpression);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cron expression");
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while validating the cron expression"));
            }
        }

        /// <summary>
        /// Get schedule statistics
        /// </summary>
        [HttpGet("statistics")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<ScheduleStatisticsDto>>> GetScheduleStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _schedulingService.GetScheduleStatisticsAsync(startDate, endDate);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule statistics");
                return StatusCode(500, ResponseDto<ScheduleStatisticsDto>.Failure("An error occurred while retrieving schedule statistics"));
            }
        }

        /// <summary>
        /// Get schedule history for a workflow
        /// </summary>
        [HttpGet("workflow/{workflowId}/history")]
        public async Task<ActionResult<ResponseDto<List<ScheduleExecutionHistoryDto>>>> GetScheduleHistory(
            int workflowId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _schedulingService.GetScheduleHistoryAsync(workflowId, page, pageSize, startDate, endDate);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule history for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<List<ScheduleExecutionHistoryDto>>.Failure("An error occurred while retrieving schedule history"));
            }
        }

        /// <summary>
        /// Bulk enable/disable schedules
        /// </summary>
        [HttpPost("bulk-action")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<BulkScheduleActionResultDto>>> BulkScheduleAction([FromBody] BulkScheduleActionDto actionDto)
        {
            try
            {
                var result = await _schedulingService.BulkScheduleActionAsync(actionDto.WorkflowIds, actionDto.Action);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk schedule action");
                return StatusCode(500, ResponseDto<BulkScheduleActionResultDto>.Failure("An error occurred while performing bulk schedule action"));
            }
        }

        /// <summary>
        /// Get schedule calendar view
        /// </summary>
        [HttpGet("calendar")]
        public async Task<ActionResult<ResponseDto<List<ScheduleCalendarEventDto>>>> GetScheduleCalendar(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null)
        {
            try
            {
                var result = await _schedulingService.GetScheduleCalendarAsync(startDate, endDate, projectId, environmentId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule calendar");
                return StatusCode(500, ResponseDto<List<ScheduleCalendarEventDto>>.Failure("An error occurred while retrieving schedule calendar"));
            }
        }

        /// <summary>
        /// Pause schedule temporarily
        /// </summary>
        [HttpPost("workflow/{workflowId}/pause")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> PauseSchedule(int workflowId, [FromBody] PauseScheduleDto pauseDto)
        {
            try
            {
                var result = await _schedulingService.PauseScheduleAsync(workflowId, pauseDto.PauseUntil, pauseDto.Reason);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while pausing the schedule"));
            }
        }

        /// <summary>
        /// Resume paused schedule
        /// </summary>
        [HttpPost("workflow/{workflowId}/resume")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> ResumeSchedule(int workflowId)
        {
            try
            {
                var result = await _schedulingService.ResumeScheduleAsync(workflowId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming schedule for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while resuming the schedule"));
            }
        }
    }

    /// <summary>
    /// Scheduling DTOs
    /// </summary>
    public class CreateScheduleDto
    {
        public string CronExpression { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "UTC";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateScheduleDto
    {
        public string CronExpression { get; set; } = string.Empty;
        public string? TimeZone { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class ValidateCronDto
    {
        public string CronExpression { get; set; } = string.Empty;
    }

    public class BulkScheduleActionDto
    {
        public List<int> WorkflowIds { get; set; } = new();
        public string Action { get; set; } = string.Empty; // enable, disable, delete
    }

    public class PauseScheduleDto
    {
        public DateTime? PauseUntil { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Response DTOs
    /// </summary>
    public class ScheduleStatisticsDto
    {
        public int TotalSchedules { get; set; }
        public int ActiveSchedules { get; set; }
        public int InactiveSchedules { get; set; }
        public int PausedSchedules { get; set; }
        public int ScheduledExecutionsToday { get; set; }
        public int SuccessfulScheduledExecutions { get; set; }
        public int FailedScheduledExecutions { get; set; }
        public double ScheduleReliability { get; set; }
        public Dictionary<string, int> ScheduleFrequencyDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyScheduledExecutions { get; set; } = new();
        public List<MostActiveScheduleDto> MostActiveSchedules { get; set; } = new();
    }

    public class MostActiveScheduleDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastExecution { get; set; }
        public DateTime? NextExecution { get; set; }
    }

    public class ScheduleExecutionHistoryDto
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public DateTime? ActualExecutionTime { get; set; }
        public ExecutionStatus Status { get; set; }
        public string? ExecutionId { get; set; }
        public TimeSpan? ExecutionDuration { get; set; }
        public string? ErrorMessage { get; set; }
        public bool WasOnTime { get; set; }
        public TimeSpan? Delay { get; set; }
    }

    public class BulkScheduleActionResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessfulActions { get; set; }
        public int FailedActions { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<int> ProcessedWorkflowIds { get; set; } = new();
        public List<int> FailedWorkflowIds { get; set; } = new();
    }

    public class ScheduleCalendarEventDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public string Status { get; set; } = string.Empty; // active, paused, disabled
        public TimeSpan? EstimatedDuration { get; set; }
    }
}