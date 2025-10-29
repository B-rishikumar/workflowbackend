// Environment/CreateEnvironmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.Environment;

public class CreateEnvironmentDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid ProjectId { get; set; }
    
    public string? Color { get; set; }
    
    public Dictionary<string, string> Variables { get; set; } = new();
}