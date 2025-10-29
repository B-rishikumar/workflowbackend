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
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(IMetricsService metricsService, ILogger<MetricsController> logger)
        {
            _metricsService = metricsService;
            _logger = logger;
        }

        /// <summary>
        /// Get workflow metrics
        /// </summary>
        [HttpGet("workflow/{workflowId}")]
        public async Task<ActionResult<ResponseDto<WorkflowMetricsDto>>> GetWorkflowMetrics(
            int workflowId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _metricsService.GetWorkflowMetricsAsync(workflowId, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow metrics for {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<WorkflowMetricsDto>.Failure("An error occurred while retrieving workflow metrics"));
            }
        }

        /// <summary>
        /// Get project metrics
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ResponseDto<List<WorkflowMetricsDto>>>> GetProjectMetrics(
            int projectId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _metricsService.GetProjectMetricsAsync(projectId, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project metrics for {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<List<WorkflowMetricsDto>>.Failure("An error occurred while retrieving project metrics"));
            }
        }

        /// <summary>
        /// Get system-wide metrics
        /// </summary>
        [HttpGet("system")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<SystemMetricsDto>>> GetSystemMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _metricsService.GetSystemMetricsAsync(startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                return StatusCode(500, ResponseDto<SystemMetricsDto>.Failure("An error occurred while retrieving system metrics"));
            }
        }

        /// <summary>
        /// Record custom metric
        /// </summary>
        [HttpPost("workflow/{workflowId}/record")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> RecordMetric(
            int workflowId,
            [FromBody] RecordMetricDto metricDto)
        {
            try
            {
                var result = await _metricsService.RecordMetricsAsync(
                    workflowId, 
                    metricDto.MetricName, 
                    metricDto.Value, 
                    metricDto.Metadata);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while recording the metric"));
            }
        }

        /// <summary>
        /// Get custom metrics for a workflow
        /// </summary>
        [HttpGet("workflow/{workflowId}/custom/{metricName}")]
        public async Task<ActionResult<ResponseDto<List<MetricDataPointDto>>>> GetCustomMetrics(
            int workflowId,
            string metricName,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _metricsService.GetCustomMetricsAsync(workflowId, metricName, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom metrics {MetricName} for workflow {WorkflowId}", metricName, workflowId);
                return StatusCode(500, ResponseDto<List<MetricDataPointDto>>.Failure("An error occurred while retrieving custom metrics"));
            }
        }

        /// <summary>
        /// Get performance insights for a workflow
        /// </summary>
        [HttpGet("workflow/{workflowId}/insights")]
        public async Task<ActionResult<ResponseDto<PerformanceInsightsDto>>> GetPerformanceInsights(
            int workflowId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _metricsService.GetPerformanceInsightsAsync(workflowId, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance insights for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<PerformanceInsightsDto>.Failure("An error occurred while retrieving performance insights"));
            }
        }

        /// <summary>
        /// Get dashboard metrics summary
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ResponseDto<DashboardMetricsDto>>> GetDashboardMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null)
        {
            try
            {
                var result = await _metricsService.GetDashboardMetricsAsync(startDate, endDate, projectId, environmentId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics");
                return StatusCode(500, ResponseDto<DashboardMetricsDto>.Failure("An error occurred while retrieving dashboard metrics"));
            }
        }

        /// <summary>
        /// Get real-time metrics
        /// </summary>
        [HttpGet("realtime")]
        public async Task<ActionResult<ResponseDto<RealtimeMetricsDto>>> GetRealtimeMetrics(
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null)
        {
            try
            {
                var result = await _metricsService.GetRealtimeMetricsAsync(projectId, environmentId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting realtime metrics");
                return StatusCode(500, ResponseDto<RealtimeMetricsDto>.Failure("An error occurred while retrieving realtime metrics"));
            }
        }

        /// <summary>
        /// Get comparative metrics between workflows
        /// </summary>
        [HttpPost("compare")]
        public async Task<ActionResult<ResponseDto<List<WorkflowComparisonDto>>>> CompareWorkflows(
            [FromBody] CompareWorkflowsDto compareDto)
        {
            try
            {
                var result = await _metricsService.CompareWorkflowsAsync(
                    compareDto.WorkflowIds, 
                    compareDto.StartDate, 
                    compareDto.EndDate,
                    compareDto.MetricsToCompare);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing workflows");
                return StatusCode(500, ResponseDto<List<WorkflowComparisonDto>>.Failure("An error occurred while comparing workflows"));
            }
        }

        /// <summary>
        /// Get trending metrics
        /// </summary>
        [HttpGet("trends")]
        public async Task<ActionResult<ResponseDto<TrendingMetricsDto>>> GetTrendingMetrics(
            [FromQuery] string period = "daily", // daily, weekly, monthly
            [FromQuery] int periodCount = 30,
            [FromQuery] int? projectId = null,
            [FromQuery] int? environmentId = null)
        {
            try
            {
                var result = await _metricsService.GetTrendingMetricsAsync(period, periodCount, projectId, environmentId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending metrics");
                return StatusCode(500, ResponseDto<TrendingMetricsDto>.Failure("An error occurred while retrieving trending metrics"));
            }
        }

        /// <summary>
        /// Export metrics data
        /// </summary>
        [HttpPost("export")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult> ExportMetrics([FromBody] ExportMetricsDto exportDto)
        {
            try
            {
                var result = await _metricsService.ExportMetricsAsync(exportDto);
                
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

                var fileName = $"metrics_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportDto.Format.ToLower()}";

                return File(result.Data!, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting metrics");
                return StatusCode(500, "An error occurred while exporting metrics");
            }
        }

        /// <summary>
        /// Get metrics health check
        /// </summary>
        [HttpGet("health")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<MetricsHealthDto>>> GetMetricsHealth()
        {
            try
            {
                var result = await _metricsService.GetMetricsHealthAsync();
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics health");
                return StatusCode(500, ResponseDto<MetricsHealthDto>.Failure("An error occurred while retrieving metrics health"));
            }
        }

        /// <summary>
        /// Configure metric alerts
        /// </summary>
        [HttpPost("alerts")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<MetricAlertDto>>> CreateMetricAlert([FromBody] CreateMetricAlertDto alertDto)
        {
            try
            {
                var result = await _metricsService.CreateMetricAlertAsync(alertDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating metric alert");
                return StatusCode(500, ResponseDto<MetricAlertDto>.Failure("An error occurred while creating the metric alert"));
            }
        }

        /// <summary>
        /// Get metric alerts
        /// </summary>
        [HttpGet("alerts")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<List<MetricAlertDto>>>> GetMetricAlerts(
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var result = await _metricsService.GetMetricAlertsAsync(activeOnly);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metric alerts");
                return StatusCode(500, ResponseDto<List<MetricAlertDto>>.Failure("An error occurred while retrieving metric alerts"));
            }
        }

        /// <summary>
        /// Update metric alert
        /// </summary>
        [HttpPut("alerts/{alertId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<MetricAlertDto>>> UpdateMetricAlert(
            int alertId,
            [FromBody] UpdateMetricAlertDto alertDto)
        {
            try
            {
                var result = await _metricsService.UpdateMetricAlertAsync(alertId, alertDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metric alert {AlertId}", alertId);
                return StatusCode(500, ResponseDto<MetricAlertDto>.Failure("An error occurred while updating the metric alert"));
            }
        }

        /// <summary>
        /// Delete metric alert
        /// </summary>
        [HttpDelete("alerts/{alertId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteMetricAlert(int alertId)
        {
            try
            {
                var result = await _metricsService.DeleteMetricAlertAsync(alertId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metric alert {AlertId}", alertId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the metric alert"));
            }
        }
    }

    /// <summary>
    /// Metrics DTOs
    /// </summary>
    public class RecordMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class DashboardMetricsDto
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int RunningExecutions { get; set; }
        public double OverallSuccessRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public List<WorkflowPerformanceDto> TopPerformingWorkflows { get; set; } = new();
        public List<WorkflowPerformanceDto> TopFailingWorkflows { get; set; } = new();
        public Dictionary<DateTime, int> ExecutionTrend { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public List<RecentExecutionDto> RecentExecutions { get; set; } = new();
    }

    public class RecentExecutionDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public ExecutionStatus Status { get; set; }
        public DateTime ExecutedAt { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
    }

    public class RealtimeMetricsDto
    {
        public int CurrentlyRunning { get; set; }
        public int ExecutionsInLast5Minutes { get; set; }
        public int FailuresInLast5Minutes { get; set; }
        public double Current5MinuteSuccessRate { get; set; }
        public List<RunningExecutionDto> RunningExecutions { get; set; } = new();
        public List<RecentFailureDto> RecentFailures { get; set; } = new();
        public SystemResourceMetricsDto SystemResources { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class RunningExecutionDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public TimeSpan RunningTime { get; set; }
        public int CompletedSteps { get; set; }
        public int TotalSteps { get; set; }
        public string? CurrentStep { get; set; }
    }

    public class RecentFailureDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? FailedStep { get; set; }
    }

    public class SystemResourceMetricsDto
    {
        public double CpuUsagePercent { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double DiskUsagePercent { get; set; }
        public long NetworkBytesReceived { get; set; }
        public long NetworkBytesSent { get; set; }
        public int DatabaseConnectionsActive { get; set; }
        public int DatabaseConnectionsIdle { get; set; }
    }

    public class CompareWorkflowsDto
    {
        public List<int> WorkflowIds { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? MetricsToCompare { get; set; }
    }

    public class WorkflowComparisonDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public Dictionary<string, object> Metrics { get; set; } = new();
        public int Rank { get; set; }
        public string ComparisonNotes { get; set; } = string.Empty;
    }

    public class TrendingMetricsDto
    {
        public string Period { get; set; } = string.Empty;
        public Dictionary<DateTime, TrendDataPointDto> TrendData { get; set; } = new();
        public List<TrendInsightDto> Insights { get; set; } = new();
        public Dictionary<string, double> OverallTrends { get; set; } = new();
    }

    public class TrendDataPointDto
    {
        public DateTime Period { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public int ActiveWorkflows { get; set; }
    }

    public class TrendInsightDto
    {
        public string Type { get; set; } = string.Empty; // improvement, degradation, anomaly
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Impact { get; set; }
        public DateTime DetectedAt { get; set; }
        public List<string> AffectedWorkflows { get; set; } = new();
    }

    public class ExportMetricsDto
    {
        public string Format { get; set; } = "csv"; // csv, json, xlsx
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? WorkflowIds { get; set; }
        public List<string>? MetricTypes { get; set; }
        public bool IncludeDetails { get; set; } = true;
        public int MaxRecords { get; set; } = 10000;
    }

    public class MetricsHealthDto
    {
        public bool IsHealthy { get; set; }
        public DateTime LastMetricRecorded { get; set; }
        public int MetricsInLastHour { get; set; }
        public int MetricsInLastDay { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> SystemStatus { get; set; } = new();
    }

    public class CreateMetricAlertDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? WorkflowId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty; // greater_than, less_than, equals, etc.
        public double Threshold { get; set; }
        public int EvaluationWindowMinutes { get; set; } = 5;
        public List<string> NotificationChannels { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class UpdateMetricAlertDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Condition { get; set; }
        public double? Threshold { get; set; }
        public int? EvaluationWindowMinutes { get; set; }
        public List<string>? NotificationChannels { get; set; }
        public bool? IsActive { get; set; }
    }

    public class MetricAlertDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public double Threshold { get; set; }
        public int EvaluationWindowMinutes { get; set; }
        public List<string> NotificationChannels { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}