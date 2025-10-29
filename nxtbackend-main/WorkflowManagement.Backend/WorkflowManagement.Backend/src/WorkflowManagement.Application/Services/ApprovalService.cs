using AutoMapper;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using Microsoft.Extensions.Logging;

namespace WorkflowManagement.Application.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly IApprovalRepository _approvalRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ApprovalService> _logger;

        public ApprovalService(
            IApprovalRepository approvalRepository,
            IWorkflowRepository workflowRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<ApprovalService> logger)
        {
            _approvalRepository = approvalRepository;
            _workflowRepository = workflowRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Interface Implementation

        public async Task<WorkflowApproval?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _approvalRepository.GetByIdAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval {ApprovalId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowApproval>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _approvalRepository.GetByWorkflowIdAsync(workflowId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approvals for workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _approvalRepository.GetPendingApprovalsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals");
                throw;
            }
        }

        public async Task<(IEnumerable<WorkflowApproval> Approvals, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, ApprovalStatus? status = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var (approvals, totalCount) = await _approvalRepository.GetPagedAsync(pageNumber, pageSize, status, cancellationToken);
                return (approvals, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged approvals");
                throw;
            }
        }

        public async Task<WorkflowApproval> RequestApprovalAsync(Guid workflowId, Guid requesterId,
            string approvalType, string? reason = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Requesting approval for workflow {WorkflowId}, type: {ApprovalType}", workflowId, approvalType);

                var workflow = await _workflowRepository.GetByIdAsync(workflowId, cancellationToken);
                if (workflow == null)
                {
                    throw new WorkflowNotFoundException($"Workflow with ID {workflowId} not found");
                }

                var requestingUser = await _userRepository.GetByIdAsync(requesterId, cancellationToken);
                if (requestingUser == null)
                {
                    throw new UserNotFoundException($"User with ID {requesterId} not found");
                }

                var existingPendingApproval = await _approvalRepository.GetPendingApprovalAsync(workflowId, approvalType, cancellationToken);
                if (existingPendingApproval != null)
                {
                    throw new InvalidOperationException("There is already a pending approval request for this workflow");
                }

                var approver = await GetApproverForWorkflowAsync(workflow, approvalType);
                if (approver == null)
                {
                    throw new InvalidOperationException("No appropriate approver found for this workflow");
                }

                var approval = new WorkflowApproval
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    RequestedByUserId = requesterId,
                    ApproverUserId = approver.Id,
                    Status = ApprovalStatus.Pending,
                    ApprovalType = approvalType,
                    RequestReason = reason,
                    RequestedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                var createdApproval = await _approvalRepository.AddAsync(approval, cancellationToken);

                try
                {
                    await _notificationService.SendApprovalRequestNotificationAsync(createdApproval.Id, approver.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send approval notification for approval {ApprovalId}", createdApproval.Id);
                }

                _logger.LogInformation("Approval request created successfully with ID {ApprovalId}", createdApproval.Id);
                return createdApproval;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting approval for workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        public async Task<WorkflowApproval> ApproveAsync(Guid approvalId, Guid approverId,
            string? comment = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing approval for request {ApprovalId} by user {ApproverId}", approvalId, approverId);

                var approval = await _approvalRepository.GetByIdAsync(approvalId, cancellationToken);
                if (approval == null)
                {
                    throw new WorkflowApprovalNotFoundException($"Approval with ID {approvalId} not found");
                }

                if (approval.ApproverUserId != approverId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to approve this request");
                }

                if (approval.Status != ApprovalStatus.Pending)
                {
                    throw new InvalidOperationException("This approval request has already been processed");
                }

                approval.Status = ApprovalStatus.Approved;
                approval.ApprovalComments = comment;
                approval.ApprovedAt = DateTime.UtcNow;
                approval.UpdatedAt = DateTime.UtcNow;

                var updatedApproval = await _approvalRepository.UpdateAsync(approval, cancellationToken);

                try
                {
                    await ProcessPostApprovalActionsAsync(approval);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing post-approval actions for approval {ApprovalId}", approvalId);
                }

                try
                {
                    var requester = await _userRepository.GetByIdAsync(approval.RequestedByUserId, cancellationToken);
                    if (requester != null)
                    {
                        await _notificationService.SendApprovalStatusNotificationAsync(approval.Id, requester.Email, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send approval notification for approval {ApprovalId}", approvalId);
                }

                _logger.LogInformation("Approval request {ApprovalId} approved successfully", approvalId);
                return updatedApproval;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving request {ApprovalId}", approvalId);
                throw;
            }
        }

        public async Task<WorkflowApproval> RejectAsync(Guid approvalId, Guid approverId,
            string? comment = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing rejection for request {ApprovalId} by user {ApproverId}", approvalId, approverId);

                var approval = await _approvalRepository.GetByIdAsync(approvalId, cancellationToken);
                if (approval == null)
                {
                    throw new WorkflowApprovalNotFoundException($"Approval with ID {approvalId} not found");
                }

                if (approval.ApproverUserId != approverId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to reject this request");
                }

                if (approval.Status != ApprovalStatus.Pending)
                {
                    throw new InvalidOperationException("This approval request has already been processed");
                }

                approval.Status = ApprovalStatus.Rejected;
                approval.ApprovalComments = comment;
                approval.ApprovedAt = DateTime.UtcNow;
                approval.UpdatedAt = DateTime.UtcNow;

                var updatedApproval = await _approvalRepository.UpdateAsync(approval, cancellationToken);

                try
                {
                    var requester = await _userRepository.GetByIdAsync(approval.RequestedByUserId, cancellationToken);
                    if (requester != null)
                    {
                        await _notificationService.SendApprovalStatusNotificationAsync(approval.Id, requester.Email, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send rejection notification for approval {ApprovalId}", approvalId);
                }

                _logger.LogInformation("Approval request {ApprovalId} rejected successfully", approvalId);
                return updatedApproval;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting request {ApprovalId}", approvalId);
                throw;
            }
        }

        public async Task<bool> CancelApprovalAsync(Guid approvalId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling approval request {ApprovalId}", approvalId);

                var approval = await _approvalRepository.GetByIdAsync(approvalId, cancellationToken);
                if (approval == null)
                {
                    return false;
                }

                if (approval.Status != ApprovalStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending approval requests can be cancelled");
                }

                approval.Status = ApprovalStatus.Cancelled;
                approval.ApprovedAt = DateTime.UtcNow;
                approval.UpdatedAt = DateTime.UtcNow;
                approval.ApprovalComments = "Cancelled";

                await _approvalRepository.UpdateAsync(approval, cancellationToken);

                try
                {
                    if (approval.ApproverUserId.HasValue)
                    {
                        var approver = await _userRepository.GetByIdAsync(approval.ApproverUserId.Value, cancellationToken);
                        if (approver != null)
                        {
                            await _notificationService.SendApprovalCancellationNotificationAsync(approval.Id, approver.Email);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send cancellation notification for approval {ApprovalId}", approvalId);
                }

                _logger.LogInformation("Approval request {ApprovalId} cancelled successfully", approvalId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling approval request {ApprovalId}", approvalId);
                throw;
            }
        }

        #endregion

        #region Legacy Methods (backward compatibility)

        public async Task<ResponseDto<WorkflowApprovalDto>> RequestApprovalAsync(int workflowId, string approvalType,
            int requestedByUserId, string? requestReason = null, string? metadata = null)
        {
            try
            {
                var approval = await RequestApprovalAsync(
                    ConvertToGuid(workflowId),
                    ConvertToGuid(requestedByUserId),
                    approvalType,
                    requestReason);

                var approvalDto = await MapToDto(approval);
                return ResponseDto<WorkflowApprovalDto>.CreateSuccess(approvalDto, "Approval request created successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<WorkflowApprovalDto>.Failure("Failed to request approval", ex.Message);
            }
        }

        public async Task<ResponseDto<WorkflowApprovalDto>> ApproveRequestAsync(int approvalId, int approverUserId,
            string? approvalComments = null)
        {
            try
            {
                var approval = await ApproveAsync(
                    ConvertToGuid(approvalId),
                    ConvertToGuid(approverUserId),
                    approvalComments);

                var approvalDto = await MapToDto(approval);
                return ResponseDto<WorkflowApprovalDto>.CreateSuccess(approvalDto, "Approval request approved successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<WorkflowApprovalDto>.Failure("Failed to approve request", ex.Message);
            }
        }

        public async Task<ResponseDto<WorkflowApprovalDto>> RejectRequestAsync(int approvalId, int approverUserId,
            string? rejectionReason = null)
        {
            try
            {
                var approval = await RejectAsync(
                    ConvertToGuid(approvalId),
                    ConvertToGuid(approverUserId),
                    rejectionReason);

                var approvalDto = await MapToDto(approval);
                return ResponseDto<WorkflowApprovalDto>.CreateSuccess(approvalDto, "Approval request rejected successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<WorkflowApprovalDto>.Failure("Failed to reject request", ex.Message);
            }
        }

        public async Task<ResponseDto<List<WorkflowApprovalDto>>> GetPendingApprovalsAsync(int? approverUserId = null)
        {
            try
            {
                var approvals = await GetPendingApprovalsAsync();
                var filteredApprovals = approverUserId.HasValue
                    ? approvals.Where(a => a.ApproverUserId == ConvertToGuid(approverUserId.Value))
                    : approvals;

                var approvalDtos = new List<WorkflowApprovalDto>();
                foreach (var approval in filteredApprovals)
                {
                    var dto = await MapToDto(approval);
                    approvalDtos.Add(dto);
                }

                return ResponseDto<List<WorkflowApprovalDto>>.CreateSuccess(approvalDtos, "Pending approvals retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<List<WorkflowApprovalDto>>.Failure("Failed to get pending approvals", ex.Message);
            }
        }

        public async Task<ResponseDto<List<WorkflowApprovalDto>>> GetApprovalHistoryAsync(int workflowId)
        {
            try
            {
                var approvals = await GetByWorkflowIdAsync(ConvertToGuid(workflowId));
                var approvalDtos = new List<WorkflowApprovalDto>();

                foreach (var approval in approvals)
                {
                    var dto = await MapToDto(approval);
                    approvalDtos.Add(dto);
                }

                return ResponseDto<List<WorkflowApprovalDto>>.CreateSuccess(approvalDtos, "Approval history retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<List<WorkflowApprovalDto>>.Failure("Failed to get approval history", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> CanUserApproveWorkflowAsync(int workflowId, int userId, string approvalType)
        {
            try
            {
                var workflow = await _workflowRepository.GetByIdAsync(ConvertToGuid(workflowId));
                if (workflow == null)
                {
                    return ResponseDto<bool>.Failure("Workflow not found");
                }

                var user = await _userRepository.GetByIdAsync(ConvertToGuid(userId));
                if (user == null)
                {
                    return ResponseDto<bool>.Failure("User not found");
                }

                var canApprove = await CanUserApproveAsync(workflow, user, approvalType);
                return ResponseDto<bool>.CreateSuccess(canApprove, $"User {(canApprove ? "can" : "cannot")} approve this workflow");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking approval permission for workflow {WorkflowId} and user {UserId}", workflowId, userId);
                return ResponseDto<bool>.Failure("Failed to check approval permissions", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> CancelApprovalRequestAsync(int approvalId, int requestedByUserId)
        {
            try
            {
                var approval = await GetByIdAsync(ConvertToGuid(approvalId));
                if (approval == null)
                {
                    return ResponseDto<bool>.Failure("Approval request not found");
                }

                if (approval.RequestedByUserId != ConvertToGuid(requestedByUserId))
                {
                    return ResponseDto<bool>.Failure("You can only cancel your own approval requests");
                }

                var result = await CancelApprovalAsync(ConvertToGuid(approvalId));
                return ResponseDto<bool>.CreateSuccess(result, result ? "Approval request cancelled successfully" : "Failed to cancel approval request");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling approval request {ApprovalId}", approvalId);
                return ResponseDto<bool>.Failure("Failed to cancel approval request", ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private async Task<WorkflowApprovalDto> MapToDto(WorkflowApproval approval)
        {
            var approvalDto = _mapper.Map<WorkflowApprovalDto>(approval);

            var requester = await _userRepository.GetByIdAsync(approval.RequestedByUserId);
            var approver = approval.ApproverUserId.HasValue
                ? await _userRepository.GetByIdAsync(approval.ApproverUserId.Value)
                : null;

            approvalDto.RequestedByUserName = requester != null ? $"{requester.FirstName} {requester.LastName}" : "Unknown";
            approvalDto.ApproverUserName = approver != null ? $"{approver.FirstName} {approver.LastName}" : "Unknown";

            return approvalDto;
        }

        private async Task<User?> GetApproverForWorkflowAsync(Workflow workflow, string approvalType)
        {
            var projectManagers = await _userRepository.GetUsersByProjectAndRoleAsync(workflow.EnvironmentId, UserRole.ProjectManager);
            if (projectManagers.Any())
            {
                return projectManagers.First();
            }

            var admins = await _userRepository.GetUsersByRoleAsync(UserRole.Admin);
            return admins.FirstOrDefault();
        }

        private async Task ProcessPostApprovalActionsAsync(WorkflowApproval approval)
        {
            var workflow = await _workflowRepository.GetByIdAsync(approval.WorkflowId);
            if (workflow == null) return;

            switch (approval.ApprovalType?.ToLower())
            {
                case "publish":
                    workflow.Status = WorkflowStatus.Published;
                    await _workflowRepository.UpdateAsync(workflow);
                    break;

                case "execution":
                    break;

                case "update":
                    break;

                default:
                    _logger.LogWarning("Unknown approval type: {ApprovalType}", approval.ApprovalType);
                    break;
            }
        }

        private async Task<bool> CanUserApproveAsync(Workflow workflow, User user, string approvalType)
        {
            if (user.Role == UserRole.Admin)
            {
                return true;
            }

            if (user.Role == UserRole.ProjectManager)
            {
                return true;
            }

            return false;
        }

        private Guid ConvertToGuid(int id)
        {
            return new Guid($"{id:D8}-0000-0000-0000-000000000000");
        }

        #endregion
    }
}