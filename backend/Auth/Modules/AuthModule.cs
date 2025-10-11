using inzynierka.Auth.Contracts;
using inzynierka.Auth.Contracts.Models;
using inzynierka.Auth.Services;

namespace inzynierka.Auth.Modules;


public class AuthModule : IAuthContract
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthModule> _logger;

    public AuthModule(IAuthService authService, ILogger<AuthModule> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        return await _authService.ValidateTokenAsync(token);
    }

    public async Task<UserInfo> GetUserInfoAsync(string userId)
    {
        return await _authService.GetUserInfoAsync(userId);
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        return await _authService.AuthenticateAsync(username, password, deviceId, userAgent, ipAddress);
    }

    public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        return await _authService.RegisterAsync(username, email, password, deviceId, userAgent, ipAddress);
    }

    public async Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken)
    {
        return await _authService.RefreshTokenAsync(refreshToken);
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        return await _authService.RevokeTokenAsync(token);
    }

    public async Task<bool> RevokeAllTokensAsync(string userId)
    {
        return await _authService.RevokeAllTokensAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        return await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, string? email = null, string? name = null)
    {
        return await _authService.UpdateUserProfileAsync(userId, email, name);
    }

    public async Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null)
    {
        return await _authService.GetUserSessionsAsync(userId, currentRefreshToken);
    }
}