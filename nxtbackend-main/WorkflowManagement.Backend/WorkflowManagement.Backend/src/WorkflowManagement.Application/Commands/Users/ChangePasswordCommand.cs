// ChangePasswordCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Users;

public record ChangePasswordCommand : IRequest<ResponseDto>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}