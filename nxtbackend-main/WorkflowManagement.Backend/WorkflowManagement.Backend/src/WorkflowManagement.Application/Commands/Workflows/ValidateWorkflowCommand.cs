// ValidateWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record ValidateWorkflowCommand : IRequest<ResponseDto<WorkflowValidationResult>>
{
    public Guid WorkflowId { get; init; }
    public string ValidatedBy { get; init; } = string.Empty;
    public bool DeepValidation { get; init; } = false;
}

public class WorkflowValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ValidationDetails { get; set; } = new();
}