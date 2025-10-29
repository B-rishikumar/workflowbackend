using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.Workflow;

public class UpdateWorkflowDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public string? Tags { get; set; }
    
    public int TimeoutMinutes { get; set; }
    
    public int RetryCount { get; set; }
    
    public Dictionary<string, object> GlobalVariables { get; set; } = new();
}