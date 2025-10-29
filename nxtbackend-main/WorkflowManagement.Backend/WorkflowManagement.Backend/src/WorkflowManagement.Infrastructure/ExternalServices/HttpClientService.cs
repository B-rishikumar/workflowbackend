// HttpClientService.cs
using System.Text;
using System.Text.Json;

namespace WorkflowManagement.Infrastructure.ExternalServices;

public class HttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientService> _logger;

    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            SetHeaders(headers);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(content);
            }

            _logger.LogWarning("HTTP GET request failed. Status: {StatusCode}, URL: {Url}", response.StatusCode, url);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during HTTP GET request to {Url}", url);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            SetHeaders(headers);
            
            StringContent? content = null;
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(responseContent);
            }

            _logger.LogWarning("HTTP POST request failed. Status: {StatusCode}, URL: {Url}", response.StatusCode, url);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during HTTP POST request to {Url}", url);
            throw;
        }
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during HTTP request to {Url}", request.RequestUri);
            throw;
        }
    }

    private void SetHeaders(Dictionary<string, string>? headers)
    {
        if (headers == null) return;

        _httpClient.DefaultRequestHeaders.Clear();
        foreach (var header in headers)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }
}
