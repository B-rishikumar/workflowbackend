using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;
using WorkflowManagement.Infrastructure.Logging;
using WorkflowManagement.Application.Services;
namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LogsController : ControllerBase
    {
        private readonly IWorkflowExecutionService _workflowExecutionService;
        private readonly ILoggerService _loggerService;
        private readonly ILogger<LogsController> _logger;

        public LogsController(
            IWorkflowExecutionService workflowExecutionService,
            ILoggerService loggerService,
            ILogger<LogsController> logger)
        {
            _workflowExecutionService = workflowExecutionService;
            _loggerService = loggerService;
            _logger = logger;
        }

        /// <summary>
        /// Get execution logs for a specific execution
        /// </summary>
        [HttpGet("execution/{executionId}")]
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
        /// Get logs for a workflow with pagination
        /// </summary>
        [HttpGet("workflow/{workflowId}")]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ExecutionLogDto>>>> GetWorkflowLogs(
            int workflowId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? logLevel = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _loggerService.GetWorkflowLogsAsync(
                    workflowId, page, pageSize, logLevel, startDate, endDate, searchTerm);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow logs for {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<PagedResultDto<ExecutionLogDto>>.Failure("An error occurred while retrieving workflow logs"));
            }
        }

        /// <summary>
        /// Get system logs with advanced filtering
        /// </summary>
        [HttpGet("system")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<SystemLogDto>>>> GetSystemLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? logLevel = null,
            [FromQuery] string? source = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeStackTrace = false)
        {
            try
            {
                var result = await _loggerService.GetSystemLogsAsync(
                    page, pageSize, logLevel, source, startDate, endDate, searchTerm, includeStackTrace);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system logs");
                return StatusCode(500, ResponseDto<PagedResultDto<SystemLogDto>>.Failure("An error occurred while retrieving system logs"));
            }
        }

        /// <summary>
        /// Get audit logs for compliance tracking
        /// </summary>
        [HttpGet("audit")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<AuditLogDto>>>> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _loggerService.GetAuditLogsAsync(
                    page, pageSize, action, entityType, userId, startDate, endDate, searchTerm);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs");
                return StatusCode(500, ResponseDto<PagedResultDto<AuditLogDto>>.Failure("An error occurred while retrieving audit logs"));
            }
        }

        /// <summary>
        /// Get error logs with grouping and analysis
        /// </summary>
        [HttpGet("errors")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ErrorLogDto>>>> GetErrorLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? errorType = null,
            [FromQuery] int? workflowId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool groupSimilar = true)
        {
            try
            {
                var result = await _loggerService.GetErrorLogsAsync(
                    page, pageSize, errorType, workflowId, startDate, endDate, groupSimilar);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting error logs");
                return StatusCode(500, ResponseDto<PagedResultDto<ErrorLogDto>>.Failure("An error occurred while retrieving error logs"));
            }
        }

        /// <summary>
        /// Get performance logs for optimization
        /// </summary>
        [HttpGet("performance")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<PerformanceLogDto>>>> GetPerformanceLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? workflowId = null,
            [FromQuery] int? minExecutionTime = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? sortBy = "ExecutionTime",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var result = await _loggerService.GetPerformanceLogsAsync(
                    page, pageSize, workflowId, minExecutionTime, startDate, endDate, sortBy, sortDescending);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance logs");
                return StatusCode(500, ResponseDto<PagedResultDto<PerformanceLogDto>>.Failure("An error occurred while retrieving performance logs"));
            }
        }

        /// <summary>
        /// Search logs across all types with advanced filtering
        /// </summary>
        [HttpPost("search")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<UnifiedLogDto>>>> SearchLogs([FromBody] LogSearchDto searchDto)
        {
            try
            {
                var result = await _loggerService.SearchLogsAsync(searchDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching logs");
                return StatusCode(500, ResponseDto<PagedResultDto<UnifiedLogDto>>.Failure("An error occurred while searching logs"));
            }
        }

        /// <summary>
        /// Export logs to various formats
        /// </summary>
        [HttpPost("export")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult> ExportLogs([FromBody] LogExportDto exportDto)
        {
            try
            {
                var result = await _loggerService.ExportLogsAsync(exportDto);
                
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

                var fileName = $"logs_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportDto.Format.ToLower()}";

                return File(result.Data!, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting logs");
                return StatusCode(500, "An error occurred while exporting logs");
            }
        }

        /// <summary>
        /// Get log statistics and analytics
        /// </summary>
        [HttpGet("statistics")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<LogStatisticsDto>>> GetLogStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? workflowId = null)
        {
            try
            {
                var result = await _loggerService.GetLogStatisticsAsync(startDate, endDate, workflowId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log statistics");
                return StatusCode(500, ResponseDto<LogStatisticsDto>.Failure("An error occurred while retrieving log statistics"));
            }
        }

        /// <summary>
        /// Get real-time log stream for monitoring
        /// </summary>
        [HttpGet("stream")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult> GetLogStream(
            [FromQuery] string? logLevel = null,
            [FromQuery] int? workflowId = null,
            [FromQuery] string? source = null)
        {
            try
            {
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");

                await _loggerService.StreamLogsAsync(Response.Body, logLevel, workflowId, source, HttpContext.RequestAborted);
                
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming logs");
                return StatusCode(500, "An error occurred while streaming logs");
            }
        }

        /// <summary>
        /// Archive old logs
        /// </summary>
        [HttpPost("archive")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<LogArchiveResultDto>>> ArchiveLogs([FromBody] LogArchiveDto archiveDto)
        {
            try
            {
                var result = await _loggerService.ArchiveLogsAsync(archiveDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving logs");
                return StatusCode(500, ResponseDto<LogArchiveResultDto>.Failure("An error occurred while archiving logs"));
            }
        }

        /// <summary>
        /// Clean up old logs
        /// </summary>
        [HttpDelete("cleanup")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<LogCleanupResultDto>>> CleanupLogs([FromBody] LogCleanupDto cleanupDto)
        {
            try
            {
                var result = await _loggerService.CleanupLogsAsync(cleanupDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up logs");
                return StatusCode(500, ResponseDto<LogCleanupResultDto>.Failure("An error occurred while cleaning up logs"));
            }
        }
    }

    /// <summary>
    /// Log DTOs
    /// </summary>
    public class SystemLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class AuditLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class ErrorLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public int? WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public string? ExecutionId { get; set; }
        public int OccurrenceCount { get; set; }
        public DateTime? LastOccurrence { get; set; }
    }

    public class PerformanceLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string ExecutionId { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public int StepCount { get; set; }
        public string? BottleneckStep { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    public class UnifiedLogDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string LogType { get; set; } = string.Empty;
        public string LogLevel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Source { get; set; }
        public int? WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public string? ExecutionId { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class LogSearchDto
    {
        public string? SearchTerm { get; set; }
        public List<string>? LogTypes { get; set; }
        public List<string>? LogLevels { get; set; }
        public List<string>? Sources { get; set; }
        public List<int>? WorkflowIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;
    }

    public class LogExportDto
    {
        public string Format { get; set; } = "csv"; // csv, json, xlsx
        public List<string>? LogTypes { get; set; }
        public List<string>? LogLevels { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? WorkflowId { get; set; }
        public string? SearchTerm { get; set; }
        public bool IncludeStackTrace { get; set; } = false;
        public int MaxRecords { get; set; } = 10000;
    }

    public class LogStatisticsDto
    {
        public int TotalLogs { get; set; }
        public Dictionary<string, int> LogLevelDistribution { get; set; } = new();
        public Dictionary<string, int> LogTypeDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyLogTrend { get; set; } = new();
        public Dictionary<string, int> TopErrorMessages { get; set; } = new();
        public Dictionary<string, int> TopSources { get; set; } = new();
        public List<WorkflowLogSummaryDto> WorkflowLogSummaries { get; set; } = new();
        public double ErrorRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
    }

    public class WorkflowLogSummaryDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public int LogCount { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
    }

    public class LogArchiveDto
    {
        public DateTime? OlderThan { get; set; }
        public List<string>? LogTypes { get; set; }
        public string ArchiveLocation { get; set; } = string.Empty;
        public bool CompressArchive { get; set; } = true;
    }

    public class LogArchiveResultDto
    {
        public int ArchivedCount { get; set; }
        public long ArchivedSizeBytes { get; set; }
        public string ArchiveLocation { get; set; } = string.Empty;
        public DateTime ArchivedAt { get; set; }
        public TimeSpan ArchiveTime { get; set; }
    }

    public class LogCleanupDto
    {
        public DateTime? OlderThan { get; set; }
        public List<string>? LogTypes { get; set; }
        public List<string>? LogLevels { get; set; }
        public bool ConfirmDeletion { get; set; } = false;
    }

    public class LogCleanupResultDto
    {
        public int DeletedCount { get; set; }
        public long FreedSpaceBytes { get; set; }
        public DateTime CleanedAt { get; set; }
        public TimeSpan CleanupTime { get; set; }
        public List<string> CleanedLogTypes { get; set; } = new();
    }
}