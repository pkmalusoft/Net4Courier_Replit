using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Net4Courier.Web.Services;

public interface IGmailEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string fromEmail, string fromName);
    Task<bool> SendEmailWithAttachmentAsync(string to, string toName, string subject, string htmlBody,
        byte[] attachmentBytes, string fileName, string contentType, string fromEmail, string fromName);
}

public class GmailEmailService : IGmailEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailEmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GmailEmailService(
        IConfiguration configuration,
        ILogger<GmailEmailService> logger,
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

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string fromEmail, string fromName)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Could not obtain Gmail access token");
                return false;
            }

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

    public async Task<bool> SendEmailWithAttachmentAsync(string to, string toName, string subject, string htmlBody,
        byte[] attachmentBytes, string fileName, string contentType, string fromEmail, string fromName)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Could not obtain Gmail access token for attachment email");
                return false;
            }

            var base64Content = Convert.ToBase64String(attachmentBytes);
            var rawMessage = CreateRawEmailWithAttachment(fromEmail, fromName, to, subject, htmlBody, base64Content, fileName, contentType);
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
