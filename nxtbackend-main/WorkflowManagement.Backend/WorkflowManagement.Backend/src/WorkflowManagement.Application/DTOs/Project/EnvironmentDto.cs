// Environment/EnvironmentDto.cs
namespace WorkflowManagement.Application.DTOs.Environment;

public class EnvironmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int WorkflowsCount { get; set; }
}