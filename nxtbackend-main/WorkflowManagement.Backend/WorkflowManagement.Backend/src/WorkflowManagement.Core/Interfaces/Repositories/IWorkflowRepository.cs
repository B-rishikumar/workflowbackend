// IWorkflowRepository.cs
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Entities;
namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IWorkflowRepository : IGenericRepository<Workflow>
{
    Task<IEnumerable<Workflow>> GetByEnvironmentIdAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> GetByStatusAsync(WorkflowStatus status, CancellationToken cancellationToken = default);
    Task<Workflow?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workflow?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> GetPublishedWorkflowsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> GetWorkflowsWithActiveSchedulesAsync(CancellationToken cancellationToken = default);
}