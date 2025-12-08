using inzynierka.Auth.Responses;
using inzynierka.Auth.Model;
using inzynierka.Auth.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using inzynierka.Users.Model;
using inzynierka.Users.Services;

namespace inzynierka.Auth.Services;

public class AuthService : IAuthService 
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IEmailService _emailService;

    // Token configuration
    private int AccessTokenExpirationMinutes =>
        int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");

    private int RefreshTokenExpirationDays =>
        int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");

    public AuthService(
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IUserService userService,
        IRoleService roleService,
        IEmailService emailService) {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
        _userService = userService;
        _roleService = roleService;
        _emailService = emailService;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null) {
        try {
            var user = await _userService.GetUserEntityByUsernameAsync(username);
            if (user == null) {
                return AuthenticationResult.Failed("Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) {
                return AuthenticationResult.Failed("Invalid username or password");
            }

            return await GenerateTokensAsync(user, deviceId, userAgent, ipAddress);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error during authentication for user: {Username}", username);
            return AuthenticationResult.Failed("Authentication failed");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(
        string username,
        string email,
        string password,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null) {
        try {
            var createResult = await _userService.CreateUserAsync(username, email, password);

            if (!createResult.Success) {
                return AuthenticationResult.Failed(createResult.ErrorMessage ?? "Registration failed");
            }

            if (createResult.User == null) {
                return AuthenticationResult.Failed("Failed to create user");
            }

            return await GenerateTokensAsync(createResult.User, deviceId, userAgent, ipAddress);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error during registration for user: {Username}", username);
            return AuthenticationResult.Failed("Registration failed");
        }
    }

    public async Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken) {
        try {
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (!IsValidRefreshToken(storedRefreshToken)) {
                return TokenRefreshResult.Failed("Invalid refresh token");
            }

            var user = storedRefreshToken!.User;
            if (user == null) {
                return TokenRefreshResult.Failed("User not found");
            }

            await RevokeRefreshTokenAsync(storedRefreshToken);

            var authResult = await GenerateTokensAsync(
                user,
                storedRefreshToken.DeviceId,
                storedRefreshToken.UserAgent,
                storedRefreshToken.IpAddress);

            if (!authResult.Success) {
                return TokenRefreshResult.Failed("Failed to generate new tokens");
            }


            return new TokenRefreshResult {
                Success = true,
                AccessToken = authResult.AccessToken,
                RefreshToken = authResult.RefreshToken,
                ExpiresAt = authResult.ExpiresAt
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error refreshing token");
            return TokenRefreshResult.Failed("Token refresh failed");
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken) {
        try {
            await _refreshTokenRepository.RevokeAsync(refreshToken);
            return true;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    public async Task<bool> RevokeAllTokensAsync(string userId) {
        try {
            await _refreshTokenRepository.RevokeAllByUserIdAsync(userId);

            return true;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string accessToken) {
        try {
            var tokenClaims = ParseAccessToken(accessToken);

            if (string.IsNullOrEmpty(tokenClaims.Username)) {
                return TokenValidationResult.Failed("Invalid token: no username found");
            }

            var user = await _userService.GetUserEntityByUsernameAsync(tokenClaims.Username);
            if (user == null) {
                return TokenValidationResult.Failed("User not found");
            }


            return new TokenValidationResult {
                IsValid = true,
                UserId = user.Id,
                Username = tokenClaims.Username,
                Roles = tokenClaims.Roles,
                ExpiresAt = tokenClaims.ExpiresAt
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error validating token");
            return TokenValidationResult.Failed(ex.Message);
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword) {
        try {
            var user = await _userService.GetUserEntityByIdAsync(userId);
            if (user == null) {
                return false;
            }

            var result = await _userService.ChangePasswordAsync(user.Id, currentPassword, newPassword);
            if (result) {
                await RevokeAllTokensAsync(userId);
            }

            return result;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null) {
        try {
            var refreshTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

            return refreshTokens
                .Select(rt => MapToUserSession(rt, currentRefreshToken))
                .ToList();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error getting user sessions for userId: {UserId}", userId);
            throw;
        }
    }


    private async Task<AuthenticationResult> GenerateTokensAsync(
        User user,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null) {
        try {
            var roles = await _roleService.GetUserRolesAsync(user.Id);
            var claims = BuildUserClaims(user, roles);

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

            await HandleDeviceTokenAsync(user.Id, deviceId);
            await StoreRefreshTokenAsync(user.Id, refreshToken, deviceId, userAgent, ipAddress);

            return new AuthenticationResult {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = new UserInfo {
                    UserId = user.Id,
                    Username = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Roles = roles.ToList()
                }
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error generating tokens for user: {UserId}", user.Id);
            return AuthenticationResult.Failed("Failed to generate tokens");
        }
    }

    private List<Claim> BuildUserClaims(User user, IList<string> roles) {
        var claims = new List<Claim> {
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    private async Task HandleDeviceTokenAsync(string userId, string? deviceId) {
        if (string.IsNullOrEmpty(deviceId)) {
            _logger.LogWarning("DeviceId is null or empty for user {UserId}", userId);
            return;
        }

        _logger.LogDebug("Checking for existing token for user {UserId} and device {DeviceId}", userId, deviceId);

        var existingToken = await _refreshTokenRepository.GetByUserIdAndDeviceIdAsync(userId, deviceId);

        if (existingToken != null) {
            _logger.LogInformation(
                "Revoking existing token {TokenId} for device {DeviceId} of user {UserId}",
                existingToken.Id, deviceId, userId);

            await _refreshTokenRepository.RevokeAsync(existingToken.Token);
        }
        else {
            _logger.LogDebug("No existing token found for device {DeviceId} of user {UserId}", deviceId, userId);
        }
    }

    private async Task StoreRefreshTokenAsync(
        string userId,
        string refreshToken,
        string? deviceId,
        string? userAgent,
        string? ipAddress) {
        var refreshTokenEntity = new RefreshToken {
            Token = refreshToken,
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            DeviceId = deviceId,
            UserAgent = userAgent,
            IpAddress = ipAddress
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        _logger.LogInformation("Created new refresh token for user {UserId} with device {DeviceId}", userId, deviceId);
    }

    private bool IsValidRefreshToken(RefreshToken? token) {
        return token != null
               && token.RevokedAt == null
               && token.ExpiryDate > DateTime.UtcNow;
    }

    private async Task RevokeRefreshTokenAsync(RefreshToken token) {
        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(token);
    }

    private (string? Username, List<string> Roles, DateTime? ExpiresAt) ParseAccessToken(string accessToken) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(accessToken);

        var username = jsonToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value ??
                       jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        var roles = jsonToken.Claims
            .Where(x => x.Type == ClaimTypes.Role ||
                        x.Type == "role")
            .Select(x => x.Value)
            .ToList();

        var exp = jsonToken.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
        var expiresAt = exp != null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime
            : (DateTime?)null;

        return (username, roles, expiresAt);
    }

    private UserSession MapToUserSession(RefreshToken token, string? currentRefreshToken) {
        return new UserSession {
            DeviceId = token.DeviceId ?? "Unknown",
            UserAgent = token.UserAgent ?? "Unknown",
            IpAddress = token.IpAddress ?? "Unknown",
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiryDate,
            IsActive = token.RevokedAt == null && token.ExpiryDate > DateTime.UtcNow,
            IsCurrent = !string.IsNullOrEmpty(currentRefreshToken) && token.Token == currentRefreshToken
        };
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email) {
        try {
            var user = await _userService.GetUserEntityByEmailAsync(email);
            if (user == null) {
                _logger.LogWarning("Forgot password request for non-existent email: {Email}", email);
                return ForgotPasswordResult.Succeeded("token_placeholder");
            }

            var token = await _signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);

            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                email, 
                token, 
                user.UserName ?? email
            );

            if (!emailSent) {
                _logger.LogWarning("Failed to send password reset email to {Email}, but token was generated", email);
            }

            return ForgotPasswordResult.Succeeded(token);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error generating password reset token for email: {Email}", email);
            return ForgotPasswordResult.Failed("Failed to generate password reset token");
        }
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(string email, string token, string newPassword) {
        try {
            var user = await _userService.GetUserEntityByEmailAsync(email);
            if (user == null) {
                _logger.LogWarning("Reset password attempt for non-existent email: {Email}", email);
                return ResetPasswordResult.Failed("Invalid request");
            }

            var result = await _signInManager.UserManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded) {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to reset password for user {UserId}: {Errors}", user.Id, errors);
                return ResetPasswordResult.Failed(errors);
            }
            await RevokeAllTokensAsync(user.Id);
            
            _logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
            return ResetPasswordResult.Succeeded();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error resetting password for email: {Email}", email);
            return ResetPasswordResult.Failed("Failed to reset password");
        }
    }
}







