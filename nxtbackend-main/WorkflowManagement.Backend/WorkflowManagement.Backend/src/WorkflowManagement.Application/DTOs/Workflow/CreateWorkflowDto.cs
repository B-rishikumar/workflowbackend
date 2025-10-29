// Workflow/CreateWorkflowDto.cs
using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.Workflow;

public class CreateWorkflowDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid EnvironmentId { get; set; }
    
    public string? Tags { get; set; }
    
    public int TimeoutMinutes { get; set; } = 30;
    
    public int RetryCount { get; set; } = 0;
    
    public Dictionary<string, object> GlobalVariables { get; set; } = new();
}