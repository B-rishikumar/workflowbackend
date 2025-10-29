using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.Workspace;

public class CreateWorkspaceDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public string? LogoUrl { get; set; }
}