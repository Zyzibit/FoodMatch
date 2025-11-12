namespace inzynierka.Auth.Responses;

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthenticationResult Failed(string errorMessage)
    {
        return new AuthenticationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}