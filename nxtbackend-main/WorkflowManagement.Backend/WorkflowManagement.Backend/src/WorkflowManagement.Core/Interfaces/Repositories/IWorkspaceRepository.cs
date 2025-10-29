// IWorkspaceRepository.cs
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IWorkspaceRepository : IGenericRepository<Workspace>
{
    Task<IEnumerable<Workspace>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Workspace?> GetWithProjectsAsync(Guid id, CancellationToken cancellationToken = default);
}
