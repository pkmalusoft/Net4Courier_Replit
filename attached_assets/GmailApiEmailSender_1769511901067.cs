using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Truebooks.Platform.Host.Services.Email;

public class GmailApiEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailApiEmailSender> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GmailApiEmailSender(
        IConfiguration configuration, 
        ILogger<GmailApiEmailSender> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var hostname = Environment.GetEnvironmentVariable("REPLIT_CONNECTORS_HOSTNAME");
            var replIdentity = Environment.GetEnvironmentVariable("REPL_IDENTITY");
            var webReplRenewal = Environment.GetEnvironmentVariable("WEB_REPL_RENEWAL");

            string? xReplitToken = null;
            if (!string.IsNullOrEmpty(replIdentity))
            {
                xReplitToken = $"repl {replIdentity}";
            }
            else if (!string.IsNullOrEmpty(webReplRenewal))
            {
                xReplitToken = $"depl {webReplRenewal}";
            }

            if (string.IsNullOrEmpty(hostname) || string.IsNullOrEmpty(xReplitToken))
            {
                _logger.LogWarning("Replit connector environment variables not found. Hostname: {Hostname}, Token: {TokenSet}", 
                    hostname ?? "null", !string.IsNullOrEmpty(xReplitToken));
                return null;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("X_REPLIT_TOKEN", xReplitToken);

            var response = await client.GetAsync($"https://{hostname}/api/v2/connection?include_secrets=true&connector_names=google-mail");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Gmail connection: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var connectionResponse = JsonSerializer.Deserialize<ConnectionResponse>(content);

            var accessToken = connectionResponse?.Items?.FirstOrDefault()?.Settings?.AccessToken 
                ?? connectionResponse?.Items?.FirstOrDefault()?.Settings?.OAuth?.Credentials?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Gmail connection found but no access token available");
                return null;
            }

            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Gmail access token");
            return null;
        }
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Could not obtain Gmail access token");
                return false;
            }

            var fromEmail = _configuration["Email:FromAddress"] ?? "info@truebooksolutions.com";
            var fromName = _configuration["Email:FromName"] ?? "Truebooks ERP";

            var rawMessage = CreateRawEmail(fromEmail, fromName, to, subject, htmlBody);
            var encodedMessage = Base64UrlEncode(rawMessage);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var requestContent = new StringContent(
                JsonSerializer.Serialize(new { raw = encodedMessage }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                "https://gmail.googleapis.com/gmail/v1/users/me/messages/send",
                requestContent
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email} via Gmail API", to);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gmail API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Gmail API", to);
            return false;
        }
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string tenantName, string verificationLink)
    {
        var subject = "Verify your email - Truebooks ERP";
        var htmlBody = GetVerificationEmailHtml(tenantName, verificationLink);
        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string tenantName, string resetLink)
    {
        var subject = "Reset your password - Truebooks ERP";
        var htmlBody = GetPasswordResetEmailHtml(tenantName, resetLink);
        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendVerificationSuccessEmailAsync(string toEmail, string tenantName, string userEmail, string defaultPassword, string loginUrl)
    {
        var subject = "Welcome to Truebooks ERP - Your Account is Ready!";
        var htmlBody = GetVerificationSuccessEmailHtml(tenantName, userEmail, defaultPassword, loginUrl);
        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendEmailWithAttachmentAsync(string to, string toName, string subject, string htmlBody,
        string base64Content, string fileName, string contentType, string fromEmail, string fromName)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Could not obtain Gmail access token for attachment email");
                return false;
            }

            var rawMessage = CreateRawEmailWithAttachment(fromEmail, fromName, to, subject, htmlBody, base64Content, fileName, contentType);
            var encodedMessage = Base64UrlEncode(rawMessage);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var requestContent = new StringContent(
                JsonSerializer.Serialize(new { raw = encodedMessage }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                "https://gmail.googleapis.com/gmail/v1/users/me/messages/send",
                requestContent
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email with attachment sent successfully to {Email} via Gmail API", to);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gmail API error sending attachment email: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachment to {Email} via Gmail API", to);
            return false;
        }
    }

    private string CreateRawEmail(string fromEmail, string fromName, string to, string subject, string htmlBody)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"From: {fromName} <{fromEmail}>");
        sb.AppendLine($"To: {to}");
        sb.AppendLine($"Subject: {subject}");
        sb.AppendLine("MIME-Version: 1.0");
        sb.AppendLine("Content-Type: text/html; charset=utf-8");
        sb.AppendLine();
        sb.Append(htmlBody);
        return sb.ToString();
    }

    private string CreateRawEmailWithAttachment(string fromEmail, string fromName, string to, string subject, 
        string htmlBody, string base64Content, string fileName, string contentType)
    {
        var boundary = $"boundary_{Guid.NewGuid():N}";
        var sb = new StringBuilder();
        
        sb.AppendLine($"From: {fromName} <{fromEmail}>");
        sb.AppendLine($"To: {to}");
        sb.AppendLine($"Subject: {subject}");
        sb.AppendLine("MIME-Version: 1.0");
        sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
        sb.AppendLine();
        
        sb.AppendLine($"--{boundary}");
        sb.AppendLine("Content-Type: text/html; charset=utf-8");
        sb.AppendLine();
        sb.AppendLine(htmlBody);
        sb.AppendLine();
        
        sb.AppendLine($"--{boundary}");
        sb.AppendLine($"Content-Type: {contentType}; name=\"{fileName}\"");
        sb.AppendLine("Content-Transfer-Encoding: base64");
        sb.AppendLine($"Content-Disposition: attachment; filename=\"{fileName}\"");
        sb.AppendLine();
        sb.AppendLine(base64Content);
        sb.AppendLine();
        
        sb.AppendLine($"--{boundary}--");
        
        return sb.ToString();
    }

    private string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    private string GetVerificationEmailHtml(string tenantName, string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f5f5f5; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1a237e 0%, #0d47a1 100%); padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">Truebooks ERP</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""color: #1a237e; margin: 0 0 20px 0; font-size: 24px;"">Verify Your Email</h2>
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;"">
                                Welcome to Truebooks ERP! You've registered <strong>{tenantName}</strong> as your organization.
                            </p>
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;"">
                                Please click the button below to verify your email address and activate your account:
                            </p>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{verificationLink}"" style=""display: inline-block; background: linear-gradient(135deg, #1a237e 0%, #0d47a1 100%); color: #ffffff; text-decoration: none; padding: 16px 40px; border-radius: 8px; font-size: 16px; font-weight: 600;"">Verify Email Address</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                This link will expire in 24 hours. If you didn't create this account, you can safely ignore this email.
                            </p>
                            <hr style=""border: none; border-top: 1px solid #eeeeee; margin: 30px 0;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.6; margin: 0;"">
                                If the button doesn't work, copy and paste this link into your browser:<br>
                                <a href=""{verificationLink}"" style=""color: #0d47a1; word-break: break-all;"">{verificationLink}</a>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 40px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                &copy; 2026 Truebooks Solutions. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GetVerificationSuccessEmailHtml(string tenantName, string userEmail, string defaultPassword, string loginUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f5f5f5; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%); padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">Truebooks ERP</h1>
                            <p style=""color: #ffffff; margin: 10px 0 0 0; font-size: 16px;"">Email Verified Successfully!</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""color: #27ae60; margin: 0 0 20px 0; font-size: 24px;"">Welcome to Truebooks ERP!</h2>
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;"">
                                Your email has been verified and your account is now active. Here are your login credentials:
                            </p>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8f9fa; border-radius: 8px; margin: 20px 0;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px; width: 120px;"">Tenant Name:</td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 16px; font-weight: 600;"">{tenantName}</td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px;"">User Email:</td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 16px; font-weight: 600;"">{userEmail}</td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px;"">Password:</td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 16px; font-weight: 600;"">{defaultPassword}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #e74c3c; font-size: 14px; line-height: 1.6; margin: 0 0 20px 0;"">
                                <strong>Important:</strong> Please change your password after your first login for security purposes.
                            </p>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{loginUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%); color: #ffffff; text-decoration: none; padding: 16px 40px; border-radius: 8px; font-size: 16px; font-weight: 600;"">Login Now</a>
                                    </td>
                                </tr>
                            </table>
                            <hr style=""border: none; border-top: 1px solid #eeeeee; margin: 30px 0;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.6; margin: 0;"">
                                If the button doesn't work, copy and paste this link into your browser:<br>
                                <a href=""{loginUrl}"" style=""color: #27ae60; word-break: break-all;"">{loginUrl}</a>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 40px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                &copy; 2026 Truebooks Solutions. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GetPasswordResetEmailHtml(string tenantName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f5f5f5; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1a237e 0%, #0d47a1 100%); padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">Truebooks ERP</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""color: #1a237e; margin: 0 0 20px 0; font-size: 24px;"">Reset Your Password</h2>
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;"">
                                We received a request to reset the password for your account at <strong>{tenantName}</strong>.
                            </p>
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;"">
                                Click the button below to set a new password:
                            </p>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{resetLink}"" style=""display: inline-block; background: linear-gradient(135deg, #1a237e 0%, #0d47a1 100%); color: #ffffff; text-decoration: none; padding: 16px 40px; border-radius: 8px; font-size: 16px; font-weight: 600;"">Reset Password</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                This link will expire in 1 hour. If you didn't request a password reset, you can safely ignore this email.
                            </p>
                            <hr style=""border: none; border-top: 1px solid #eeeeee; margin: 30px 0;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.6; margin: 0;"">
                                If the button doesn't work, copy and paste this link into your browser:<br>
                                <a href=""{resetLink}"" style=""color: #0d47a1; word-break: break-all;"">{resetLink}</a>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 40px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                &copy; 2026 Truebooks Solutions. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private class ConnectionResponse
    {
        [JsonPropertyName("items")]
        public List<ConnectionItem>? Items { get; set; }
    }

    private class ConnectionItem
    {
        [JsonPropertyName("settings")]
        public ConnectionSettings? Settings { get; set; }
    }

    private class ConnectionSettings
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("oauth")]
        public OAuthSettings? OAuth { get; set; }
    }

    private class OAuthSettings
    {
        [JsonPropertyName("credentials")]
        public OAuthCredentials? Credentials { get; set; }
    }

    private class OAuthCredentials
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
