// User/CreateUserDto.cs
using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.User;

public class CreateUserDto
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
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    public UserRole Role { get; set; } = UserRole.Developer;
    
    public string? Department { get; set; }
    
    public string? JobTitle { get; set; }
}
