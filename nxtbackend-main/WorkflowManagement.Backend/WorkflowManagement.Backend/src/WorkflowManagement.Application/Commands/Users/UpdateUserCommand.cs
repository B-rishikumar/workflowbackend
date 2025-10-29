// UpdateUserCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Commands.Users;

public record UpdateUserCommand : IRequest<ResponseDto<UserDto>>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public UserRole Role { get; init; }
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public bool IsActive { get; init; }
    public Guid? RoleId { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
}