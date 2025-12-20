using System.Net;
using System.Net.Mail;
using System.Web;

namespace inzynierka.Auth.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
        _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
        _fromName = _configuration["Email:FromName"] ?? "FoodMatch";
        _frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
    {
        try
        {
            var encodedToken = HttpUtility.UrlEncode(resetToken);
            var encodedEmail = HttpUtility.UrlEncode(email);
            
            var resetLink = $"{_frontendUrl}/reset-password?token={encodedToken}&email={encodedEmail}";

            var subject = "Resetowanie hasła - FoodMatch";
var body = $@"
<!DOCTYPE html>
<html lang='pl'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Resetowanie hasła DIET ZYNZI</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Inter', 'Helvetica Neue', Helvetica, Arial, sans-serif;
            line-height: 1.5;
            color: #1f2937; 
            background-color: #f8f8f4;
            -webkit-text-size-adjust: 100%;
            -ms-text-size-adjust: 100%;
        }}
        .container {{
            max-width: 550px; 
            margin: 30px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.08); 
            overflow: hidden;
        }}
        .header {{
            background-color: #4F7972; 
            color: white;
            padding: 20px 25px 30px; 
            text-align: center;
        }}
        .header h1 {{
            margin: 10px 0 0 0; 
            font-size: 22px; 
            font-weight: 700;
        }}
        .header .logo {{
            max-width: 60px;
            height: auto;
            display: block;
            margin: 0 auto;
        }}
        .content {{
            padding: 30px 35px;
        }}
        .content p {{
            margin-bottom: 20px;
            font-size: 16px;
            color: #374151;
        }}
        .button-area {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 15px 40px;
            background-color: #5cb85c;
            color: #ffffff;
            text-decoration: none;
            border-radius: 8px; 
            font-weight: 600;
            font-size: 17px;
            box-shadow: 0 4px 6px rgba(92, 184, 92, 0.4);
        }}
        .token-box {{
            background-color: #f3f5f6;
            padding: 15px;
            margin: 20px 0;
            border-radius: 8px;
            border: 1px solid #e5e7eb;
            word-break: break-all;
            font-size: 14px;
        }}
        .token-box a, .token-box code {{
            color: #1f2937;
            text-decoration: none;
            font-family: monospace;
        }}
        .info-list {{
            list-style-type: none;
            padding-left: 0;
            margin-top: 25px;
            border-top: 1px solid #f3f4f6;
            padding-top: 20px;
        }}
        .info-list li {{
            margin-bottom: 10px;
            font-size: 14px;
            color: #6b7280;
        }}
        .info-list li::before {{
            content: '•';
            color: #5cb85c;
            font-weight: bold;
            display: inline-block;
            width: 1em;
            margin-left: -1em;
        }}
        .footer {{
            text-align: center;
            padding: 25px;
            color: #9ca3af;
            font-size: 12px;
            background-color: #f7f9fb;
            border-top: 1px solid #f3f4f6;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://i.imgur.com/Hkbw9Zb.png' alt='Logo DIET ZYNZI' class='logo' width='60' height='60' style='display: block; margin: 0 auto; border-radius: 10px;'>
            <h1>🔐 Resetowanie Hasła</h1>
        </div>
        <div class='content'>
            <p>Witaj, <strong>{HttpUtility.HtmlEncode(userName)}</strong>!</p>
            
            <p>Aby ukończyć proces resetowania hasła do Twojego konta **DIET ZYNZI**, prosimy o kliknięcie poniższego przycisku.
            </p>
            
            <div class='button-area'>
                <a href='{resetLink}' class='button'>Resetuj Hasło</a>
            </div>
            
            <p style='font-size: 14px; text-align: center; color: #9ca3af; margin-bottom: 30px;'>
                Przyciski nie działają? Użyj poniższego linku:
            </p>

            <div class='token-box'>
                <a href='{resetLink}'>{resetLink}</a>
            </div>
            
            <p style='font-weight: 600; margin-top: 30px;'>Ważne:</p>
            <ul class='info-list'>
                <li>Ten link resetujący jest ważny tylko przez <strong>24 godziny</strong>.</li>
                <li>Jeśli nie prosiłeś o reset hasła, <strong>natychmiast zignoruj</strong> tę wiadomość.</li>
                <li>Po zmianie hasła, wszystkie aktywne sesje zostaną wylogowane.</li>
            </ul>
        </div>
        <div class='footer'>
            <p>Jeśli masz jakiekolwiek pytania, skontaktuj się z naszym wsparciem.</p>
            <p>© 2024 DIET ZYNZI. Wszystkie prawa zastrzeżone.</p>
            <p>To jest automatyczna wiadomość - prosimy nie odpowiadać.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            throw new Exception("Error sending password reset email", ex);
        }
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                throw new Exception("SMTP username or password is not configured.");
            }

            using var message = new MailMessage();
            message.From = new MailAddress(_fromEmail, _fromName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.Priority = MailPriority.High;

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false; 
            smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 30000; 

            await smtpClient.SendMailAsync(message);
            
            return true;
        }
        catch (SmtpException ex)
        {
            throw new Exception("SMTP error sending email", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Error sending email", ex);
        }
    }
}


