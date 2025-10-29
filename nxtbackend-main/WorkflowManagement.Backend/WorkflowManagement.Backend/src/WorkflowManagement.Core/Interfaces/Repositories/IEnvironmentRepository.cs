// IEnvironmentRepository.cs
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IEnvironmentRepository : IGenericRepository<WorkflowEnvironment>
{
    Task<IEnumerable<WorkflowEnvironment>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<WorkflowEnvironment?> GetWithWorkflowsAsync(Guid id, CancellationToken cancellationToken = default);
}
