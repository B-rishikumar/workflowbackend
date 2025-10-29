using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class ApiParameter : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public Guid ApiEndpointId { get; set; }
    
    public ParameterType Type { get; set; } = ParameterType.String;
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsInput { get; set; } = true;
    
    public bool IsOutput { get; set; } = false;
    
    public string? DefaultValue { get; set; }
    
    public string? ValidationRules { get; set; }
    
    public string? Location { get; set; } // query, header, body, path
    
    public Dictionary<string, object> Schema { get; set; } = new();
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    // Navigation Properties
    public ApiEndpoint ApiEndpoint { get; set; } = null!;
}