// AddWorkflowStepCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record AddWorkflowStepCommand : IRequest<ResponseDto<WorkflowStepDto>>
{
    public Guid WorkflowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid ApiEndpointId { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
    public bool ContinueOnError { get; init; } = false;
    public int TimeoutSeconds { get; init; } = 300;
    public int RetryCount { get; init; } = 0;
    public int RetryDelaySeconds { get; init; } = 5;
    public Dictionary<string, object> InputMapping { get; init; } = new();
    public Dictionary<string, object> OutputMapping { get; init; } = new();
    public Dictionary<string, object> Conditions { get; init; } = new();
    public Dictionary<string, object> Configuration { get; init; } = new();
    public string CreatedBy { get; init; } = string.Empty;
}

public class WorkflowStepDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid ApiEndpointId { get; set; }
    public string ApiEndpointName { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public bool ContinueOnError { get; set; }
    public int TimeoutSeconds { get; set; }
    public int RetryCount { get; set; }
    public int RetryDelaySeconds { get; set; }
    public Dictionary<string, object> InputMapping { get; set; } = new();
    public Dictionary<string, object> OutputMapping { get; set; } = new();
    public Dictionary<string, object> Conditions { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
