namespace inzynierka.Auth.Responses;

public class ForgotPasswordResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }

    public static ForgotPasswordResult Succeeded(string token)
    {
        return new ForgotPasswordResult
        {
            Success = true,
            Token = token
        };
    }

    public static ForgotPasswordResult Failed(string errorMessage)
    {
        return new ForgotPasswordResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

