// UnlockUserCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Users;

public record UnlockUserCommand : IRequest<ResponseDto>
{
    public Guid UserId { get; init; }
    public string UnlockedBy { get; init; } = string.Empty;
    public string? Reason { get; init; }
}