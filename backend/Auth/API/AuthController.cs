using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inzynierka.Auth.Contracts;
using inzynierka.Auth.Contracts.Models;
using inzynierka.Auth.Services;
using inzynierka.Auth.Utilities;
using System.Security.Claims;

namespace inzynierka.Auth.API;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthContract _authModule;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthContract authModule, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _authModule = authModule;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Logowanie u¿ytkownika
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var (deviceId, userAgent, ipAddress) = DeviceInfoHelper.ExtractDeviceInfo(Request, _logger);
            _logger.LogInformation("Login attempt for user {Username} from device {DeviceId} (IP: {IpAddress})",
                request.Username, deviceId, ipAddress);

            var result = await _authModule.AuthenticateAsync(request.Username, request.Password, deviceId, userAgent, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            // Ustaw tokeny w ciasteczkach
            var accessTokenExpirationMinutes = 60;
            var refreshTokenExpirationDays = 7;

            _tokenService.SetAccessTokenCookie(Response, result.AccessToken, accessTokenExpirationMinutes);
            _tokenService.SetRefreshTokenCookie(Response, result.RefreshToken ?? "", refreshTokenExpirationDays);

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Rejestracja nowego u¿ytkownika
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var (deviceId, userAgent, ipAddress) = DeviceInfoHelper.ExtractDeviceInfo(Request, _logger);
            _logger.LogInformation("Registration attempt for user {Username} from device {DeviceId} (IP: {IpAddress})",
                request.Username, deviceId, ipAddress);

            var result = await _authModule.RegisterAsync(request.Username, request.Email, request.Password, deviceId, userAgent, ipAddress);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            // Ustaw tokeny w ciasteczkach
            var accessTokenExpirationMinutes = 60;
            var refreshTokenExpirationDays = 7;

            _tokenService.SetAccessTokenCookie(Response, result.AccessToken, accessTokenExpirationMinutes);
            _tokenService.SetRefreshTokenCookie(Response, result.RefreshToken ?? "", refreshTokenExpirationDays);

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Odnowienie tokenu dostêpu
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request = null)
    {
        // Spróbuj pobraæ refresh token z ciasteczka jeœli nie ma w body
        var refreshToken = request?.RefreshToken ?? Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            var result = await _authModule.RefreshTokenAsync(refreshToken);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            // Ustaw nowe tokeny w ciasteczkach
            var accessTokenExpirationMinutes = 60;
            var refreshTokenExpirationDays = 7;

            _tokenService.SetAccessTokenCookie(Response, result.AccessToken, accessTokenExpirationMinutes);
            _tokenService.SetRefreshTokenCookie(Response, result.RefreshToken ?? "", refreshTokenExpirationDays);

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Walidacja tokenu
    /// </summary>
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest? request = null)
    {
        // Spróbuj pobraæ token z body lub z ciasteczka
        var token = request?.Token ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        try
        {
            var result = await _authModule.ValidateTokenAsync(token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobranie informacji o u¿ytkowniku
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo(string userId)
    {
        try
        {
            var userInfo = await _authModule.GetUserInfoAsync(userId);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for userId: {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userInfo = await _authModule.GetUserInfoAsync(userId);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Wylogowanie - uniewa¿nienie refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            bool success;
            var refreshToken = request?.RefreshToken ?? Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                success = await _authModule.RevokeTokenAsync(refreshToken);
            }
            else
            {
                success = await _authModule.RevokeAllTokensAsync(userId);
            }

            // Usuñ ciasteczka
            _tokenService.RemoveAccessTokenCookie(Response);
            _tokenService.RemoveRefreshTokenCookie(Response);

            return Ok(new { message = success ? "Logged out successfully" : "Logout failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Zmiana has³a u¿ytkownika
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var success = await _authModule.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Please check your current password." });
            }

            // Po zmianie has³a usuñ wszystkie tokeny z ciasteczek dla bezpieczeñstwa
            _tokenService.RemoveAccessTokenCookie(Response);
            _tokenService.RemoveRefreshTokenCookie(Response);

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Aktualizacja profilu u¿ytkownika
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var success = await _authModule.UpdateUserProfileAsync(userId, request.Email, request.Name);

            if (!success)
            {
                return BadRequest(new { message = "Failed to update profile" });
            }

            return Ok(new { message = "Profile updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobranie aktywnych sesji u¿ytkownika
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetUserSessions()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            // Pobierz aktualny refresh token z ciasteczka
            var currentRefreshToken = Request.Cookies["RefreshToken"];

            var sessions = await _authModule.GetUserSessionsAsync(userId, currentRefreshToken);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sessions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var success = await _authModule.RevokeAllTokensAsync(userId);

            _tokenService.RemoveAccessTokenCookie(Response);
            _tokenService.RemoveRefreshTokenCookie(Response);

            return Ok(new { message = success ? "All tokens revoked successfully" : "Failed to revoke tokens" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ValidateTokenRequest
    {
        public string? Token { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}