// UpdateUserProfileCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;

namespace WorkflowManagement.Application.Commands.Users;

public record UpdateUserProfileCommand : IRequest<ResponseDto<UserDto>>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public Dictionary<string, object> Preferences { get; init; } = new();
}
