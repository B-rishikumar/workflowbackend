// IApprovalService.cs
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface IApprovalService
{
    Task<WorkflowApproval?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowApproval>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WorkflowApproval> Approvals, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, ApprovalStatus? status = null, CancellationToken cancellationToken = default);
    Task<WorkflowApproval> RequestApprovalAsync(Guid workflowId, Guid requesterId, string approvalType, string? reason = null, CancellationToken cancellationToken = default);
    Task<WorkflowApproval> ApproveAsync(Guid approvalId, Guid approverId, string? comment = null, CancellationToken cancellationToken = default);
    Task<WorkflowApproval> RejectAsync(Guid approvalId, Guid approverId, string? comment = null, CancellationToken cancellationToken = default);
    Task<bool> CancelApprovalAsync(Guid approvalId, CancellationToken cancellationToken = default);
}