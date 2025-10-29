// IMetricsRepository.cs
using WorkflowManagement.Core.Entities;
namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IMetricsRepository : IGenericRepository<WorkflowMetrics>
{
    Task<IEnumerable<WorkflowMetrics>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowMetrics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<WorkflowMetrics?> GetByWorkflowAndDateAsync(Guid workflowId, DateTime date, CancellationToken cancellationToken = default);
}