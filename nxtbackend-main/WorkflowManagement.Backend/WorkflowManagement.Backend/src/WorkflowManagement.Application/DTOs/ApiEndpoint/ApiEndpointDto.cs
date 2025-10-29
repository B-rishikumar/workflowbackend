// ApiEndpoint/ApiEndpointDto.cs
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.ApiEndpoint;

public class ApiEndpointDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public WFMHttpMethod Method { get; set; }
    public ApiEndpointType Type { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? RequestBody { get; set; }
    public string? RequestContentType { get; set; }
    public string? ResponseContentType { get; set; }
    public bool RequiresAuthentication { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ApiParameterDto> Parameters { get; set; } = new List<ApiParameterDto>();
}