// CreateUserHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Commands.Users;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Shared.Helpers;

namespace WorkflowManagement.Application.Handlers.Commands;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, ResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(IUserRepository userRepository, IMapper mapper, ILogger<CreateUserHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating user with email: {Email}", request.Email);

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            {
                throw new ValidationException("Email", request.Email, "Email address already exists");
            }

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken: cancellationToken))
            {
                throw new ValidationException("Username", request.Username, "Username already exists");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = CryptographyHelper.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                Department = request.Department,
                JobTitle = request.JobTitle,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedBy = request.CreatedBy,
                UpdatedBy = request.CreatedBy
            };

            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            var userDto = _mapper.Map<UserDto>(createdUser);

            _logger.LogInformation("Successfully created user with ID: {UserId}", createdUser.Id);

            return new ResponseDto<UserDto>
            {
                Success = true,
                Message = "User created successfully",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
            return new ResponseDto<UserDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}
