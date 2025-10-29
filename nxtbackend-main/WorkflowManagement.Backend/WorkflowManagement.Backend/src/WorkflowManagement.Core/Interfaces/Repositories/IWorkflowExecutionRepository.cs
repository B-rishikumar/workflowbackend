// IWorkflowExecutionRepository.cs
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Entities;
namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IWorkflowExecutionRepository : IGenericRepository<WorkflowExecution>
{
    Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowExecution>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowExecution>> GetByStatusAsync(ExecutionStatus status, CancellationToken cancellationToken = default);
    Task<WorkflowExecution?> GetWithLogsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowExecution>> GetRecentExecutionsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowExecution>> GetRunningExecutionsAsync(CancellationToken cancellationToken = default);
}
