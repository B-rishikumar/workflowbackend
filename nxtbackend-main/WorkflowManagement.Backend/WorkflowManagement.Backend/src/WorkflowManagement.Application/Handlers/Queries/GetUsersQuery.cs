// GetUsersQuery.cs
using MediatR;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Queries.Users;

public record GetUsersQuery : IRequest<ResponseDto<PagedResultDto<UserDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
    public string? Department { get; init; }
}