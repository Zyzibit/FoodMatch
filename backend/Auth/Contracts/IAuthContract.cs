using inzynierka.Auth.Contracts.Models;

namespace inzynierka.Auth.Contracts;

/// <summary>
/// Kontrakt dla modu³u autoryzacji - definiuje interfejs komunikacji
/// </summary>
public interface IAuthContract
{
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    Task<UserInfo> GetUserInfoAsync(string userId);
    Task<AuthenticationResult> AuthenticateAsync(string username, string password);
    Task<AuthenticationResult> RegisterAsync(string username, string email, string password);
    Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string token);
}