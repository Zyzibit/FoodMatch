namespace foodmatch.Auth.Services;
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
}



