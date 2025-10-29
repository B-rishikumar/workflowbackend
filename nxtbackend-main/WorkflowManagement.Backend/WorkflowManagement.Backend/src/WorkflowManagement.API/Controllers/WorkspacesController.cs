using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Workspace;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly ILogger<WorkspacesController> _logger;

        public WorkspacesController(IWorkspaceService workspaceService, ILogger<WorkspacesController> logger)
        {
            _workspaceService = workspaceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all workspaces with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<PagedResultDto<WorkspaceDto>>>> GetWorkspaces(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var result = await _workspaceService.GetWorkspacesAsync(page, pageSize, searchTerm, isActive, sortBy, sortDescending);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspaces");
                return StatusCode(500, ResponseDto<PagedResultDto<WorkspaceDto>>.Failure("An error occurred while retrieving workspaces"));
            }
        }

        /// <summary>
        /// Get workspace by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<WorkspaceDto>>> GetWorkspace(int id)
        {
            try
            {
                var result = await _workspaceService.GetWorkspaceByIdAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace {WorkspaceId}", id);
                return StatusCode(500, ResponseDto<WorkspaceDto>.Failure("An error occurred while retrieving the workspace"));
            }
        }

        /// <summary>
        /// Get workspace with detailed information including projects and users
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ResponseDto<WorkspaceDetailDto>>> GetWorkspaceDetail(int id)
        {
            try
            {
                var result = await _workspaceService.GetWorkspaceDetailAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace detail {WorkspaceId}", id);
                return StatusCode(500, ResponseDto<WorkspaceDetailDto>.Failure("An error occurred while retrieving workspace details"));
            }
        }

        /// <summary>
        /// Get workspaces for current user
        /// </summary>
        [HttpGet("my-workspaces")]
        public async Task<ActionResult<ResponseDto<List<WorkspaceDto>>>> GetMyWorkspaces()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _workspaceService.GetUserWorkspacesAsync(currentUserId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user workspaces");
                return StatusCode(500, ResponseDto<List<WorkspaceDto>>.Failure("An error occurred while retrieving user workspaces"));
            }
        }

        /// <summary>
        /// Get active workspaces for dropdown lists
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResponseDto<List<WorkspaceDto>>>> GetActiveWorkspaces()
        {
            try
            {
                var result = await _workspaceService.GetActiveWorkspacesAsync();
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active workspaces");
                return StatusCode(500, ResponseDto<List<WorkspaceDto>>.Failure("An error occurred while retrieving active workspaces"));
            }
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        [HttpPost]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<WorkspaceDto>>> CreateWorkspace([FromBody] CreateWorkspaceDto createWorkspaceDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _workspaceService.CreateWorkspaceAsync(createWorkspaceDto, currentUserId);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetWorkspace), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workspace");
                return StatusCode(500, ResponseDto<WorkspaceDto>.Failure("An error occurred while creating the workspace"));
            }
        }

        /// <summary>
        /// Update workspace information
        /// </summary>
        [HttpPut("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<WorkspaceDto>>> UpdateWorkspace(int id, [FromBody] UpdateWorkspaceDto updateWorkspaceDto)
        {
            try
            {
                var result = await _workspaceService.UpdateWorkspaceAsync(id, updateWorkspaceDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workspace {WorkspaceId}", id);
                return StatusCode(500, ResponseDto<WorkspaceDto>.Failure("An error occurred while updating the workspace"));
            }
        }

        /// <summary>
        /// Delete workspace (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteWorkspace(int id)
        {
            try
            {
                var result = await _workspaceService.DeleteWorkspaceAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workspace {WorkspaceId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the workspace"));
            }
        }

        /// <summary>
        /// Activate workspace
        /// </summary>
        [HttpPatch("{id}/activate")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> ActivateWorkspace(int id)
        {
            try
            {
                var result = await _workspaceService.ActivateWorkspaceAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating workspace {WorkspaceId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while activating the workspace"));
            }
        }

        /// <summary>
        /// Deactivate workspace
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeactivateWorkspace(int id)
        {
            try
            {
                var result = await _workspaceService.DeactivateWorkspaceAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating workspace {WorkspaceId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deactivating the workspace"));
            }
        }

        /// <summary>
        /// Add user to workspace
        /// </summary>
        [HttpPost("{workspaceId}/users/{userId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> AddUserToWorkspace(int workspaceId, int userId, [FromBody] WorkspaceUserRoleDto roleDto)
        {
            try
            {
                var result = await _workspaceService.AddUserToWorkspaceAsync(workspaceId, userId, roleDto.Role);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to workspace {WorkspaceId}", userId, workspaceId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while adding user to workspace"));
            }
        }

        /// <summary>
        /// Remove user from workspace
        /// </summary>
        [HttpDelete("{workspaceId}/users/{userId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> RemoveUserFromWorkspace(int workspaceId, int userId)
        {
            try
            {
                var result = await _workspaceService.RemoveUserFromWorkspaceAsync(workspaceId, userId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from workspace {WorkspaceId}", userId, workspaceId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while removing user from workspace"));
            }
        }

        /// <summary>
        /// Get workspace users
        /// </summary>
        [HttpGet("{workspaceId}/users")]
        public async Task<ActionResult<ResponseDto<List<WorkspaceUserDto>>>> GetWorkspaceUsers(int workspaceId)
        {
            try
            {
                var result = await _workspaceService.GetWorkspaceUsersAsync(workspaceId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, ResponseDto<List<WorkspaceUserDto>>.Failure("An error occurred while retrieving workspace users"));
            }
        }

        /// <summary>
        /// Get workspace projects
        /// </summary>
        [HttpGet("{workspaceId}/projects")]
        public async Task<ActionResult<ResponseDto<List<ProjectSummaryDto>>>> GetWorkspaceProjects(int workspaceId)
        {
            try
            {
                var result = await _workspaceService.GetWorkspaceProjectsAsync(workspaceId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects for workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, ResponseDto<List<ProjectSummaryDto>>.Failure("An error occurred while retrieving workspace projects"));
            }
        }

        /// <summary>
        /// Get workspace statistics
        /// </summary>
        [HttpGet("{workspaceId}/statistics")]
        public async Task<ActionResult<ResponseDto<WorkspaceStatisticsDto>>> GetWorkspaceStatistics(int workspaceId)
        {
            try
            {
                var result = await _workspaceService.GetWorkspaceStatisticsAsync(workspaceId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, ResponseDto<WorkspaceStatisticsDto>.Failure("An error occurred while retrieving workspace statistics"));
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
    /// DTO for workspace user role assignment
    /// </summary>
    public class WorkspaceUserRoleDto
    {
        public UserRole Role { get; set; }
    }

    /// <summary>
    /// Summary DTOs for related entities
    /// </summary>
    public class WorkspaceUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProjectSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int WorkflowCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WorkspaceStatisticsDto
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalExecutions { get; set; }
        public double SuccessRate { get; set; }
    }

    public class WorkspaceDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProjectSummaryDto> Projects { get; set; } = new();
        public List<WorkspaceUserDto> Users { get; set; } = new();
        public WorkspaceStatisticsDto Statistics { get; set; } = new();
    }
}