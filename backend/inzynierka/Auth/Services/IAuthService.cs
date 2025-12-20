using inzynierka.Auth.Responses;

namespace inzynierka.Auth.Services;

public interface IAuthService {
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? deviceId = null,
        string? userAgent = null, string? ipAddress = null);

    Task<AuthenticationResult> RegisterAsync(string username, string email, string password, string? deviceId = null,
        string? userAgent = null, string? ipAddress = null);

    Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> RevokeAllTokensAsync(string userId);
    Task<TokenValidationResult> ValidateTokenAsync(string accessToken);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null);
    Task<ForgotPasswordResult> ForgotPasswordAsync(string email);
    Task<ResetPasswordResult> ResetPasswordAsync(string email, string token, string newPassword);
}
