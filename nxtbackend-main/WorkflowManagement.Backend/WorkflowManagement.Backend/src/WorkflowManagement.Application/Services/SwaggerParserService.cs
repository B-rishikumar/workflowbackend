// SwaggerParserService.cs
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Application.Services;

public class SwaggerParserService : ISwaggerParserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SwaggerParserService> _logger;

    public SwaggerParserService(IHttpClientFactory httpClientFactory, ILogger<SwaggerParserService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<ApiEndpoint>> ParseSwaggerUrlAsync(string swaggerUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync(swaggerUrl, cancellationToken);
            return await ParseSwaggerJsonAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Swagger from URL: {SwaggerUrl}", swaggerUrl);
            throw new InvalidOperationException($"Failed to parse Swagger from URL: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ApiEndpoint>> ParseSwaggerJsonAsync(string swaggerJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoints = new List<ApiEndpoint>();
            
            using var doc = JsonDocument.Parse(swaggerJson);
            var root = doc.RootElement;

            // Get base information
            var info = root.GetProperty("info");
            var title = info.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : "Imported API";
            var version = info.TryGetProperty("version", out var versionProp) ? versionProp.GetString() : "1.0";

            // Get server information
            var baseUrl = "";
            if (root.TryGetProperty("servers", out var servers) && servers.GetArrayLength() > 0)
            {
                baseUrl = servers[0].GetProperty("url").GetString() ?? "";
            }
            else if (root.TryGetProperty("host", out var host))
            {
                var scheme = root.TryGetProperty("schemes", out var schemes) && schemes.GetArrayLength() > 0
                    ? schemes[0].GetString()
                    : "https";
                baseUrl = $"{scheme}://{host.GetString()}";
                
                if (root.TryGetProperty("basePath", out var basePath))
                {
                    baseUrl += basePath.GetString();
                }
            }

            // Parse paths
            if (root.TryGetProperty("paths", out var paths))
            {
                foreach (var pathProperty in paths.EnumerateObject())
                {
                    var path = pathProperty.Name;
                    var pathItem = pathProperty.Value;

                    foreach (var methodProperty in pathItem.EnumerateObject())
                    {
                        var methodName = methodProperty.Name.ToUpper();
                        if (!Enum.TryParse<HttpMethod>(methodName, out var httpMethod))
                            continue;

                        var operation = methodProperty.Value;
                        
                        var endpoint = new ApiEndpoint
                        {
                            Name = GetOperationSummary(operation, path, methodName),
                            Description = GetOperationDescription(operation),
                            Url = baseUrl + path,
                            Method = httpMethod,
                            Type = ApiEndpointType.REST,
                            RequestContentType = "application/json",
                            ResponseContentType = "application/json",
                            TimeoutSeconds = 30,
                            IsActive = true,
                            SwaggerDefinition = swaggerJson,
                            Parameters = ParseParameters(operation).ToList()
                        };

                        endpoints.Add(endpoint);
                    }
                }
            }

            return endpoints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Swagger JSON");
            throw new InvalidOperationException($"Failed to parse Swagger JSON: {ex.Message}");
        }
    }

    public async Task<bool> ValidateSwaggerAsync(string swagger, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(swagger);
            var root = doc.RootElement;

            // Basic validation - check for required properties
            return root.TryGetProperty("openapi", out _) || root.TryGetProperty("swagger", out _);
        }
        catch
        {
            return false;
        }
    }

    private string GetOperationSummary(JsonElement operation, string path, string method)
    {
        if (operation.TryGetProperty("summary", out var summary))
        {
            return summary.GetString() ?? $"{method} {path}";
        }
        
        if (operation.TryGetProperty("operationId", out var operationId))
        {
            return operationId.GetString() ?? $"{method} {path}";
        }

        return $"{method} {path}";
    }

    private string? GetOperationDescription(JsonElement operation)
    {
        return operation.TryGetProperty("description", out var description) 
            ? description.GetString() 
            : null;
    }

    private IEnumerable<ApiParameter> ParseParameters(JsonElement operation)
    {
        var parameters = new List<ApiParameter>();

        if (operation.TryGetProperty("parameters", out var parametersArray))
        {
            foreach (var param in parametersArray.EnumerateArray())
            {
                var parameter = new ApiParameter
                {
                    Name = param.GetProperty("name").GetString() ?? "",
                    Description = param.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    IsRequired = param.TryGetProperty("required", out var req) && req.GetBoolean(),
                    IsInput = true,
                    IsOutput = false,
                    Location = param.TryGetProperty("in", out var location) ? location.GetString() : "query",
                    Type = ParseParameterType(param)
                };

                parameters.Add(parameter);
            }
        }

        return parameters;
    }

    private ParameterType ParseParameterType(JsonElement parameter)
    {
        if (parameter.TryGetProperty("type", out var type))
        {
            return type.GetString()?.ToLower() switch
            {
                "string" => ParameterType.String,
                "integer" => ParameterType.Integer,
                "number" => ParameterType.Decimal,
                "boolean" => ParameterType.Boolean,
                "array" => ParameterType.Array,
                "object" => ParameterType.Object,
                _ => ParameterType.String
            };
        }

        return ParameterType.String;
    }
}