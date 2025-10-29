// DeleteUserCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Commands.Users;

public record DeleteUserCommand : IRequest<ResponseDto>
{
    public Guid Id { get; init; }
    public string DeletedBy { get; init; } = string.Empty;
    public string? DeleteReason { get; init; }
}