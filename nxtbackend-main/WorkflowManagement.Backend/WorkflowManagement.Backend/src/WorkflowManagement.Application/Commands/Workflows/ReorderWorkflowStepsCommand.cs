// ReorderWorkflowStepsCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Workflows;

public record ReorderWorkflowStepsCommand : IRequest<ResponseDto>
{
    public Guid WorkflowId { get; init; }
    public List<StepOrderInfo> StepOrders { get; init; } = new();
    public string UpdatedBy { get; init; } = string.Empty;
}

public class StepOrderInfo
{
    public Guid StepId { get; set; }
    public int NewOrder { get; set; }
}
