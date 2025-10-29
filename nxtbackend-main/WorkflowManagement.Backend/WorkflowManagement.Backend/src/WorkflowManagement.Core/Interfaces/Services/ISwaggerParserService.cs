// ISwaggerParserService.cs
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface ISwaggerParserService
{
    Task<IEnumerable<ApiEndpoint>> ParseSwaggerUrlAsync(string swaggerUrl, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiEndpoint>> ParseSwaggerJsonAsync(string swaggerJson, CancellationToken cancellationToken = default);
    Task<bool> ValidateSwaggerAsync(string swagger, CancellationToken cancellationToken = default);
}