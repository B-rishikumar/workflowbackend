namespace WorkflowManagement.Core.Interfaces.Services;

public interface IAuthService
{
    Task<(bool Success, string Token, string RefreshToken, string Message)> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string Token, string RefreshToken, string Message)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
}