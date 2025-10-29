// ApprovalRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Repositories;

public class ApprovalRepository : GenericRepository<WorkflowApproval>, IApprovalRepository
{
    public ApprovalRepository(WorkflowDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkflowApproval>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wa => wa.WorkflowId == workflowId)
            .Include(wa => wa.RequestedBy)
            .Include(wa => wa.ApprovedBy)
            .OrderByDescending(wa => wa.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wa => wa.Status == ApprovalStatus.Pending)
            .Include(wa => wa.Workflow)
            .Include(wa => wa.RequestedBy)
            .OrderBy(wa => wa.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowApproval>> GetByApproverIdAsync(Guid approverId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wa => wa.ApprovedById == approverId)
            .Include(wa => wa.Workflow)
            .Include(wa => wa.RequestedBy)
            .OrderByDescending(wa => wa.ApprovedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowApproval>> GetByRequesterIdAsync(Guid requesterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wa => wa.RequestedById == requesterId)
            .Include(wa => wa.Workflow)
            .Include(wa => wa.ApprovedBy)
            .OrderByDescending(wa => wa.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowApproval>> GetByStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wa => wa.Status == status)
            .Include(wa => wa.Workflow)
            .Include(wa => wa.RequestedBy)
            .Include(wa => wa.ApprovedBy)
            .OrderByDescending(wa => wa.RequestedAt)
            .ToListAsync(cancellationToken);
    }
}