using AutoMapper;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Application.DTOs.Common;
using Microsoft.Extensions.Logging;

namespace WorkflowManagement.Application.Services
{
    /// <summary>
    /// Service for collecting and managing workflow metrics
    /// </summary>
    public class MetricsService : IMetricsService
    {
        private readonly IMetricsRepository _metricsRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowExecutionRepository _executionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MetricsService> _logger;

        public MetricsService(
            IMetricsRepository metricsRepository,
            IWorkflowRepository workflowRepository,
            IWorkflowExecutionRepository executionRepository,
            IMapper mapper,
            ILogger<MetricsService> logger)
        {
            _metricsRepository = metricsRepository;
            _workflowRepository = workflowRepository;
            _executionRepository = executionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto<WorkflowMetricsDto>> GetWorkflowMetricsAsync(int workflowId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting metrics for workflow {WorkflowId} from {StartDate} to {EndDate}", workflowId, startDate, endDate);

                var workflow = await _workflowRepository.GetByIdAsync(workflowId);
                if (workflow == null)
                {
                    return ResponseDto<WorkflowMetricsDto>.Failure("Workflow not found");
                }

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var executions = await _executionRepository.GetExecutionsByWorkflowIdAsync(workflowId, startDate.Value, endDate.Value);
                
                var metrics = new WorkflowMetricsDto
                {
                    WorkflowId = workflowId,
                    WorkflowName = workflow.Name,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalExecutions = executions.Count(),
                    SuccessfulExecutions = executions.Count(e => e.Status == ExecutionStatus.Completed),
                    FailedExecutions = executions.Count(e => e.Status == ExecutionStatus.Failed),
                    PendingExecutions = executions.Count(e => e.Status == ExecutionStatus.Running),
                    CancelledExecutions = executions.Count(e => e.Status == ExecutionStatus.Cancelled),
                    AverageExecutionTime = CalculateAverageExecutionTime(executions.Where(e => e.Status == ExecutionStatus.Completed)),
                    MinExecutionTime = CalculateMinExecutionTime(executions.Where(e => e.Status == ExecutionStatus.Completed)),
                    MaxExecutionTime = CalculateMaxExecutionTime(executions.Where(e => e.Status == ExecutionStatus.Completed)),
                    SuccessRate = CalculateSuccessRate(executions),
                    DailyExecutionCounts = CalculateDailyExecutionCounts(executions, startDate.Value, endDate.Value),
                    HourlyExecutionDistribution = CalculateHourlyDistribution(executions),
                    ErrorDistribution = CalculateErrorDistribution(executions.Where(e => e.Status == ExecutionStatus.Failed)),
                    LastExecutionAt = executions.OrderByDescending(e => e.ExecutedAt).FirstOrDefault()?.ExecutedAt,
                    LastSuccessfulExecutionAt = executions.Where(e => e.Status == ExecutionStatus.Completed)
                                                         .OrderByDescending(e => e.ExecutedAt)
                                                         .FirstOrDefault()?.ExecutedAt
                };

                return ResponseDto<WorkflowMetricsDto>.Success(metrics, "Workflow metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for workflow {WorkflowId}", workflowId);
                return ResponseDto<WorkflowMetricsDto>.Failure("An error occurred while retrieving workflow metrics");
            }
        }

        public async Task<ResponseDto<List<WorkflowMetricsDto>>> GetProjectMetricsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting metrics for project {ProjectId}", projectId);

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var workflows = await _workflowRepository.GetWorkflowsByProjectIdAsync(projectId);
                var metricsResults = new List<WorkflowMetricsDto>();

                foreach (var workflow in workflows)
                {
                    var workflowMetricsResult = await GetWorkflowMetricsAsync(workflow.Id, startDate, endDate);
                    if (workflowMetricsResult.Success && workflowMetricsResult.Data != null)
                    {
                        metricsResults.Add(workflowMetricsResult.Data);
                    }
                }

