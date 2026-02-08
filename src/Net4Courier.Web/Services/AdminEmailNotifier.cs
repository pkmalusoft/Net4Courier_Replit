using Net4Courier.Infrastructure.Services;

namespace Net4Courier.Web.Services;

public class AdminEmailNotifier : IAdminEmailNotifier
{
    private readonly IGmailEmailService _emailService;
    private readonly ILogger<AdminEmailNotifier> _logger;
    private readonly IConfiguration _configuration;

    public AdminEmailNotifier(
        IGmailEmailService emailService, 
        ILogger<AdminEmailNotifier> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendAdminCredentialsEmailAsync(string toEmail, string fullName, string username, string password, string loginUrl)
    {
        try
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@net4courier.com";
            var fromName = _configuration["EmailSettings:FromName"] ?? "Net4Courier System";
            var subject = "Your Net4Courier Administrator Account Credentials";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #1a237e 0%, #0d47a1 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border: 1px solid #e0e0e0; }}
        .credentials {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #1976d2; }}
        .credentials p {{ margin: 8px 0; }}
        .label {{ color: #666; font-size: 14px; }}
        .value {{ font-weight: bold; color: #1a237e; font-size: 16px; }}
        .button {{ display: inline-block; background: #1976d2; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning {{ background: #fff3e0; border-left: 4px solid #ff9800; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1 style='margin: 0;'>Net4Courier</h1>
        <p style='margin: 10px 0 0 0; opacity: 0.9;'>Administrator Account Created</p>
    </div>
    <div class='content'>
        <p>Dear <strong>{fullName}</strong>,</p>
        <p>Your administrator account has been successfully created for the Net4Courier system. Below are your login credentials:</p>
        
        <div class='credentials'>
            <p><span class='label'>Username:</span><br><span class='value'>{username}</span></p>
            <p><span class='label'>Password:</span><br><span class='value'>{password}</span></p>
            <p><span class='label'>Login URL:</span><br><span class='value'>{loginUrl}</span></p>
        </div>
        
        <div class='warning'>
            <strong>Security Notice:</strong> Please change your password after your first login for security purposes. Do not share these credentials with anyone.
        </div>
        
        <center>
            <a href='{loginUrl}' class='button'>Login to Net4Courier</a>
        </center>
    </div>
    <div class='footer'>
        <p>This is an automated message from Net4Courier. Please do not reply to this email.</p>
        <p>&copy; {DateTime.UtcNow.Year} Net4Courier. All rights reserved.</p>
    </div>
</body>
</html>";

            var result = await _emailService.SendEmailAsync(toEmail, subject, htmlBody, fromEmail, fromName);
            
            if (result)
            {
                _logger.LogInformation("Admin credentials email sent successfully to {Email}", toEmail);
            }
            else
            {
                _logger.LogWarning("Failed to send admin credentials email to {Email}", toEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin credentials email to {Email}", toEmail);
            return false;
        }
    }
}
