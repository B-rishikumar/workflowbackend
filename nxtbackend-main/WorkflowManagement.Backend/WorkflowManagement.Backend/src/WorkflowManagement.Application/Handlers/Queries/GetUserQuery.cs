// GetUserQuery.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;

namespace WorkflowManagement.Application.Queries.Users;

public record GetUserQuery : IRequest<ResponseDto<UserDto>>
{
    public Guid Id { get; init; }
}
