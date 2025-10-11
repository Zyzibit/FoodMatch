namespace inzynierka.Auth.Contracts.Models
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserInfo? User { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
