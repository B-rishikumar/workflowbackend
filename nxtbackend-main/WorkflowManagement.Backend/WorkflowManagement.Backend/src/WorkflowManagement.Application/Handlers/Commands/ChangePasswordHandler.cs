// ChangePasswordHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Commands.Users;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Shared.Helpers;

namespace WorkflowManagement.Application.Handlers.Commands;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(IUserRepository userRepository, ILogger<ChangePasswordHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ResponseDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Changing password for user: {UserId}", request.UserId);

            // Validate passwords match
            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new ValidationException("ConfirmPassword", "Passwords do not match");
            }

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User", request.UserId);
            }

            // Verify current password
            if (!CryptographyHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new ValidationException("CurrentPassword", "Current password is incorrect");
            }

            // Update password
            user.PasswordHash = CryptographyHelper.HashPassword(request.NewPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            user.RequirePasswordChange = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Successfully changed password for user: {UserId}", request.UserId);

            return new ResponseDto
            {
                Success = true,
                Message = "Password changed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
            return new ResponseDto
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}