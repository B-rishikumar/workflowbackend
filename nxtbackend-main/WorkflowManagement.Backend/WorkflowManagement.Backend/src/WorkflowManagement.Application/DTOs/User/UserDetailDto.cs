using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.User
{
    /// <summary>
    /// Detailed user DTO with roles, permissions, and workspace information
    /// </summary>
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? Manager { get; set; }
        public string? TimeZone { get; set; } = "UTC";

        // Related entities
        public List<UserWorkspaceDto> Workspaces { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<UserRoleDto> Roles { get; set; } = new();

        // Activity statistics
        public int TotalWorkflowsCreated { get; set; }
        public int TotalWorkflowsExecuted { get; set; }
        public DateTime? LastWorkflowCreated { get; set; }
        public DateTime? LastWorkflowExecuted { get; set; }
    }

    /// <summary>
    /// User workspace relationship DTO
    /// </summary>
    public class UserWorkspaceDto
    {
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string WorkspaceDescription { get; set; } = string.Empty;
        public UserRole RoleInWorkspace { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// User role DTO
    /// </summary>
    public class UserRoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}