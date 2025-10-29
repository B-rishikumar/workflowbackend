// IProjectRepository.cs
using WorkflowManagement.Core.Entities; 
namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<IEnumerable<Project>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Project?> GetWithEnvironmentsAsync(Guid id, CancellationToken cancellationToken = default);
}
