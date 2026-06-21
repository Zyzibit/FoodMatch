namespace inzynierka.Auth.Responses;

public class TokenRefreshResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }

    public static TokenRefreshResult Failed(string errorMessage)
    {
        return new TokenRefreshResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

