// User/UpdateUserDto.cs
using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.User;

public class UpdateUserDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    public UserRole Role { get; set; }
    
    public string? Department { get; set; }
    
    public string? JobTitle { get; set; }
    
    public bool IsActive { get; set; }
}
