// ApiEndpoint/ApiParameterDto.cs
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.ApiEndpoint;

public class ApiParameterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ParameterType Type { get; set; }
    public bool IsRequired { get; set; }
    public bool IsInput { get; set; }
    public bool IsOutput { get; set; }
    public string? DefaultValue { get; set; }
    public string? Location { get; set; }
    public Dictionary<string, object> Schema { get; set; } = new();
}