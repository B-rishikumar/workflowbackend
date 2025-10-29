// AuthService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Shared.Helpers;

namespace WorkflowManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string Token, string RefreshToken, string Message)> LoginAsync(
        string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(username, cancellationToken);
            }

            if (user == null || !user.IsActive)
            {
                return (false, string.Empty, string.Empty, "Invalid credentials");
            }

            if (!CryptographyHelper.VerifyPassword(password, user.PasswordHash))
            {
                return (false, string.Empty, string.Empty, "Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Update user with refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user, cancellationToken);

            return (true, token, refreshToken, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", username);
            return (false, string.Empty, string.Empty, "An error occurred during login");
        }
    }

    public async Task<(bool Success, string Token, string RefreshToken, string Message)> RefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken, cancellationToken);

            if (user == null || !user.IsActive || 
                user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return (false, string.Empty, string.Empty, "Invalid or expired refresh token");
            }

            var token = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _userRepository.UpdateAsync(user, cancellationToken);

            return (true, token, newRefreshToken, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return (false, string.Empty, string.Empty, "An error occurred during token refresh");
        }
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken, cancellationToken);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtToken(Core.Entities.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return CryptographyHelper.GenerateRandomToken(64);
    }
}