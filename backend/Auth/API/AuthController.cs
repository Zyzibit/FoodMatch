using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inzynierka.Auth.Services;
using inzynierka.Auth.Utilities;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using inzynierka.Auth.Requests;
using inzynierka.Auth.Responses;
using inzynierka.Users.Model;

namespace inzynierka.Auth.API;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;

    public AuthController(
        IConfiguration configuration,
        IAuthService authService, 
        ITokenService tokenService,
        UserManager<User> userManager,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
        _userManager = userManager;
        _logger = logger;
    }


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

            var result = await _authService.AuthenticateAsync(request.Username, request.Password, deviceId, userAgent, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            var accessTokenExpirationMinutes = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");
            var refreshTokenExpirationDays = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");
            
            
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

            var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password, deviceId, userAgent, ipAddress);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            var accessTokenExpirationMinutes = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");
            var refreshTokenExpirationDays = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");

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
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request = null)
    {
        var refreshToken = request?.RefreshToken ?? Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            var accessTokenExpirationMinutes = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");
            var refreshTokenExpirationDays = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");

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


    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken()
    {
        var token = Request.Cookies["AccessToken"] ?? null;

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        try
        {
            var result = await _authService.ValidateTokenAsync(token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo(string userId)
    {       
        var token = Request.Cookies["AccessToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userInfo = new UserInfo
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                CreatedAt = DateTime.UtcNow,
            };

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
        var token = Request.Cookies["RefreshToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userInfo = new UserInfo
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
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
                success = await _authService.RevokeTokenAsync(refreshToken);
            }
            else
            {
                success = await _authService.RevokeAllTokensAsync(userId);
            }

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

            var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Please check your current password." });
            }

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

            var currentRefreshToken = Request.Cookies["RefreshToken"];

            var sessions = await _authService.GetUserSessionsAsync(userId, currentRefreshToken);
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

            var success = await _authService.RevokeAllTokensAsync(userId);

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
}