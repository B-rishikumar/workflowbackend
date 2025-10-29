
// WorkspaceRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class WorkspaceRepository : GenericRepository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Workspace>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.OwnerId == ownerId)
            .Include(w => w.Owner)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Workspace?> GetWithProjectsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Projects.Where(p => p.IsActive))
                .ThenInclude(p => p.Environments.Where(e => e.IsActive))
                .ThenInclude(e => e.Workflows.Where(wf => !wf.IsDeleted))
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }
}