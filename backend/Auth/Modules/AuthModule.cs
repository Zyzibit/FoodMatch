using inzynierka.Auth.Contracts;
using inzynierka.Auth.Contracts.Models;
using inzynierka.Auth.Services;

namespace inzynierka.Auth.Modules;

public class AuthModule(IAuthService authService) : IAuthContract
{
    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        return await authService.ValidateTokenAsync(token);
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        return await authService.AuthenticateAsync(username, password, deviceId, userAgent, ipAddress);
    }

    public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        return await authService.RegisterAsync(username, email, password, deviceId, userAgent, ipAddress);
    }

    public async Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken)
    {
        return await authService.RefreshTokenAsync(refreshToken);
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        return await authService.RevokeTokenAsync(token);
    }

    public async Task<bool> RevokeAllTokensAsync(string userId)
    {
        return await authService.RevokeAllTokensAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        return await authService.ChangePasswordAsync(userId, currentPassword, newPassword);
    }


    public async Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null)
    {
        return await authService.GetUserSessionsAsync(userId, currentRefreshToken);
    }
}