using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;
        private readonly ILogger<ApprovalController> _logger;

        public ApprovalController(IApprovalService approvalService, ILogger<ApprovalController> logger)
        {
            _approvalService = approvalService;
            _logger = logger;
        }

        /// <summary>
        /// Request approval for a workflow
        /// </summary>
        [HttpPost("request")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<WorkflowApprovalDto>>> RequestApproval([FromBody] RequestApprovalDto requestDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.RequestApprovalAsync(
                    requestDto.WorkflowId,
                    requestDto.ApprovalType,
                    currentUserId,
                    requestDto.RequestReason,
                    requestDto.Metadata);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting approval");
                return StatusCode(500, ResponseDto<WorkflowApprovalDto>.Failure("An error occurred while requesting approval"));
            }
        }

        /// <summary>
        /// Approve a workflow request
        /// </summary>
        [HttpPost("{approvalId}/approve")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<WorkflowApprovalDto>>> ApproveRequest(int approvalId, [FromBody] ApprovalActionDto actionDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.ApproveRequestAsync(approvalId, currentUserId, actionDto.Comments);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving request {ApprovalId}", approvalId);
                return StatusCode(500, ResponseDto<WorkflowApprovalDto>.Failure("An error occurred while approving the request"));
            }
        }

        /// <summary>
        /// Reject a workflow request
        /// </summary>
        [HttpPost("{approvalId}/reject")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<WorkflowApprovalDto>>> RejectRequest(int approvalId, [FromBody] ApprovalActionDto actionDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.RejectRequestAsync(approvalId, currentUserId, actionDto.Comments);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting request {ApprovalId}", approvalId);
                return StatusCode(500, ResponseDto<WorkflowApprovalDto>.Failure("An error occurred while rejecting the request"));
            }
        }

        /// <summary>
        /// Cancel an approval request
        /// </summary>
        [HttpPost("{approvalId}/cancel")]
        public async Task<ActionResult<ResponseDto<bool>>> CancelApprovalRequest(int approvalId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.CancelApprovalRequestAsync(approvalId, currentUserId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling approval request {ApprovalId}", approvalId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while cancelling the approval request"));
            }
        }

        /// <summary>
        /// Get pending approvals for current user
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<ResponseDto<List<WorkflowApprovalDto>>>> GetPendingApprovals()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.GetPendingApprovalsAsync(currentUserId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals");
                return StatusCode(500, ResponseDto<List<WorkflowApprovalDto>>.Failure("An error occurred while retrieving pending approvals"));
            }
        }

        /// <summary>
        /// Get all pending approvals (admin/manager only)
        /// </summary>
        [HttpGet("pending/all")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<List<WorkflowApprovalDto>>>> GetAllPendingApprovals()
        {
            try
            {
                var result = await _approvalService.GetPendingApprovalsAsync();
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all pending approvals");
                return StatusCode(500, ResponseDto<List<WorkflowApprovalDto>>.Failure("An error occurred while retrieving all pending approvals"));
            }
        }

        /// <summary>
        /// Get approval history for a workflow
        /// </summary>
        [HttpGet("workflow/{workflowId}/history")]
        public async Task<ActionResult<ResponseDto<List<WorkflowApprovalDto>>>> GetApprovalHistory(int workflowId)
        {
            try
            {
                var result = await _approvalService.GetApprovalHistoryAsync(workflowId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval history for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<List<WorkflowApprovalDto>>.Failure("An error occurred while retrieving approval history"));
            }
        }

        /// <summary>
        /// Check if user can approve a workflow
        /// </summary>
        [HttpGet("workflow/{workflowId}/can-approve")]
        public async Task<ActionResult<ResponseDto<bool>>> CanUserApproveWorkflow(
            int workflowId,
            [FromQuery] string approvalType = "execution")
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.CanUserApproveWorkflowAsync(workflowId, currentUserId, approvalType);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking approval permission for workflow {WorkflowId}", workflowId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while checking approval permissions"));
            }
        }

        /// <summary>
        /// Get approval statistics
        /// </summary>
        [HttpGet("statistics")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<ApprovalStatisticsDto>>> GetApprovalStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? userId = null)
        {
            try
            {
                var result = await _approvalService.GetApprovalStatisticsAsync(startDate, endDate, userId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval statistics");
                return StatusCode(500, ResponseDto<ApprovalStatisticsDto>.Failure("An error occurred while retrieving approval statistics"));
            }
        }

        /// <summary>
        /// Get approval requests by user
        /// </summary>
        [HttpGet("user/{userId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<List<WorkflowApprovalDto>>>> GetApprovalsByUser(
            int userId,
            [FromQuery] ApprovalStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _approvalService.GetApprovalsByUserAsync(userId, status, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approvals for user {UserId}", userId);
                return StatusCode(500, ResponseDto<List<WorkflowApprovalDto>>.Failure("An error occurred while retrieving user approvals"));
            }
        }

        /// <summary>
        /// Get my approval requests
        /// </summary>
        [HttpGet("my-requests")]
        public async Task<ActionResult<ResponseDto<List<WorkflowApprovalDto>>>> GetMyApprovalRequests(
            [FromQuery] ApprovalStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.GetApprovalsByUserAsync(currentUserId, status, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my approval requests");
                return StatusCode(500, ResponseDto<List<WorkflowApprovalDto>>.Failure("An error occurred while retrieving your approval requests"));
            }
        }

        /// <summary>
        /// Bulk approve multiple requests
        /// </summary>
        [HttpPost("bulk-approve")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<BulkApprovalResultDto>>> BulkApprove([FromBody] BulkApprovalDto bulkDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.BulkApproveAsync(bulkDto.ApprovalIds, currentUserId, bulkDto.Comments);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk approval");
                return StatusCode(500, ResponseDto<BulkApprovalResultDto>.Failure("An error occurred while performing bulk approval"));
            }
        }

        /// <summary>
        /// Bulk reject multiple requests
        /// </summary>
        [HttpPost("bulk-reject")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<BulkApprovalResultDto>>> BulkReject([FromBody] BulkApprovalDto bulkDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _approvalService.BulkRejectAsync(bulkDto.ApprovalIds, currentUserId, bulkDto.Comments);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk rejection");
                return StatusCode(500, ResponseDto<BulkApprovalResultDto>.Failure("An error occurred while performing bulk rejection"));
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
        }

        #endregion
    }

    /// <summary>
    /// Request DTOs
    /// </summary>
    public class RequestApprovalDto
    {
        public int WorkflowId { get; set; }
        public string ApprovalType { get; set; } = "execution";
        public string? RequestReason { get; set; }
        public string? Metadata { get; set; }
    }

    public class ApprovalActionDto
    {
        public string? Comments { get; set; }
    }

    public class BulkApprovalDto
    {
        public List<int> ApprovalIds { get; set; } = new();
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Response DTOs
    /// </summary>
    public class ApprovalStatisticsDto
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int CancelledRequests { get; set; }
        public double ApprovalRate { get; set; }
        public TimeSpan? AverageApprovalTime { get; set; }
        public Dictionary<string, int> ApprovalTypeDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyApprovalTrend { get; set; } = new();
        public List<TopApproverDto> TopApprovers { get; set; } = new();
    }

    public class TopApproverDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ApprovalsCount { get; set; }
        public double ApprovalRate { get; set; }
        public TimeSpan? AverageResponseTime { get; set; }
    }

    public class BulkApprovalResultDto
    {
        public int TotalRequests { get; set; }
        public int SuccessfulActions { get; set; }
        public int FailedActions { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<int> ProcessedApprovalIds { get; set; } = new();
        public List<int> FailedApprovalIds { get; set; } = new();
    }
}