// ISoapParserService.cs
using WorkflowManagement.Core.Entities; 
namespace WorkflowManagement.Core.Interfaces.Services;

public interface ISoapParserService
{
    Task<IEnumerable<ApiEndpoint>> ParseWsdlUrlAsync(string wsdlUrl, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiEndpoint>> ParseWsdlContentAsync(string wsdlContent, CancellationToken cancellationToken = default);
    Task<bool> ValidateWsdlAsync(string wsdl, CancellationToken cancellationToken = default);
}