                return ResponseDto<List<WorkflowMetricsDto>>.Success(metricsResults, "Project metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for project {ProjectId}", projectId);
                return ResponseDto<List<WorkflowMetricsDto>>.Failure("An error occurred while retrieving project metrics");
            }
        }

        public async Task<ResponseDto<SystemMetricsDto>> GetSystemMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting system-wide metrics");

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var allExecutions = await _executionRepository.GetExecutionsAsync(startDate.Value, endDate.Value);
                var allWorkflows = await _workflowRepository.GetAllAsync();

                var systemMetrics = new SystemMetricsDto
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalWorkflows = allWorkflows.Count(),
                    ActiveWorkflows = allWorkflows.Count(w => w.Status == WorkflowStatus.Published),
                    DraftWorkflows = allWorkflows.Count(w => w.Status == WorkflowStatus.Draft),
                    TotalExecutions = allExecutions.Count(),
                    SuccessfulExecutions = allExecutions.Count(e => e.Status == ExecutionStatus.Completed),
                    FailedExecutions = allExecutions.Count(e => e.Status == ExecutionStatus.Failed),
                    RunningExecutions = allExecutions.Count(e => e.Status == ExecutionStatus.Running),
                    CancelledExecutions = allExecutions.Count(e => e.Status == ExecutionStatus.Cancelled),
                    OverallSuccessRate = CalculateSuccessRate(allExecutions),
                    AverageExecutionTime = CalculateAverageExecutionTime(allExecutions.Where(e => e.Status == ExecutionStatus.Completed)),
                    TopFailingWorkflows = await GetTopFailingWorkflowsAsync(startDate.Value, endDate.Value),
                    TopPerformingWorkflows = await GetTopPerformingWorkflowsAsync(startDate.Value, endDate.Value),
                    DailyExecutionTrend = CalculateDailyExecutionCounts(allExecutions, startDate.Value, endDate.Value),
                    WorkflowStatusDistribution = CalculateWorkflowStatusDistribution(allWorkflows),
                    ExecutionStatusDistribution = CalculateExecutionStatusDistribution(allExecutions)
                };

                return ResponseDto<SystemMetricsDto>.Success(systemMetrics, "System metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                return ResponseDto<SystemMetricsDto>.Failure("An error occurred while retrieving system metrics");
            }
        }

        public async Task<ResponseDto<bool>> RecordMetricsAsync(int workflowId, string metricName, double value, Dictionary<string, object>? metadata = null)
        {
            try
            {
                _logger.LogInformation("Recording metric {MetricName} for workflow {WorkflowId} with value {Value}", metricName, workflowId, value);

                var metric = new WorkflowMetrics
                {
                    WorkflowId = workflowId,
                    MetricName = metricName,
                    MetricValue = value,
                    RecordedAt = DateTime.UtcNow,
                    Metadata = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null
                };

                await _metricsRepository.AddAsync(metric);

                return ResponseDto<bool>.Success(true, "Metric recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric {MetricName} for workflow {WorkflowId}", metricName, workflowId);
                return ResponseDto<bool>.Failure("An error occurred while recording the metric");
            }
        }

        public async Task<ResponseDto<List<MetricDataPointDto>>> GetCustomMetricsAsync(int workflowId, string metricName, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting custom metrics {MetricName} for workflow {WorkflowId}", metricName, workflowId);

                startDate ??= DateTime.UtcNow.AddDays(-7);
                endDate ??= DateTime.UtcNow;

                var metrics = await _metricsRepository.GetMetricsByWorkflowAndNameAsync(workflowId, metricName, startDate.Value, endDate.Value);
                
                var dataPoints = metrics.Select(m => new MetricDataPointDto
                {
                    Timestamp = m.RecordedAt,
                    Value = m.MetricValue,
                    Metadata = m.Metadata
                }).OrderBy(dp => dp.Timestamp).ToList();

                return ResponseDto<List<MetricDataPointDto>>.Success(dataPoints, "Custom metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom metrics {MetricName} for workflow {WorkflowId}", metricName, workflowId);
                return ResponseDto<List<MetricDataPointDto>>.Failure("An error occurred while retrieving custom metrics");
            }
        }

        public async Task<ResponseDto<PerformanceInsightsDto>> GetPerformanceInsightsAsync(int workflowId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting performance insights for workflow {WorkflowId}", workflowId);

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var executions = await _executionRepository.GetExecutionsByWorkflowIdAsync(workflowId, startDate.Value, endDate.Value);
                var workflow = await _workflowRepository.GetByIdAsync(workflowId);

                if (workflow == null)
                {
                    return ResponseDto<PerformanceInsightsDto>.Failure("Workflow not found");
                }

                var insights = new PerformanceInsightsDto
                {
                    WorkflowId = workflowId,
                    WorkflowName = workflow.Name,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    Recommendations = await GeneratePerformanceRecommendationsAsync(workflowId, executions),
                    Bottlenecks = await IdentifyBottlenecksAsync(workflowId, executions),
                    Trends = CalculatePerformanceTrends(executions),
                    OptimizationScore = CalculateOptimizationScore(executions),
                    Anomalies = DetectAnomalies(executions)
                };

                return ResponseDto<PerformanceInsightsDto>.Success(insights, "Performance insights retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance insights for workflow {WorkflowId}", workflowId);
                return ResponseDto<PerformanceInsightsDto>.Failure("An error occurred while retrieving performance insights");
            }
        }

        #region Private Helper Methods

        private TimeSpan? CalculateAverageExecutionTime(IEnumerable<WorkflowExecution> executions)
        {
            var completedExecutions = executions.Where(e => e.CompletedAt.HasValue).ToList();
            if (!completedExecutions.Any()) return null;

            var totalTicks = completedExecutions.Sum(e => (e.CompletedAt!.Value - e.ExecutedAt).Ticks);
            return new TimeSpan(totalTicks / completedExecutions.Count);
        }

        private TimeSpan? CalculateMinExecutionTime(IEnumerable<WorkflowExecution> executions)
        {
            var times = executions.Where(e => e.CompletedAt.HasValue)
                                 .Select(e => e.CompletedAt!.Value - e.ExecutedAt);
            return times.Any() ? times.Min() : null;
        }

        private TimeSpan? CalculateMaxExecutionTime(IEnumerable<WorkflowExecution> executions)
        {
            var times = executions.Where(e => e.CompletedAt.HasValue)
                                 .Select(e => e.CompletedAt!.Value - e.ExecutedAt);
            return times.Any() ? times.Max() : null;
        }

        private double CalculateSuccessRate(IEnumerable<WorkflowExecution> executions)
        {
            var executionList = executions.ToList();
            if (!executionList.Any()) return 0;

            var successfulCount = executionList.Count(e => e.Status == ExecutionStatus.Completed);
            return (double)successfulCount / executionList.Count * 100;
        }

        private Dictionary<DateTime, int> CalculateDailyExecutionCounts(IEnumerable<WorkflowExecution> executions, DateTime startDate, DateTime endDate)
        {
            var dailyCounts = new Dictionary<DateTime, int>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var count = executions.Count(e => e.ExecutedAt.Date == currentDate);
                dailyCounts[currentDate] = count;
                currentDate = currentDate.AddDays(1);
            }

            return dailyCounts;
        }

        private Dictionary<int, int> CalculateHourlyDistribution(IEnumerable<WorkflowExecution> executions)
        {
            var hourlyDistribution = new Dictionary<int, int>();
            
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyDistribution[hour] = executions.Count(e => e.ExecutedAt.Hour == hour);
            }

            return hourlyDistribution;
        }

        private Dictionary<string, int> CalculateErrorDistribution(IEnumerable<WorkflowExecution> failedExecutions)
        {
            var errorDistribution = new Dictionary<string, int>();
            
            foreach (var execution in failedExecutions)
            {
                var errorType = string.IsNullOrEmpty(execution.ErrorMessage) ? "Unknown Error" : 
                               execution.ErrorMessage.Split(':')[0].Trim();
                
                if (errorDistribution.ContainsKey(errorType))
                    errorDistribution[errorType]++;
                else
                    errorDistribution[errorType] = 1;
            }

            return errorDistribution;
        }

        private async Task<List<WorkflowPerformanceDto>> GetTopFailingWorkflowsAsync(DateTime startDate, DateTime endDate, int count = 5)
        {
            var executions = await _executionRepository.GetExecutionsAsync(startDate, endDate);
            
            return executions.Where(e => e.Status == ExecutionStatus.Failed)
                           .GroupBy(e => e.WorkflowId)
                           .Select(g => new WorkflowPerformanceDto
                           {
                               WorkflowId = g.Key,
                               WorkflowName = g.First().Workflow?.Name ?? "Unknown",
                               ExecutionCount = g.Count(),
                               FailureRate = 100
                           })
                           .OrderByDescending(w => w.ExecutionCount)
                           .Take(count)
                           .ToList();
        }

        private async Task<List<WorkflowPerformanceDto>> GetTopPerformingWorkflowsAsync(DateTime startDate, DateTime endDate, int count = 5)
        {
            var executions = await _executionRepository.GetExecutionsAsync(startDate, endDate);
            
            return executions.GroupBy(e => e.WorkflowId)
                           .Where(g => g.Any())
                           .Select(g => new WorkflowPerformanceDto
                           {
                               WorkflowId = g.Key,
                               WorkflowName = g.First().Workflow?.Name ?? "Unknown",
                               ExecutionCount = g.Count(),
                               SuccessRate = (double)g.Count(e => e.Status == ExecutionStatus.Completed) / g.Count() * 100
                           })
                           .OrderByDescending(w => w.SuccessRate)
                           .ThenByDescending(w => w.ExecutionCount)
                           .Take(count)
                           .ToList();
        }

        private Dictionary<string, int> CalculateWorkflowStatusDistribution(IEnumerable<Workflow> workflows)
        {
            return workflows.GroupBy(w => w.Status.ToString())
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        private Dictionary<string, int> CalculateExecutionStatusDistribution(IEnumerable<WorkflowExecution> executions)
        {
            return executions.GroupBy(e => e.Status.ToString())
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<List<string>> GeneratePerformanceRecommendationsAsync(int workflowId, IEnumerable<WorkflowExecution> executions)
        {
            var recommendations = new List<string>();
            var executionList = executions.ToList();

            if (!executionList.Any()) return recommendations;

            var failureRate = (double)executionList.Count(e => e.Status == ExecutionStatus.Failed) / executionList.Count * 100;
            if (failureRate > 10)
            {
                recommendations.Add($"High failure rate detected ({failureRate:F1}%). Consider reviewing workflow steps and error handling.");
            }

            var avgExecutionTime = CalculateAverageExecutionTime(executionList.Where(e => e.Status == ExecutionStatus.Completed));
            if (avgExecutionTime?.TotalMinutes > 30)
            {
                recommendations.Add("Long average execution time detected. Consider optimizing API calls and adding parallel processing where possible.");
            }

            return recommendations;
        }

        private async Task<List<string>> IdentifyBottlenecksAsync(int workflowId, IEnumerable<WorkflowExecution> executions)
        {
            // This would analyze step-by-step execution times to identify bottlenecks
            // For now, return a simple implementation
            return new List<string>();
        }

        private Dictionary<string, double> CalculatePerformanceTrends(IEnumerable<WorkflowExecution> executions)
        {
            // Calculate various performance trends over time
            return new Dictionary<string, double>();
        }

        private double CalculateOptimizationScore(IEnumerable<WorkflowExecution> executions)
        {
            var executionList = executions.ToList();
            if (!executionList.Any()) return 0;

            var successRate = CalculateSuccessRate(executionList);
            var avgTime = CalculateAverageExecutionTime(executionList.Where(e => e.Status == ExecutionStatus.Completed));
            
            // Simple optimization score calculation
            var score = successRate * 0.7; // 70% weight for success rate
            
            if (avgTime?.TotalMinutes < 5) score += 20; // Bonus for fast execution
            else if (avgTime?.TotalMinutes < 15) score += 10;

            return Math.Min(100, score);
        }

        private List<string> DetectAnomalies(IEnumerable<WorkflowExecution> executions)
        {
            var anomalies = new List<string>();
            var executionList = executions.ToList();

            if (!executionList.Any()) return anomalies;

            // Detect execution time anomalies
            var completedExecutions = executionList.Where(e => e.Status == ExecutionStatus.Completed && e.CompletedAt.HasValue);
            if (completedExecutions.Any())
            {
                var executionTimes = completedExecutions.Select(e => (e.CompletedAt!.Value - e.ExecutedAt).TotalMinutes).ToList();
                var avgTime = executionTimes.Average();
                var stdDev = Math.Sqrt(executionTimes.Average(x => Math.Pow(x - avgTime, 2)));

                var threshold = avgTime + (2 * stdDev); // 2 standard deviations
                var slowExecutions = executionTimes.Where(t => t > threshold).Count();

                if (slowExecutions > 0)
                {
                    anomalies.Add($"Detected {slowExecutions} unusually slow executions (>{threshold:F1} minutes)");
                }
            }

            return anomalies;
        }

        #endregion
    }

    #region DTOs

    public class WorkflowMetricsDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int PendingExecutions { get; set; }
        public int CancelledExecutions { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public TimeSpan? MinExecutionTime { get; set; }
        public TimeSpan? MaxExecutionTime { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<DateTime, int> DailyExecutionCounts { get; set; } = new();
        public Dictionary<int, int> HourlyExecutionDistribution { get; set; } = new();
        public Dictionary<string, int> ErrorDistribution { get; set; } = new();
        public DateTime? LastExecutionAt { get; set; }
        public DateTime? LastSuccessfulExecutionAt { get; set; }
    }

    public class SystemMetricsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int DraftWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int RunningExecutions { get; set; }
        public int CancelledExecutions { get; set; }
        public double OverallSuccessRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public List<WorkflowPerformanceDto> TopFailingWorkflows { get; set; } = new();
        public List<WorkflowPerformanceDto> TopPerformingWorkflows { get; set; } = new();
        public Dictionary<DateTime, int> DailyExecutionTrend { get; set; } = new();
        public Dictionary<string, int> WorkflowStatusDistribution { get; set; } = new();
        public Dictionary<string, int> ExecutionStatusDistribution { get; set; } = new();
    }

    public class WorkflowPerformanceDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public double SuccessRate { get; set; }
        public double FailureRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
    }

    public class MetricDataPointDto
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string? Metadata { get; set; }
    }

    public class PerformanceInsightsDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public List<string> Bottlenecks { get; set; } = new();
        public Dictionary<string, double> Trends { get; set; } = new();
        public double OptimizationScore { get; set; }
        public List<string> Anomalies { get; set; } = new();
    }

    #endregion
}