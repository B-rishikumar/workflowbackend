
// ProjectRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.WorkspaceId == workspaceId)
            .Include(p => p.Owner)
            .Include(p => p.Workspace)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.Workspace)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetWithEnvironmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Environments.Where(e => e.IsActive))
                .ThenInclude(e => e.Workflows.Where(w => !w.IsDeleted))
            .Include(p => p.Owner)
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}