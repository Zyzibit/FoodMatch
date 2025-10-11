using inzynierka.Auth.Contracts.Models;

namespace inzynierka.Auth.Contracts;

/// <summary>
/// Kontrakt dla modu³u autoryzacji - definiuje interfejs komunikacji
/// </summary>
public interface IAuthContract
{
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    Task<UserInfo> GetUserInfoAsync(string userId);
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null);
    Task<AuthenticationResult> RegisterAsync(string username, string email, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null);
    Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> RevokeAllTokensAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> UpdateUserProfileAsync(string userId, string? email = null, string? name = null);
    Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null);
}