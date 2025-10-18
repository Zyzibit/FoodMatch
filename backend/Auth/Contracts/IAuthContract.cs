using inzynierka.Auth.Contracts.Models;

namespace inzynierka.Auth.Contracts;

public interface IAuthContract
{
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null);
    Task<AuthenticationResult> RegisterAsync(string username, string email, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null);
    
    Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> RevokeAllTokensAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null);
}