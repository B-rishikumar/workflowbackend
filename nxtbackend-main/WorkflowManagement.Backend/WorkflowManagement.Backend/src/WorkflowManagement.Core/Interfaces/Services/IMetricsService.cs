using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface IMetricsService
{
    Task<WorkflowMetrics?> GetMetricsAsync(Guid workflowId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowMetrics>> GetMetricsRangeAsync(Guid workflowId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetDashboardMetricsAsync(Guid? workflowId = null, CancellationToken cancellationToken = default);
    Task UpdateMetricsAsync(Guid workflowId, DateTime date, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetExecutionMetricsAsync(Guid executionId, CancellationToken cancellationToken = default);
}
