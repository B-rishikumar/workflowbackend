using MediatR;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Application.Queries.Users
{
    /// <summary>
    /// Query to get a single user by ID
    /// </summary>
    public class GetUserQuery : IRequest<ResponseDto<UserDto>>
    {
        public int UserId { get; set; }

        public GetUserQuery(int userId)
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Query to get a user by email
    /// </summary>
    public class GetUserByEmailQuery : IRequest<ResponseDto<UserDto>>
    {
        public string Email { get; set; }

        public GetUserByEmailQuery(string email)
        {
            Email = email;
        }
    }

    /// <summary>
    /// Query to get user with detailed information including roles and permissions
    /// </summary>
    public class GetUserDetailQuery : IRequest<ResponseDto<UserDetailDto>>
    {
        public int UserId { get; set; }

        public GetUserDetailQuery(int userId)
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Query to get current user profile
    /// </summary>
    public class GetCurrentUserQuery : IRequest<ResponseDto<UserDto>>
    {
        public string? CurrentUserEmail { get; set; }

        public GetCurrentUserQuery(string? currentUserEmail = null)
        {
            CurrentUserEmail = currentUserEmail;
        }
    }
}