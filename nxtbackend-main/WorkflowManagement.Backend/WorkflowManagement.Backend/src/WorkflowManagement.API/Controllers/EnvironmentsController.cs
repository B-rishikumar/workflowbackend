using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.Environment;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnvironmentsController : ControllerBase
    {
        private readonly IEnvironmentService _environmentService;
        private readonly ILogger<EnvironmentsController> _logger;

        public EnvironmentsController(IEnvironmentService environmentService, ILogger<EnvironmentsController> logger)
        {
            _environmentService = environmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all environments with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<PagedResultDto<EnvironmentDto>>>> GetEnvironments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? projectId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentsAsync(page, pageSize, searchTerm, projectId, isActive, sortBy, sortDescending);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environments");
                return StatusCode(500, ResponseDto<PagedResultDto<EnvironmentDto>>.Failure("An error occurred while retrieving environments"));
            }
        }

        /// <summary>
        /// Get environment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<EnvironmentDto>>> GetEnvironment(int id)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentByIdAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<EnvironmentDto>.Failure("An error occurred while retrieving the environment"));
            }
        }

        /// <summary>
        /// Get environment with detailed information
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ResponseDto<EnvironmentDetailDto>>> GetEnvironmentDetail(int id)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentDetailAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environment detail {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<EnvironmentDetailDto>.Failure("An error occurred while retrieving environment details"));
            }
        }

        /// <summary>
        /// Get environments by project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ResponseDto<List<EnvironmentDto>>>> GetEnvironmentsByProject(
            int projectId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentsByProjectAsync(projectId, includeInactive);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environments by project {ProjectId}", projectId);
                return StatusCode(500, ResponseDto<List<EnvironmentDto>>.Failure("An error occurred while retrieving project environments"));
            }
        }

        /// <summary>
        /// Get active environments for dropdown lists
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResponseDto<List<EnvironmentDto>>>> GetActiveEnvironments([FromQuery] int? projectId = null)
        {
            try
            {
                var result = await _environmentService.GetActiveEnvironmentsAsync(projectId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active environments");
                return StatusCode(500, ResponseDto<List<EnvironmentDto>>.Failure("An error occurred while retrieving active environments"));
            }
        }

        /// <summary>
        /// Create a new environment
        /// </summary>
        [HttpPost]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<EnvironmentDto>>> CreateEnvironment([FromBody] CreateEnvironmentDto createEnvironmentDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _environmentService.CreateEnvironmentAsync(createEnvironmentDto, currentUserId);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetEnvironment), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating environment");
                return StatusCode(500, ResponseDto<EnvironmentDto>.Failure("An error occurred while creating the environment"));
            }
        }

        /// <summary>
        /// Update environment information
        /// </summary>
        [HttpPut("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<EnvironmentDto>>> UpdateEnvironment(int id, [FromBody] UpdateEnvironmentDto updateEnvironmentDto)
        {
            try
            {
                var result = await _environmentService.UpdateEnvironmentAsync(id, updateEnvironmentDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<EnvironmentDto>.Failure("An error occurred while updating the environment"));
            }
        }

        /// <summary>
        /// Delete environment (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteEnvironment(int id)
        {
            try
            {
                var result = await _environmentService.DeleteEnvironmentAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the environment"));
            }
        }

        /// <summary>
        /// Activate environment
        /// </summary>
        [HttpPatch("{id}/activate")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> ActivateEnvironment(int id)
        {
            try
            {
                var result = await _environmentService.ActivateEnvironmentAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while activating the environment"));
            }
        }

        /// <summary>
        /// Deactivate environment
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeactivateEnvironment(int id)
        {
            try
            {
                var result = await _environmentService.DeactivateEnvironmentAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deactivating the environment"));
            }
        }

        /// <summary>
        /// Get environment configuration
        /// </summary>
        [HttpGet("{id}/configuration")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<EnvironmentConfigurationDto>>> GetEnvironmentConfiguration(int id)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentConfigurationAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration for environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<EnvironmentConfigurationDto>.Failure("An error occurred while retrieving environment configuration"));
            }
        }

        /// <summary>
        /// Update environment configuration
        /// </summary>
        [HttpPut("{id}/configuration")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> UpdateEnvironmentConfiguration(int id, [FromBody] UpdateEnvironmentConfigurationDto configDto)
        {
            try
            {
                var result = await _environmentService.UpdateEnvironmentConfigurationAsync(id, configDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration for environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while updating environment configuration"));
            }
        }

        /// <summary>
        /// Get environment variables
        /// </summary>
        [HttpGet("{id}/variables")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<Dictionary<string, string>>>> GetEnvironmentVariables(int id)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentVariablesAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variables for environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<Dictionary<string, string>>.Failure("An error occurred while retrieving environment variables"));
            }
        }

        /// <summary>
        /// Update environment variables
        /// </summary>
        [HttpPut("{id}/variables")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> UpdateEnvironmentVariables(int id, [FromBody] Dictionary<string, string> variables)
        {
            try
            {
                var result = await _environmentService.UpdateEnvironmentVariablesAsync(id, variables);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating variables for environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while updating environment variables"));
            }
        }

        /// <summary>
        /// Get environment workflows
        /// </summary>
        [HttpGet("{environmentId}/workflows")]
        public async Task<ActionResult<ResponseDto<List<EnvironmentWorkflowDto>>>> GetEnvironmentWorkflows(
            int environmentId,
            [FromQuery] WorkflowStatus? status = null)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentWorkflowsAsync(environmentId, status);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows for environment {EnvironmentId}", environmentId);
                return StatusCode(500, ResponseDto<List<EnvironmentWorkflowDto>>.Failure("An error occurred while retrieving environment workflows"));
            }
        }

        /// <summary>
        /// Get environment statistics
        /// </summary>
        [HttpGet("{environmentId}/statistics")]
        public async Task<ActionResult<ResponseDto<EnvironmentStatisticsDto>>> GetEnvironmentStatistics(
            int environmentId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _environmentService.GetEnvironmentStatisticsAsync(environmentId, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for environment {EnvironmentId}", environmentId);
                return StatusCode(500, ResponseDto<EnvironmentStatisticsDto>.Failure("An error occurred while retrieving environment statistics"));
            }
        }

        /// <summary>
        /// Test environment connectivity
        /// </summary>
        [HttpPost("{id}/test")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<EnvironmentTestResultDto>>> TestEnvironment(int id)
        {
            try
            {
                var result = await _environmentService.TestEnvironmentAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<EnvironmentTestResultDto>.Failure("An error occurred while testing the environment"));
            }
        }

        /// <summary>
        /// Clone environment
        /// </summary>
        [HttpPost("{id}/clone")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<EnvironmentDto>>> CloneEnvironment(int id, [FromBody] CloneEnvironmentDto cloneDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _environmentService.CloneEnvironmentAsync(id, cloneDto.Name, cloneDto.Description, currentUserId);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetEnvironment), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning environment {EnvironmentId}", id);
                return StatusCode(500, ResponseDto<EnvironmentDto>.Failure("An error occurred while cloning the environment"));
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
    public class EnvironmentConfigurationDto
    {
        public int EnvironmentId { get; set; }
        public Dictionary<string, string> Variables { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
        public List<string> AllowedIpAddresses { get; set; } = new();
        public string? DatabaseConnectionString { get; set; }
        public string? LogLevel { get; set; }
        public int? TimeoutMinutes { get; set; }
        public bool EnableLogging { get; set; } = true;
        public bool EnableMetrics { get; set; } = true;
    }

    public class UpdateEnvironmentConfigurationDto
    {
        public Dictionary<string, string>? Variables { get; set; }
        public Dictionary<string, object>? Settings { get; set; }
        public List<string>? AllowedIpAddresses { get; set; }
        public string? DatabaseConnectionString { get; set; }
        public string? LogLevel { get; set; }
        public int? TimeoutMinutes { get; set; }
        public bool? EnableLogging { get; set; }
        public bool? EnableMetrics { get; set; }
    }

    public class EnvironmentWorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double SuccessRate { get; set; }
        public bool HasSchedule { get; set; }
        public DateTime? NextScheduledRun { get; set; }
    }

    public class EnvironmentStatisticsDto
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int DraftWorkflows { get; set; }
        public int PublishedWorkflows { get; set; }
        public int ScheduledWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int RunningExecutions { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastExecutionAt { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public Dictionary<string, int> WorkflowStatusDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyExecutionTrend { get; set; } = new();
        public List<TopWorkflowDto> TopPerformingWorkflows { get; set; } = new();
        public List<TopWorkflowDto> TopFailingWorkflows { get; set; } = new();
    }

    public class TopWorkflowDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
    }

    public class EnvironmentTestResultDto
    {
        public bool IsHealthy { get; set; }
        public List<string> TestResults { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime TestedAt { get; set; }
        public TimeSpan TestDuration { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
    }

    public class CloneEnvironmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class EnvironmentDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public EnvironmentConfigurationDto Configuration { get; set; } = new();
        public List<EnvironmentWorkflowDto> Workflows { get; set; } = new();
        public EnvironmentStatisticsDto Statistics { get; set; } = new();
    }