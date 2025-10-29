// IApiEndpointService.cs
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface IApiEndpointService
{
    Task<ApiEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiEndpoint?> GetWithParametersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiEndpoint>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<ApiEndpoint> Endpoints, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, ApiEndpointType? type = null, CancellationToken cancellationToken = default);
    Task<ApiEndpoint> CreateAsync(ApiEndpoint endpoint, CancellationToken cancellationToken = default);
    Task<ApiEndpoint> UpdateAsync(ApiEndpoint endpoint, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TestEndpointAsync(Guid id, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    Task<(bool Success, object? Response)> ExecuteEndpointAsync(Guid id, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
}
