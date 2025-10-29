// UserService.cs
using AutoMapper;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Shared.Helpers;
using Microsoft.Extensions.Logging;
namespace WorkflowManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByEmailAsync(email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByUsernameAsync(username, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAllAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _userRepository.GetPagedAsync(
                pageNumber, pageSize,
                u => u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                     u.LastName.ToLower().Contains(lowerSearchTerm) ||
                     u.Email.ToLower().Contains(lowerSearchTerm) ||
                     u.Username.ToLower().Contains(lowerSearchTerm),
                cancellationToken: cancellationToken);
        }

        return await _userRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken: cancellationToken);
    }

    public async Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(user.Email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Check if username already exists
        if (await _userRepository.UsernameExistsAsync(user.Username, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Hash password
        user.PasswordHash = CryptographyHelper.HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        if (existingUser == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if email already exists (excluding current user)
        if (await _userRepository.EmailExistsAsync(user.Email, user.Id, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Update properties
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.Role = user.Role;
        existingUser.Department = user.Department;
        existingUser.JobTitle = user.JobTitle;
        existingUser.IsActive = user.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateAsync(existingUser, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return false;
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        if (!CryptographyHelper.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = CryptographyHelper.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            return false;
        }

        // Generate temporary password
        var tempPassword = CryptographyHelper.GenerateApiKey(12);
        user.PasswordHash = CryptographyHelper.HashPassword(tempPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        // TODO: Send email with temporary password
        _logger.LogInformation("Password reset for user {UserId}. Temporary password: {TempPassword}", 
            user.Id, tempPassword);

        return true;
    }
}
