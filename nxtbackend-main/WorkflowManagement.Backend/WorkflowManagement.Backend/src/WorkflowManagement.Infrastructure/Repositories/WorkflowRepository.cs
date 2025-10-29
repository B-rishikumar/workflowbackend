// WorkflowRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class WorkflowRepository : GenericRepository<Workflow>, IWorkflowRepository
{
    public WorkflowRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Workflow>> GetByEnvironmentIdAsync(Guid environmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.EnvironmentId == environmentId)
            .Include(w => w.Owner)
            .Include(w => w.Environment)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workflow>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.OwnerId == ownerId)
            .Include(w => w.Environment)
                .ThenInclude(e => e.Project)
                .ThenInclude(p => p.Workspace)
            .OrderByDescending(w => w.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workflow>> GetByStatusAsync(WorkflowStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.Status == status)
            .Include(w => w.Owner)
            .Include(w => w.Environment)
            .OrderByDescending(w => w.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Workflow?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Steps.OrderBy(s => s.Order))
                .ThenInclude(s => s.ApiEndpoint)
                .ThenInclude(a => a.Parameters)
            .Include(w => w.Owner)
            .Include(w => w.Environment)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workflow?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Versions.OrderByDescending(v => v.CreatedAt))
            .Include(w => w.Owner)
            .Include(w => w.Environment)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Workflow>> GetPublishedWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.IsPublished && w.Status == WorkflowStatus.Active)
            .Include(w => w.Owner)
            .Include(w => w.Environment)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workflow>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(w => w.Name.ToLower().Contains(lowerSearchTerm) ||
                       (w.Description != null && w.Description.ToLower().Contains(lowerSearchTerm)) ||
                       (w.Tags != null && w.Tags.ToLower().Contains(lowerSearchTerm)))
            .Include(w => w.Owner)
            .Include(w => w.Environment)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }
}