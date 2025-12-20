namespace inzynierka.Auth.Responses;

public class TokenValidationResult 
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }

    public static TokenValidationResult Failed(string errorMessage)
    {
        return new TokenValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}

