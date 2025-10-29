// ApiEndpoint/CreateApiEndpointDto.cs
using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.ApiEndpoint;

public class CreateApiEndpointDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
    
    public WFMHttpMethod Method { get; set; } = WFMHttpMethod.GET;
    
    public ApiEndpointType Type { get; set; } = ApiEndpointType.REST;
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public string? RequestBody { get; set; }
    
    public string? RequestContentType { get; set; } = "application/json";
    
    public bool RequiresAuthentication { get; set; } = false;
    
    public int TimeoutSeconds { get; set; } = 30;
    
    public Dictionary<string, object> Authentication { get; set; } = new();
    
    public ICollection<CreateApiParameterDto> Parameters { get; set; } = new List<CreateApiParameterDto>();
}
