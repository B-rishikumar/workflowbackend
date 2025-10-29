// ApiEndpointRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class ApiEndpointRepository : GenericRepository<ApiEndpoint>, IApiEndpointRepository
{
    public ApiEndpointRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<ApiEndpoint?> GetWithParametersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Parameters.OrderBy(p => p.Name))
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ApiEndpoint>> GetByTypeAsync(ApiEndpointType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Type == type && a.IsActive)
            .Include(a => a.Parameters)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApiEndpoint>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(a => a.Name.ToLower().Contains(lowerSearchTerm) ||
                       (a.Description != null && a.Description.ToLower().Contains(lowerSearchTerm)) ||
                       a.Url.ToLower().Contains(lowerSearchTerm))
            .Include(a => a.Parameters)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UrlExistsAsync(string url, HttpMethod method, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.Url == url && a.Method == method);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}