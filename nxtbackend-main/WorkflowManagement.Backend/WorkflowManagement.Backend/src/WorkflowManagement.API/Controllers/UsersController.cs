using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WorkflowManagement.Application.Commands.Users;
using WorkflowManagement.Application.Queries.Users;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, IUserService userService, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination and filtering
        /// </summary>
        [HttpGet]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<UserDto>>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] UserRole? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var query = new GetUsersQuery(page, pageSize, searchTerm, role, isActive, sortBy, sortDescending);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, ResponseDto<PagedResultDto<UserDto>>.Failure("An error occurred while retrieving users"));
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<UserDto>>> GetUser(int id)
        {
            try
            {
                var query = new GetUserQuery(id);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, ResponseDto<UserDto>.Failure("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Get user with detailed information
        /// </summary>
        [HttpGet("{id}/detail")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<UserDetailDto>>> GetUserDetail(int id)
        {
            try
            {
                var query = new GetUserDetailQuery(id);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail {UserId}", id);
                return StatusCode(500, ResponseDto<UserDetailDto>.Failure("An error occurred while retrieving user details"));
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ResponseDto<UserDto>>> GetCurrentUser()
        {
            try
            {
                var currentUserEmail = User.Identity?.Name;
                var query = new GetCurrentUserQuery(currentUserEmail);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ResponseDto<UserDto>.Failure("An error occurred while retrieving current user"));
            }
        }

        /// <summary>
        /// Get active users for dropdown lists
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResponseDto<List<UserDto>>>> GetActiveUsers(
            [FromQuery] int? workspaceId = null,
            [FromQuery] UserRole? role = null)
        {
            try
            {
                var query = new GetActiveUsersQuery(workspaceId, role);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users");
                return StatusCode(500, ResponseDto<List<UserDto>>.Failure("An error occurred while retrieving active users"));
            }
        }

        /// <summary>
        /// Get users by workspace
        /// </summary>
        [HttpGet("workspace/{workspaceId}")]
        public async Task<ActionResult<ResponseDto<List<UserDto>>>> GetUsersByWorkspace(
            int workspaceId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetUsersByWorkspaceQuery(workspaceId, includeInactive);
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, ResponseDto<List<UserDto>>.Failure("An error occurred while retrieving workspace users"));
            }
        }

        /// <summary>
        /// Search users with advanced filters
        /// </summary>
        [HttpPost("search")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<UserDto>>>> SearchUsers([FromBody] SearchUsersQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, ResponseDto<PagedResultDto<UserDto>>.Failure("An error occurred while searching users"));
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var command = new CreateUserCommand
                {
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Email = createUserDto.Email,
                    PhoneNumber = createUserDto.PhoneNumber,
                    Role = createUserDto.Role,
                    Department = createUserDto.Department,
                    JobTitle = createUserDto.JobTitle,
                    Manager = createUserDto.Manager,
                    TimeZone = createUserDto.TimeZone
                };

                var result = await _mediator.Send(command);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetUser), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, ResponseDto<UserDto>.Failure("An error occurred while creating the user"));
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDto<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only update their own profile unless they are admin/manager
                if (currentUserId != id && !IsAdminOrManager())
                {
                    return Forbid();
                }

                var command = new UpdateUserCommand
                {
                    Id = id,
                    FirstName = updateUserDto.FirstName,
                    LastName = updateUserDto.LastName,
                    PhoneNumber = updateUserDto.PhoneNumber,
                    Department = updateUserDto.Department,
                    JobTitle = updateUserDto.JobTitle,
                    Manager = updateUserDto.Manager,
                    TimeZone = updateUserDto.TimeZone
                };

                // Only admins can update role
                if (IsAdmin())
                {
                    command.Role = updateUserDto.Role;
                    command.IsActive = updateUserDto.IsActive;
                }

                var result = await _mediator.Send(command);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, ResponseDto<UserDto>.Failure("An error occurred while updating the user"));
            }
        }

        /// <summary>
        /// Deactivate user
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<bool>>> DeactivateUser(int id)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deactivating the user"));
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        [HttpPatch("{id}/activate")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<bool>>> ActivateUser(int id)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while activating the user"));
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the user"));
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult<ResponseDto<bool>>> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only change their own password unless they are admin
                if (currentUserId != id && !IsAdmin())
                {
                    return Forbid();
                }

                var result = await _userService.ChangePasswordAsync(id, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while changing the password"));
            }
        }

        /// <summary>
        /// Reset user password (admin only)
        /// </summary>
        [HttpPost("{id}/reset-password")]
        [RequireRole(UserRole.Admin)]
        public async Task<ActionResult<ResponseDto<string>>> ResetPassword(int id)
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", id);
                return StatusCode(500, ResponseDto<string>.Failure("An error occurred while resetting the password"));
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
        }

        private bool IsAdmin()
        {
            return User.IsInRole(UserRole.Admin.ToString());
        }

        private bool IsAdminOrManager()
        {
            return User.IsInRole(UserRole.Admin.ToString()) || User.IsInRole(UserRole.ProjectManager.ToString());
        }

        #endregion
    }

    /// <summary>
    /// DTO for changing password
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}