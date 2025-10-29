// ExportWorkflowCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record ExportWorkflowCommand : IRequest<ResponseDto<WorkflowExportResult>>
{
    public Guid WorkflowId { get; init; }
    public string Format { get; init; } = "json"; // json, xml, yaml
    public bool IncludeSteps { get; init; } = true;
    public bool IncludeVersions { get; init; } = false;
    public bool IncludeExecutionHistory { get; init; } = false;
    public string ExportedBy { get; init; } = string.Empty;
}

public class WorkflowExportResult
{
    public string Definition { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
}