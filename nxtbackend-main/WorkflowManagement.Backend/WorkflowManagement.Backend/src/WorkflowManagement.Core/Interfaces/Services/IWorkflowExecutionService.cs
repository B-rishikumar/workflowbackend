// IWorkflowExecutionService.cs
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface IWorkflowExecutionService
{
    Task<WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkflowExecution?> GetWithLogsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WorkflowExecution> Executions, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? workflowId = null, ExecutionStatus? status = null, CancellationToken cancellationToken = default);
    Task<WorkflowExecution> ExecuteAsync(Guid workflowId, Guid userId, Dictionary<string, object>? inputData = null, string triggerType = "manual", CancellationToken cancellationToken = default);
    Task<bool> CancelExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);
    Task<bool> RetryExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExecutionLog>> GetExecutionLogsAsync(Guid executionId, CancellationToken cancellationToken = default);


}