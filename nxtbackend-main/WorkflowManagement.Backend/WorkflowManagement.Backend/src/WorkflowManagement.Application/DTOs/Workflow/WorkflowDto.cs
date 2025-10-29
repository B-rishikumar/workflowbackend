using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.Workflow;

public class WorkflowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowStatus Status { get; set; }
    public Guid EnvironmentId { get; set; }
    public string EnvironmentName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public int TimeoutMinutes { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int StepsCount { get; set; }
    public int ExecutionsCount { get; set; }
    public string? LatestVersion { get; set; }
}
