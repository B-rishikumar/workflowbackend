// Workflow/WorkflowVersionDto.cs
namespace WorkflowManagement.Application.DTOs.Workflow;

public class WorkflowVersionDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public string? ChangeDescription { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}