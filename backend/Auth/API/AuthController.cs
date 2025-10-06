using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inzynierka.Auth.Contracts;
using inzynierka.Auth.Contracts.Models;

namespace inzynierka.Auth.API;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthContract _authModule;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthContract authModule, ILogger<AuthController> logger)
    {
        _authModule = authModule;
        _logger = logger;
    }

    /// <summary>
    /// Logowanie u퓓tkownika
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
            var result = await _authModule.AuthenticateAsync(request.Username, request.Password);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
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
    /// Rejestracja nowego u퓓tkownika
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
            var result = await _authModule.RegisterAsync(request.Username, request.Email, request.Password);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
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
    /// Walidacja tokenu
    /// </summary>
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        try
        {
            var result = await _authModule.ValidateTokenAsync(request.Token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobranie informacji o u퓓tkowniku
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

    /// <summary>
    /// Pobranie informacji o aktualnie zalogowanym u퓓tkowniku
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst("nameid")?.Value;
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
    /// Wylogowanie (placeholder)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Placeholder dla funkcjonalno쐁i wylogowania
        await Task.CompletedTask;
        return Ok(new { message = "Logged out successfully" });
    }
}

// DTOs dla API
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
    public string Token { get; set; } = string.Empty;
}