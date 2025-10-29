// GetUserHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Application.Queries.Users;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Queries;

public class GetUserHandler : IRequestHandler<GetUserQuery, ResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(IUserRepository userRepository, IMapper mapper, ILogger<GetUserHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting user with ID: {UserId}", request.Id);

            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User", request.Id);
            }

            var userDto = _mapper.Map<UserDto>(user);

            return new ResponseDto<UserDto>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with ID: {UserId}", request.Id);
            return new ResponseDto<UserDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}