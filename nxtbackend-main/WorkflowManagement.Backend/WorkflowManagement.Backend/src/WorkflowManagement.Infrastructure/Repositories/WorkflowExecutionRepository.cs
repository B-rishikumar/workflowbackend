using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;
using System.Linq.Expressions;

namespace WorkflowManagement.Infrastructure.Repositories;

public class WorkflowExecutionRepository : GenericRepository<WorkflowExecution>, IWorkflowExecutionRepository
{
    public WorkflowExecutionRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.WorkflowId == workflowId)
            .Include(we => we.ExecutedBy)
            .Include(we => we.Workflow)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.ExecutedById == userId)
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
                .ThenInclude(p => p.Workspace)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByStatusAsync(ExecutionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.Status == status)
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowExecution?> GetWithLogsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(we => we.Logs.OrderBy(l => l.Timestamp))
                .ThenInclude(l => l.WorkflowStep)
                .ThenInclude(ws => ws.ApiEndpoint)
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Steps.OrderBy(s => s.Order))
                .ThenInclude(s => s.ApiEndpoint)
            .Include(we => we.ExecutedBy)
            .FirstOrDefaultAsync(we => we.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetRecentExecutionsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetRunningExecutionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.Status == ExecutionStatus.Running || we.Status == ExecutionStatus.Pending)
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
            .Include(we => we.ExecutedBy)
            .OrderBy(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Additional methods for comprehensive workflow execution management

    public async Task<IEnumerable<WorkflowExecution>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.CreatedAt >= startDate && we.CreatedAt <= endDate)
            .Include(we => we.Workflow)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByTriggerTypeAsync(
        string triggerType, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.TriggerType == triggerType)
            .Include(we => we.Workflow)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetFailedExecutionsAsync(
        DateTime? since = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(we => we.Status == ExecutionStatus.Failed);

        if (since.HasValue)
        {
            query = query.Where(we => we.CreatedAt >= since.Value);
        }

        return await query
            .Include(we => we.Workflow)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetLongRunningExecutionsAsync(
        TimeSpan threshold, 
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(threshold);

        return await _dbSet
            .Where(we => (we.Status == ExecutionStatus.Running || we.Status == ExecutionStatus.Pending) &&
                        we.StartedAt.HasValue && we.StartedAt.Value <= cutoffTime)
            .Include(we => we.Workflow)
            .Include(we => we.ExecutedBy)
            .OrderBy(we => we.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<WorkflowExecution> Executions, int TotalCount)> GetPagedByWorkflowAsync(
        Guid workflowId,
        int pageNumber,
        int pageSize,
        ExecutionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(we => we.WorkflowId == workflowId);

        if (status.HasValue)
        {
            query = query.Where(we => we.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var executions = await query
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (executions, totalCount);
    }

    public async Task<(IEnumerable<WorkflowExecution> Executions, int TotalCount)> GetPagedByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        ExecutionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(we => we.ExecutedById == userId);

        if (status.HasValue)
        {
            query = query.Where(we => we.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var executions = await query
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
            .OrderByDescending(we => we.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (executions, totalCount);
    }

    public async Task<Dictionary<ExecutionStatus, int>> GetExecutionCountsByStatusAsync(
        Guid? workflowId = null,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (workflowId.HasValue)
        {
            query = query.Where(we => we.WorkflowId == workflowId.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(we => we.CreatedAt >= since.Value);
        }

        return await query
            .GroupBy(we => we.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetExecutionsForMetricsAsync(
        Guid workflowId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _dbSet
            .Where(we => we.WorkflowId == workflowId &&
                        we.CreatedAt >= startOfDay &&
                        we.CreatedAt <= endOfDay)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowExecution?> GetLatestExecutionAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.WorkflowId == workflowId)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WorkflowExecution?> GetLatestSuccessfulExecutionAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.WorkflowId == workflowId && we.Status == ExecutionStatus.Completed)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetExecutionsByEnvironmentAsync(
        Guid environmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.Workflow.EnvironmentId == environmentId)
            .Include(we => we.Workflow)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetExecutionsByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.Workflow.Environment.ProjectId == projectId)
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetExecutionsByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(we => we.Workflow.Environment.Project.WorkspaceId == workspaceId)
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetExecutionCountAsync(
        Guid? workflowId = null,
        Guid? userId = null,
        ExecutionStatus? status = null,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (workflowId.HasValue)
        {
            query = query.Where(we => we.WorkflowId == workflowId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(we => we.ExecutedById == userId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(we => we.Status == status.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(we => we.CreatedAt >= since.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<TimeSpan?> GetAverageExecutionTimeAsync(
        Guid workflowId,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(we => we.WorkflowId == workflowId &&
                        we.Status == ExecutionStatus.Completed &&
                        we.StartedAt.HasValue &&
                        we.CompletedAt.HasValue);

        if (since.HasValue)
        {
            query = query.Where(we => we.CreatedAt >= since.Value);
        }

        var executions = await query
            .Select(we => new { we.StartedAt, we.CompletedAt })
            .ToListAsync(cancellationToken);

        if (!executions.Any())
            return null;

        var durations = executions
            .Where(e => e.StartedAt.HasValue && e.CompletedAt.HasValue)
            .Select(e => e.CompletedAt!.Value - e.StartedAt!.Value)
            .ToList();

        if (!durations.Any())
            return null;

        var averageTicks = (long)durations.Average(d => d.Ticks);
        return new TimeSpan(averageTicks);
    }

    public async Task<bool> HasConcurrentExecutionsAsync(
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        var runningCount = await _dbSet
            .CountAsync(we => we.WorkflowId == workflowId &&
                             (we.Status == ExecutionStatus.Running || we.Status == ExecutionStatus.Pending),
                       cancellationToken);

        return runningCount > 1;
    }

    public async Task<IEnumerable<WorkflowExecution>> SearchExecutionsAsync(
        string searchTerm,
        Guid? workflowId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        var query = _dbSet.AsQueryable();

        if (workflowId.HasValue)
        {
            query = query.Where(we => we.WorkflowId == workflowId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(we => we.ExecutedById == userId.Value);
        }

        return await query
            .Where(we => we.Workflow.Name.ToLower().Contains(lowerSearchTerm) ||
                        (we.TriggerSource != null && we.TriggerSource.ToLower().Contains(lowerSearchTerm)) ||
                        (we.ErrorMessage != null && we.ErrorMessage.ToLower().Contains(lowerSearchTerm)))
            .Include(we => we.Workflow)
            .Include(we => we.ExecutedBy)
            .OrderByDescending(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateExecutionStatusAsync(
        Guid executionId,
        ExecutionStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var execution = await _dbSet.FindAsync(new object[] { executionId }, cancellationToken);
        if (execution == null)
            return false;

        execution.Status = status;
        execution.UpdatedAt = DateTime.UtcNow;

        if (status == ExecutionStatus.Running && !execution.StartedAt.HasValue)
        {
            execution.StartedAt = DateTime.UtcNow;
        }

        if (status == ExecutionStatus.Completed || status == ExecutionStatus.Failed || status == ExecutionStatus.Cancelled)
        {
            execution.CompletedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            execution.ErrorMessage = errorMessage;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateExecutionProgressAsync(
        Guid executionId,
        int completedSteps,
        int failedSteps,
        CancellationToken cancellationToken = default)
    {
        var execution = await _dbSet.FindAsync(new object[] { executionId }, cancellationToken);
        if (execution == null)
            return false;

        execution.CompletedSteps = completedSteps;
        execution.FailedSteps = failedSteps;
        execution.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Override the base GetPagedAsync to include navigation properties by default
    public override async Task<(IEnumerable<WorkflowExecution> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<WorkflowExecution, bool>>? predicate = null,
        Expression<Func<WorkflowExecution, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(we => we.Workflow)
                .ThenInclude(w => w.Environment)
                .ThenInclude(e => e.Project)
            .Include(we => we.ExecutedBy)
            .AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }
        else
        {
            query = query.OrderByDescending(e => e.CreatedAt);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}