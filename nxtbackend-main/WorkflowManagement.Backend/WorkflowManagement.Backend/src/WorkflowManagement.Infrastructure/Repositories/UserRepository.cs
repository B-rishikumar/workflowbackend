using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.Username == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetRecentExecutionsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(we => we.Workflow)
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
            .Include(we => we.ExecutedBy)
            .OrderBy(we => we.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
