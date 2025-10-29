// IApprovalRepository.cs
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Entities;
namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IApprovalRepository : IGenericRepository<WorkflowApproval>
{
    Task<IEnumerable<WorkflowApproval>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowApproval>> GetByApproverIdAsync(Guid approverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowApproval>> GetByRequesterIdAsync(Guid requesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowApproval>> GetByStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default);
}