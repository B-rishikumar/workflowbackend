// SoapClientService.cs
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace WorkflowManagement.Infrastructure.ExternalServices;

public class SoapClientService
{
    private readonly ILogger<SoapClientService> _logger;

    public SoapClientService(ILogger<SoapClientService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CallSoapServiceAsync(
        string endpoint, 
        string action, 
        string soapBody, 
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var soapEnvelope = CreateSoapEnvelope(soapBody);
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("SOAPAction", action);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            var response = await httpClient.PostAsync(endpoint, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return ExtractSoapBody(responseContent);
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"SOAP call failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SOAP service at {Endpoint}", endpoint);
            throw;
        }
    }

    private string CreateSoapEnvelope(string body)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        {body}
    </soap:Body>
</soap:Envelope>";
    }

    private string ExtractSoapBody(string soapResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

            var bodyNode = doc.SelectSingleNode("//soap:Body", nsManager);
            return bodyNode?.InnerXml ?? soapResponse;
        }
        catch
        {
            return soapResponse;
        }
    }
}