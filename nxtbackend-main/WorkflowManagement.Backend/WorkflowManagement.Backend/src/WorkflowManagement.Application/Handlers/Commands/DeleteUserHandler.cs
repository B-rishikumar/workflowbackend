// DeleteUserHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Commands.Users;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Commands;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, ResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(IUserRepository userRepository, ILogger<DeleteUserHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ResponseDto> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);

            var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingUser == null)
            {
                throw new NotFoundException("User", request.Id);
            }

            // Soft delete
            existingUser.IsDeleted = true;
            existingUser.DeletedAt = DateTime.UtcNow;
            existingUser.DeletedBy = request.DeletedBy;
            existingUser.DeleteReason = request.DeleteReason;
            existingUser.IsActive = false;

            await _userRepository.UpdateAsync(existingUser, cancellationToken);

            _logger.LogInformation("Successfully deleted user with ID: {UserId}", request.Id);

            return new ResponseDto
            {
                Success = true,
                Message = "User deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", request.Id);
            return new ResponseDto
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}