using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;
using System.Net.Http;
namespace WorkflowManagement.Core.Entities;

public class ApiEndpoint : SoftDeleteEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public string Url { get; set; } = string.Empty;
    
    public WFMHttpMethod Method { get; set; } = WFMHttpMethod.GET;
    
    public ApiEndpointType Type { get; set; } = ApiEndpointType.REST;
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public string? RequestBody { get; set; }
    
    public string? RequestContentType { get; set; } = "application/json";
    
    public string? ResponseContentType { get; set; } = "application/json";
    
    public Dictionary<string, object> Authentication { get; set; } = new();
    
    public bool RequiresAuthentication { get; set; } = false;
    
    public int TimeoutSeconds { get; set; } = 30;
    
    public bool IsActive { get; set; } = true;
    
    public string? SwaggerDefinition { get; set; }
    
    public string? SoapWsdl { get; set; }
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    // Navigation Properties
    public ICollection<ApiParameter> Parameters { get; set; } = new List<ApiParameter>();
    public ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
}