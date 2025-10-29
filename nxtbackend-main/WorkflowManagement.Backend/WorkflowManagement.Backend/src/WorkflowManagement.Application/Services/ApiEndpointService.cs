// ApiEndpointService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Application.Services;

public class ApiEndpointService : IApiEndpointService
{
    private readonly IApiEndpointRepository _apiEndpointRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<ApiEndpointService> _logger;

    public ApiEndpointService(
        IApiEndpointRepository apiEndpointRepository,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<ApiEndpointService> logger)
    {
        _apiEndpointRepository = apiEndpointRepository;
        _httpClientFactory = httpClientFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _apiEndpointRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiEndpoint?> GetWithParametersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _apiEndpointRepository.GetWithParametersAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<ApiEndpoint>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _apiEndpointRepository.GetAllAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ApiEndpoint> Endpoints, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm = null, ApiEndpointType? type = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(searchTerm) && type.HasValue)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _apiEndpointRepository.GetPagedAsync(
                pageNumber, pageSize,
                e => e.Type == type.Value &&
                     (e.Name.ToLower().Contains(lowerSearchTerm) ||
                      e.Url.ToLower().Contains(lowerSearchTerm) ||
                      (e.Description != null && e.Description.ToLower().Contains(lowerSearchTerm))),
                cancellationToken: cancellationToken);
        }
        else if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _apiEndpointRepository.GetPagedAsync(
                pageNumber, pageSize,
                e => e.Name.ToLower().Contains(lowerSearchTerm) ||
                     e.Url.ToLower().Contains(lowerSearchTerm) ||
                     (e.Description != null && e.Description.ToLower().Contains(lowerSearchTerm)),
                cancellationToken: cancellationToken);
        }
        else if (type.HasValue)
        {
            return await _apiEndpointRepository.GetPagedAsync(
                pageNumber, pageSize,
                e => e.Type == type.Value,
                cancellationToken: cancellationToken);
        }

        return await _apiEndpointRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken: cancellationToken);
    }

    public async Task<ApiEndpoint> CreateAsync(ApiEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        // Check if URL and method combination already exists
        if (await _apiEndpointRepository.UrlExistsAsync(endpoint.Url, endpoint.Method, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("An endpoint with this URL and method already exists");
        }

        endpoint.CreatedAt = DateTime.UtcNow;
        endpoint.UpdatedAt = DateTime.UtcNow;

        return await _apiEndpointRepository.AddAsync(endpoint, cancellationToken);
    }

    public async Task<ApiEndpoint> UpdateAsync(ApiEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var existingEndpoint = await _apiEndpointRepository.GetByIdAsync(endpoint.Id, cancellationToken);
        if (existingEndpoint == null)
        {
            throw new InvalidOperationException("API endpoint not found");
        }

        // Check if URL and method combination already exists (excluding current endpoint)
        if (await _apiEndpointRepository.UrlExistsAsync(endpoint.Url, endpoint.Method, endpoint.Id, cancellationToken))
        {
            throw new InvalidOperationException("An endpoint with this URL and method already exists");
        }

        existingEndpoint.Name = endpoint.Name;
        existingEndpoint.Description = endpoint.Description;
        existingEndpoint.Url = endpoint.Url;
        existingEndpoint.Method = endpoint.Method;
        existingEndpoint.Type = endpoint.Type;
        existingEndpoint.Headers = endpoint.Headers;
        existingEndpoint.RequestBody = endpoint.RequestBody;
        existingEndpoint.RequestContentType = endpoint.RequestContentType;
        existingEndpoint.ResponseContentType = endpoint.ResponseContentType;
        existingEndpoint.Authentication = endpoint.Authentication;
        existingEndpoint.RequiresAuthentication = endpoint.RequiresAuthentication;
        existingEndpoint.TimeoutSeconds = endpoint.TimeoutSeconds;
        existingEndpoint.IsActive = endpoint.IsActive;
        existingEndpoint.UpdatedAt = DateTime.UtcNow;

        return await _apiEndpointRepository.UpdateAsync(existingEndpoint, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var endpoint = await _apiEndpointRepository.GetByIdAsync(id, cancellationToken);
        if (endpoint == null)
        {
            return false;
        }

        await _apiEndpointRepository.DeleteAsync(endpoint, cancellationToken);
        return true;
    }

    public async Task<bool> TestEndpointAsync(Guid id, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteEndpointAsync(id, parameters, cancellationToken);
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, object? Response)> ExecuteEndpointAsync(Guid id, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        var endpoint = await _apiEndpointRepository.GetWithParametersAsync(id, cancellationToken);
        if (endpoint == null || !endpoint.IsActive)
        {
            return (false, "Endpoint not found or inactive");
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(endpoint.TimeoutSeconds);

            // Set headers
            foreach (var header in endpoint.Headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            // Build request
            var request = new HttpRequestMessage();
            var url = endpoint.Url;

            // Replace path parameters and add query parameters
            if (parameters != null)
            {
                foreach (var param in endpoint.Parameters.Where(p => p.IsInput))
                {
                    if (parameters.TryGetValue(param.Name, out var value))
                    {
                        var stringValue = value?.ToString() ?? string.Empty;

                        switch (param.Location?.ToLower())
                        {
                            case "path":
                                url = url.Replace($"{{{param.Name}}}", stringValue);
                                break;
                            case "query":
                                var separator = url.Contains('?') ? "&" : "?";
                                url += $"{separator}{param.Name}={Uri.EscapeDataString(stringValue)}";
                                break;
                            case "header":
                                request.Headers.Add(param.Name, stringValue);
                                break;
                        }
                    }
                }
            }

            request.RequestUri = new Uri(url);
            request.Method = new System.Net.Http.HttpMethod(endpoint.Method.ToString());

            // Set request body for non-GET requests
            if (endpoint.Method != HttpMethod.GET && !string.IsNullOrEmpty(endpoint.RequestBody))
            {
                var content = endpoint.RequestBody;
                
                // Replace body parameters
                if (parameters != null)
                {
                    foreach (var param in endpoint.Parameters.Where(p => p.IsInput && p.Location?.ToLower() == "body"))
                    {
                        if (parameters.TryGetValue(param.Name, out var value))
                        {
                            content = content.Replace($"{{{param.Name}}}", value?.ToString() ?? string.Empty);
                        }
                    }
                }

                request.Content = new StringContent(content, System.Text.Encoding.UTF8, endpoint.RequestContentType ?? "application/json");
            }

            // Execute request
            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("API endpoint {EndpointId} executed. Status: {StatusCode}", 
                id, response.StatusCode);

            return (response.IsSuccessStatusCode, new
            {
                StatusCode = (int)response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                Content = responseContent
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing API endpoint {EndpointId}", id);
            return (false, ex.Message);
        }
    }
}
