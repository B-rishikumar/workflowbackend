// LockUserCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Users;

public record LockUserCommand : IRequest<ResponseDto>
{
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime? LockoutEnd { get; init; }
    public string LockedBy { get; init; } = string.Empty;
}
