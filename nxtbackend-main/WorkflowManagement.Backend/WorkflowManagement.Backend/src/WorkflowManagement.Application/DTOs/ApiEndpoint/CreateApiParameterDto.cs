// ApiEndpoint/CreateApiParameterDto.cs
using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.ApiEndpoint;

public class CreateApiParameterDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public ParameterType Type { get; set; } = ParameterType.String;
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsInput { get; set; } = true;
    
    public bool IsOutput { get; set; } = false;
    
    public string? DefaultValue { get; set; }
    
    public string? Location { get; set; } = "query";
    
    public Dictionary<string, object> Schema { get; set; } = new();
}
