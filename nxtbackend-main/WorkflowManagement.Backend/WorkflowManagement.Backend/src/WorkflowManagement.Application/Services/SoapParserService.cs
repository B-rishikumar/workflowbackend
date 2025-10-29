// SoapParserService.cs
using Microsoft.Extensions.Logging;
using System.Xml;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Application.Services;

public class SoapParserService : ISoapParserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SoapParserService> _logger;

    public SoapParserService(IHttpClientFactory httpClientFactory, ILogger<SoapParserService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<ApiEndpoint>> ParseWsdlUrlAsync(string wsdlUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync(wsdlUrl, cancellationToken);
            return await ParseWsdlContentAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing WSDL from URL: {WsdlUrl}", wsdlUrl);
            throw new InvalidOperationException($"Failed to parse WSDL from URL: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ApiEndpoint>> ParseWsdlContentAsync(string wsdlContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoints = new List<ApiEndpoint>();
            var doc = new XmlDocument();
            doc.LoadXml(wsdlContent);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/wsdl/soap/");

            // Get service information
            var serviceNodes = doc.SelectNodes("//wsdl:service", namespaceManager);
            foreach (XmlNode serviceNode in serviceNodes ?? new XmlNodeList())
            {
                var serviceName = serviceNode.Attributes?["name"]?.Value ?? "Unknown Service";

                // Get port information
                var portNodes = serviceNode.SelectNodes(".//wsdl:port", namespaceManager);
                foreach (XmlNode portNode in portNodes ?? new XmlNodeList())
                {
                    var portName = portNode.Attributes?["name"]?.Value ?? "Unknown Port";
                    var binding = portNode.Attributes?["binding"]?.Value;

                    // Get SOAP address
                    var addressNode = portNode.SelectSingleNode(".//soap:address", namespaceManager);
                    var soapAddress = addressNode?.Attributes?["location"]?.Value ?? "";

                    // Get operations from binding
                    var operations = GetOperationsFromBinding(doc, binding, namespaceManager);

                    foreach (var operation in operations)
                    {
                        var endpoint = new ApiEndpoint
                        {
                            Name = $"{serviceName}.{operation.Name}",
                            Description = $"SOAP operation {operation.Name} from service {serviceName}",
                            Url = soapAddress,
                            Method = HttpMethod.POST,
                            Type = ApiEndpointType.SOAP,
                            RequestContentType = "text/xml; charset=utf-8",
                            ResponseContentType = "text/xml",
                            TimeoutSeconds = 30,
                            IsActive = true,
                            SoapWsdl = wsdlContent,
                            Headers = new Dictionary<string, string>
                            {
                                ["SOAPAction"] = operation.SoapAction ?? ""
                            },
                            Parameters = operation.Parameters
                        };

                        endpoints.Add(endpoint);
                    }
                }
            }

            return endpoints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing WSDL content");
            throw new InvalidOperationException($"Failed to parse WSDL content: {ex.Message}");
        }
    }

    public async Task<bool> ValidateWsdlAsync(string wsdl, CancellationToken cancellationToken = default)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(wsdl);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");

            // Check for required WSDL elements
            var definitionsNode = doc.SelectSingleNode("//wsdl:definitions", namespaceManager);
            return definitionsNode != null;
        }
        catch
        {
            return false;
        }
    }

    private List<SoapOperation> GetOperationsFromBinding(XmlDocument doc, string? bindingName, XmlNamespaceManager namespaceManager)
    {
        var operations = new List<SoapOperation>();

        if (string.IsNullOrEmpty(bindingName))
            return operations;

        // Remove namespace prefix if present
        var localBindingName = bindingName.Contains(':') ? bindingName.Split(':')[1] : bindingName;

        var bindingNode = doc.SelectSingleNode($"//wsdl:binding[@name='{localBindingName}']", namespaceManager);
        if (bindingNode == null)
            return operations;

        var operationNodes = bindingNode.SelectNodes(".//wsdl:operation", namespaceManager);
        foreach (XmlNode operationNode in operationNodes ?? new XmlNodeList())
        {
            var operationName = operationNode.Attributes?["name"]?.Value ?? "";
            var soapOperationNode = operationNode.SelectSingleNode(".//soap:operation", namespaceManager);
            var soapAction = soapOperationNode?.Attributes?["soapAction"]?.Value;

            var operation = new SoapOperation
            {
                Name = operationName,
                SoapAction = soapAction,
                Parameters = new List<ApiParameter>()
            };

            operations.Add(operation);
        }

        return operations;
    }

    private class SoapOperation
    {
        public string Name { get; set; } = string.Empty;
        public string? SoapAction { get; set; }
        public List<ApiParameter> Parameters { get; set; } = new();
    }
}