using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class EnvironmentRepository : GenericRepository<Core.Entities.WorkflowEnvironment>, IEnvironmentRepository
{
    public EnvironmentRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Core.Entities.WorkflowEnvironment>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ProjectId == projectId)
            .Include(e => e.Project)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Core.Entities.WorkflowEnvironment?> GetWithWorkflowsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Workflows.Where(w => !w.IsDeleted))
                .ThenInclude(w => w.Owner)
            .Include(e => e.Project)
                .ThenInclude(p => p.Workspace)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
