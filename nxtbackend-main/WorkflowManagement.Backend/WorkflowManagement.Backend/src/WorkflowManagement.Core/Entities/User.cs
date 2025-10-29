using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class User : SoftDeleteEntity
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool EmailConfirmed { get; set; } = false;
    
    public DateTime? LastLoginAt { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
    
    public string? Department { get; set; }
    
    public string? JobTitle { get; set; }
    
    // Keep enum for backward compatibility and simple role checking
    public UserRole Role { get; set; } = UserRole.Developer;
    
    // Add support for granular role-based permissions
    public Guid? RoleId { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public DateTime? PasswordChangedAt { get; set; }
    
    public bool RequirePasswordChange { get; set; } = false;
    
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockedOutUntil { get; set; }
    
    public Dictionary<string, object> Preferences { get; set; } = new();
    
    // Navigation Properties
    public Role? DetailedRole { get; set; }
    public ICollection<Workspace> OwnedWorkspaces { get; set; } = new List<Workspace>();
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<Workflow> OwnedWorkflows { get; set; } = new List<Workflow>();
    public ICollection<WorkflowExecution> WorkflowExecutions { get; set; } = new List<WorkflowExecution>();
    public ICollection<WorkflowApproval> WorkflowApprovals { get; set; } = new List<WorkflowApproval>();
    public ICollection<WorkflowApproval> RequestedApprovals { get; set; } = new List<WorkflowApproval>();
    
    // Computed Properties
    public string FullName => $"{FirstName} {LastName}";
    
    public bool IsLockedOut => LockedOutUntil.HasValue && LockedOutUntil.Value > DateTime.UtcNow;
    
    public bool IsPasswordExpired => PasswordChangedAt.HasValue && 
                                   PasswordChangedAt.Value.AddDays(90) < DateTime.UtcNow;
    
    // Helper Methods
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
    }
    
    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockedOutUntil = DateTime.UtcNow.AddMinutes(30);
        }
    }
    
    public bool HasPermission(string permissionName)
    {
        return DetailedRole?.HasPermission(permissionName) ?? false;
    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        ResetFailedLoginAttempts();
    }
}