// AssignUserRoleCommand.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Commands.Users;

public record AssignUserRoleCommand : IRequest<ResponseDto>
{
    public Guid UserId { get; init; }
    public UserRole Role { get; init; }
    public Guid? DetailedRoleId { get; init; }
    public string AssignedBy { get; init; } = string.Empty;
    public string? Reason { get; init; }
}