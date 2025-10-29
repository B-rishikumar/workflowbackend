using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Project;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ProjectDto>>>> GetProjects(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? workspaceId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var result = await _projectService.GetProjectsAsync(page, pageSize, searchTerm, workspaceId, isActive, sortBy, sortDescending);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects");
                return StatusCode(500, ResponseDto<PagedResultDto<ProjectDto>>.Failure("An error occurred while retrieving projects"));
            }
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<ProjectDto>>> GetProject(int id)
        {
            try
            {
                var result = await _projectService.GetProjectByIdAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project {ProjectId}", id);
                return StatusCode(500, ResponseDto<ProjectDto>.Failure("An error occurred while retrieving the project"));
            }
        }

        /// <summary>
        /// Get project with detailed information
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ResponseDto<ProjectDetailDto>>> GetProjectDetail(int id)
        {
            try
            {
                var result = await _projectService.GetProjectDetailAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project detail {ProjectId}", id);
                return StatusCode(500, ResponseDto<ProjectDetailDto>.Failure("An error occurred while retrieving project details"));
            }
        }

        /// <summary>
        /// Get projects by workspace
        /// </summary>
        [HttpGet("workspace/{workspaceId}")]
        public async Task<ActionResult<ResponseDto<List<ProjectDto>>>> GetProjectsByWorkspace(
            int workspaceId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _projectService.GetProjectsByWorkspaceAsync(workspaceId, includeInactive);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects by workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, ResponseDto<List<ProjectDto>>.Failure("An error occurred while retrieving workspace projects"));
            }
        }

        /// <summary>
        /// Get active projects for dropdown lists
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResponseDto<List<ProjectDto>>>> GetActiveProjects([FromQuery] int? workspaceId = null)
        {
            try
            {
                var result = await _projectService.GetActiveProjectsAsync(workspaceId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active projects");
                return StatusCode(500, ResponseDto<List<ProjectDto>>.Failure("An error occurred while retrieving active projects"));
            }
        }

        /// <summary>
        /// Get projects for current user
        /// </summary>
        [HttpGet("my-projects")]
        public async Task<ActionResult<ResponseDto<List<ProjectDto>>>> GetMyProjects()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _projectService.GetUserProjectsAsync(currentUserId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user projects");
                return StatusCode(500, ResponseDto<List<ProjectDto>>.Failure("An error occurred while retrieving user projects"));
            }
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        [HttpPost]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<ProjectDto>>> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _projectService.CreateProjectAsync(createProjectDto, currentUserId);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetProject), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, ResponseDto<ProjectDto>.Failure("An error occurred while creating the project"));
            }
        }

        /// <summary>
        /// Update project information
        /// </summary>
        [HttpPut("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<ProjectDto>>> UpdateProject(int id, [FromBody] UpdateProjectDto updateProjectDto)
        {
            try
            {
                var result = await _projectService.UpdateProjectAsync(id, updateProjectDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, ResponseDto<ProjectDto>.Failure("An error occurred while updating the project"));
            }
        }

        /// <summary>
        /// Delete project (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteProject(int id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the project"));
            }
        }

        /// <summary>
        /// Activate project
        /// </summary>
        [HttpPatch("{id}/activate")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> ActivateProject(int id)
        {
            try
            {
                var result = await _projectService.ActivateProjectAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating project {ProjectId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while activating the project"));
            }
        }

        /// <summary>
        /// Deactivate project
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeactivateProject(int id)
        {
            try
            {
                var result = await _projectService.DeactivateProjectAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating project {ProjectId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deactivating the project"));
            }
        }

        /// <summary>
        /// Get project environments
        /// </summary>
        [HttpGet("{projectId}/environments")]
        public async Task<ActionResult<ResponseDto<List<EnvironmentSummaryDto>>>> GetProjectEnvironments(int projectId)
        {
            try
            {
                var result = await _projectService.GetProjectEnvironmentsAsync(projectId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environments for project {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<List<EnvironmentSummaryDto>>.Failure("An error occurred while retrieving project environments"));
            }
        }

        /// <summary>
        /// Get project workflows
        /// </summary>
        [HttpGet("{projectId}/workflows")]
        public async Task<ActionResult<ResponseDto<List<WorkflowSummaryDto>>>> GetProjectWorkflows(
            int projectId,
            [FromQuery] WorkflowStatus? status = null)
        {
            try
            {
                var result = await _projectService.GetProjectWorkflowsAsync(projectId, status);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows for project {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<List<WorkflowSummaryDto>>.Failure("An error occurred while retrieving project workflows"));
            }
        }

        /// <summary>
        /// Get project statistics
        /// </summary>
        [HttpGet("{projectId}/statistics")]
        public async Task<ActionResult<ResponseDto<ProjectStatisticsDto>>> GetProjectStatistics(
            int projectId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _projectService.GetProjectStatisticsAsync(projectId, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for project {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<ProjectStatisticsDto>.Failure("An error occurred while retrieving project statistics"));
            }
        }

        /// <summary>
        /// Add user to project
        /// </summary>
        [HttpPost("{projectId}/users/{userId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> AddUserToProject(int projectId, int userId, [FromBody] ProjectUserRoleDto roleDto)
        {
            try
            {
                var result = await _projectService.AddUserToProjectAsync(projectId, userId, roleDto.Role);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to project {ProjectId}", userId, projectId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while adding user to project"));
            }
        }

        /// <summary>
        /// Remove user from project
        /// </summary>
        [HttpDelete("{projectId}/users/{userId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> RemoveUserFromProject(int projectId, int userId)
        {
            try
            {
                var result = await _projectService.RemoveUserFromProjectAsync(projectId, userId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from project {ProjectId}", userId, projectId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while removing user from project"));
            }
        }

        /// <summary>
        /// Get project users
        /// </summary>
        [HttpGet("{projectId}/users")]
        public async Task<ActionResult<ResponseDto<List<ProjectUserDto>>>> GetProjectUsers(int projectId)
        {
            try
            {
                var result = await _projectService.GetProjectUsersAsync(projectId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for project {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<List<ProjectUserDto>>.Failure("An error occurred while retrieving project users"));
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
    /// Supporting DTOs
    /// </summary>
    public class ProjectUserRoleDto
    {
        public UserRole Role { get; set; }
    }

    public class ProjectUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class EnvironmentSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int WorkflowCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WorkflowSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public int TotalExecutions { get; set; }
        public double SuccessRate { get; set; }
    }

    public class ProjectStatisticsDto
    {
        public int TotalEnvironments { get; set; }
        public int ActiveEnvironments { get; set; }
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int DraftWorkflows { get; set; }
        public int PublishedWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastExecutionAt { get; set; }
        public Dictionary<string, int> WorkflowStatusDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyExecutionTrend { get; set; } = new();
    }

    public class ProjectDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<EnvironmentSummaryDto> Environments { get; set; } = new();
        public List<WorkflowSummaryDto> Workflows { get; set; } = new();
        public List<ProjectUserDto> Users { get; set; } = new();
        public ProjectStatisticsDto Statistics { get; set; } = new();
    }
}