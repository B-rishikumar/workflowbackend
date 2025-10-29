// UpdateUserPreferencesCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Users;

public record UpdateUserPreferencesCommand : IRequest<ResponseDto>
{
    public Guid UserId { get; init; }
    public Dictionary<string, object> Preferences { get; init; } = new();
    public string UpdatedBy { get; init; } = string.Empty;
}