// MetricsRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class MetricsRepository : GenericRepository<WorkflowMetrics>, IMetricsRepository
{
    public MetricsRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkflowMetrics>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wm => wm.WorkflowId == workflowId)
            .OrderByDescending(wm => wm.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowMetrics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wm => wm.Date >= startDate.Date && wm.Date <= endDate.Date)
            .Include(wm => wm.Workflow)
            .OrderBy(wm => wm.Date)
            .ThenBy(wm => wm.Workflow.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowMetrics?> GetByWorkflowAndDateAsync(Guid workflowId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(wm => wm.WorkflowId == workflowId && wm.Date == date.Date, cancellationToken);
    }

public async Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(u => u.Role.ToString() == role).ToListAsync(cancellationToken);
    }
}