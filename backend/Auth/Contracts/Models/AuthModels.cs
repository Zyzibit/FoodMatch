namespace inzynierka.Auth.Contracts.Models;

public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TokenRefreshResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}