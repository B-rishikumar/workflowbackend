// IWorkflowService.cs
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface IWorkflowService
{
    Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workflow?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> GetByEnvironmentIdAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Workflow> Workflows, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? environmentId = null, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<Workflow> CreateAsync(Workflow workflow, CancellationToken cancellationToken = default);
    Task<Workflow> UpdateAsync(Workflow workflow, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workflow> PublishAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Workflow> UnpublishAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkflowVersion> CreateVersionAsync(Guid workflowId, string description, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowVersion>> GetVersionsAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<bool> ValidateWorkflowAsync(Guid id, CancellationToken cancellationToken = default);
}