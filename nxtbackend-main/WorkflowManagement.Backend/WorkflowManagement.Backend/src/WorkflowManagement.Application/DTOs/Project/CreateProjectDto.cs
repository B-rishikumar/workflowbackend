// Project/CreateProjectDto.cs
using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.Project;

public class CreateProjectDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid WorkspaceId { get; set; }
    
    public string? Color { get; set; }
}