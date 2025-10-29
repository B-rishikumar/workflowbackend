// CreateUserCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Commands.Users;

public record CreateUserCommand : IRequest<ResponseDto<UserDto>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public UserRole Role { get; init; } = UserRole.Developer;
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public Guid? RoleId { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}
