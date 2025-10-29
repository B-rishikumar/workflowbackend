// UpdateUserHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Commands.Users;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Commands;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(IUserRepository userRepository, IMapper mapper, ILogger<UpdateUserHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", request.Id);

            var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingUser == null)
            {
                throw new NotFoundException("User", request.Id);
            }

            // Check if email already exists (excluding current user)
            if (await _userRepository.EmailExistsAsync(request.Email, request.Id, cancellationToken))
            {
                throw new ValidationException("Email", request.Email, "Email address already exists");
            }

            // Update user properties
            existingUser.FirstName = request.FirstName;
            existingUser.LastName = request.LastName;
            existingUser.Email = request.Email;
            existingUser.PhoneNumber = request.PhoneNumber;
            existingUser.Role = request.Role;
            existingUser.Department = request.Department;
            existingUser.JobTitle = request.JobTitle;
            existingUser.IsActive = request.IsActive;
            existingUser.RoleId = request.RoleId;
            existingUser.UpdatedBy = request.UpdatedBy;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(existingUser, cancellationToken);
            var userDto = _mapper.Map<UserDto>(updatedUser);

            _logger.LogInformation("Successfully updated user with ID: {UserId}", request.Id);

            return new ResponseDto<UserDto>
            {
                Success = true,
                Message = "User updated successfully",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", request.Id);
            return new ResponseDto<UserDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}
