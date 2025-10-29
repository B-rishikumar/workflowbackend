// IUserRepository.cs
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
}