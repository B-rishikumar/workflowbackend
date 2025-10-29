using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.Auth;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.API.Controllers;

/// <summary>
/// Authentication and authorization endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and get access token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        var result = await _authService.LoginAsync(request.Username, request.Password, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}. Reason: {Message}", 
                request.Username, result.Message);
            
            return Unauthorized(new ResponseDto
            {
                Success = false,
                Message = result.Message
            });
        }

        var response = new LoginResponseDto
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // JWT expiry time
            User = new Application.DTOs.User.UserDto() // You'll need to get user details
        };

        _logger.LogInformation("Successful login for username: {Username}", request.Username);

        return Ok(new ResponseDto<LoginResponseDto>
        {
            Success = true,
            Message = "Login successful",
            Data = response
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto<LoginResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Token refresh attempt");

        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed token refresh attempt. Reason: {Message}", result.Message);
            
            return Unauthorized(new ResponseDto
            {
                Success = false,
                Message = result.Message
            });
        }

        var response = new LoginResponseDto
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new Application.DTOs.User.UserDto() // You'll need to get user details
        };

        _logger.LogInformation("Successful token refresh");

        return Ok(new ResponseDto<LoginResponseDto>
        {
            Success = true,
            Message = "Token refreshed successfully",
            Data = response
        });
    }

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    /// <param name="request">Refresh token to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseDto>> Logout(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logout attempt for user: {UserId}", User.Identity?.Name);

        var result = await _authService.LogoutAsync(request.RefreshToken, cancellationToken);

        if (!result)
        {
            return BadRequest(new ResponseDto
            {
                Success = false,
                Message = "Failed to logout"
            });
        }

        _logger.LogInformation("Successful logout for user: {UserId}", User.Identity?.Name);

        return Ok(new ResponseDto
        {
            Success = true,
            Message = "Logout successful"
        });
    }

    /// <summary>
    /// Validate current access token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token validation result</returns>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto>> ValidateToken(CancellationToken cancellationToken = default)
    {
        // If we reach here, the token is valid (due to [Authorize] attribute)
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring(7);
            var isValid = await _authService.ValidateTokenAsync(token, cancellationToken);
            
            if (isValid)
            {
                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Token is valid"
                });
            }
        }

        return Unauthorized(new ResponseDto
        {
            Success = false,
            Message = "Invalid token"
        });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto<Application.DTOs.User.UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto<Application.DTOs.User.UserDto>>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring(7);
            var userId = await _authService.GetUserIdFromTokenAsync(token, cancellationToken);
            
            if (userId.HasValue)
            {
                // You'll need to implement IUserService to get user details
                // var user = await _userService.GetByIdAsync(userId.Value, cancellationToken);
                
                return Ok(new ResponseDto<Application.DTOs.User.UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = new Application.DTOs.User.UserDto() // Replace with actual user data
                });
            }
        }

        return Unauthorized(new ResponseDto
        {
            Success = false,
            Message = "Unable to retrieve user information"
        });
    }
}