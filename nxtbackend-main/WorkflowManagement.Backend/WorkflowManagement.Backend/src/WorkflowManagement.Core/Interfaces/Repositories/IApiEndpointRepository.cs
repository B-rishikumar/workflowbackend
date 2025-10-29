// IApiEndpointRepository.cs
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Entities;
namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IApiEndpointRepository : IGenericRepository<ApiEndpoint>
{
    Task<ApiEndpoint?> GetWithParametersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiEndpoint>> GetByTypeAsync(ApiEndpointType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiEndpoint>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    Task<bool> UrlExistsAsync(string url,  WFMHttpMethod method, Guid? excludeId = null, CancellationToken cancellationToken = default);
}