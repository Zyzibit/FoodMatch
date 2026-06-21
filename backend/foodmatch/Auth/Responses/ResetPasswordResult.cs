namespace inzynierka.Auth.Responses;

public class ResetPasswordResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static ResetPasswordResult Succeeded()
    {
        return new ResetPasswordResult
        {
            Success = true
        };
    }

    public static ResetPasswordResult Failed(string errorMessage)
    {
        return new ResetPasswordResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

