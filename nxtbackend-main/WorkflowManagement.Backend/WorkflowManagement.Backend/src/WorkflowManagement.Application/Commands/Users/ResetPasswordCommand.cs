// ResetPasswordCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Users;

public record ResetPasswordCommand : IRequest<ResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string ResetBy { get; init; } = string.Empty;
}
