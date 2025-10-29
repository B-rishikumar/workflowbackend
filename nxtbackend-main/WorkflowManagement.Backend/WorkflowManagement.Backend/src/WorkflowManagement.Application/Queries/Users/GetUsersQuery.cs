using MediatR;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.Queries.Users
{
    /// <summary>
    /// Query to get paginated list of users
    /// </summary>
    public class GetUsersQuery : IRequest<ResponseDto<PagedResultDto<UserDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;

        public GetUsersQuery() { }

        public GetUsersQuery(int page, int pageSize, string? searchTerm = null, 
            UserRole? role = null, bool? isActive = null, 
            string? sortBy = "CreatedAt", bool sortDescending = true)
        {
            Page = page;
            PageSize = pageSize;
            SearchTerm = searchTerm;
            Role = role;
            IsActive = isActive;
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }

    /// <summary>
    /// Query to get all active users (for dropdown lists, etc.)
    /// </summary>
    public class GetActiveUsersQuery : IRequest<ResponseDto<List<UserDto>>>
    {
        public int? WorkspaceId { get; set; }
        public UserRole? Role { get; set; }

        public GetActiveUsersQuery(int? workspaceId = null, UserRole? role = null)
        {
            WorkspaceId = workspaceId;
            Role = role;
        }
    }

    /// <summary>
    /// Query to get users by workspace
    /// </summary>
    public class GetUsersByWorkspaceQuery : IRequest<ResponseDto<List<UserDto>>>
    {
        public int WorkspaceId { get; set; }
        public bool IncludeInactive { get; set; } = false;

        public GetUsersByWorkspaceQuery(int workspaceId, bool includeInactive = false)
        {
            WorkspaceId = workspaceId;
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query to get users by role
    /// </summary>
    public class GetUsersByRoleQuery : IRequest<ResponseDto<List<UserDto>>>
    {
        public UserRole Role { get; set; }
        public int? WorkspaceId { get; set; }

        public GetUsersByRoleQuery(UserRole role, int? workspaceId = null)
        {
            Role = role;
            WorkspaceId = workspaceId;
        }
    }

    /// <summary>
    /// Query to search users with advanced filters
    /// </summary>
    public class SearchUsersQuery : IRequest<ResponseDto<PagedResultDto<UserDto>>>
    {
        public string? SearchTerm { get; set; }
        public List<UserRole>? Roles { get; set; }
        public List<int>? WorkspaceIds { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? LastLoginAfter { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}