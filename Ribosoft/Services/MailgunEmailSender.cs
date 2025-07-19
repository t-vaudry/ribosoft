using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using System.Security.Cryptography;

namespace Ribosoft.Services
{
    /// <summary>
    /// Email sender implementation using Mailgun API
    /// </summary>
    public class MailgunEmailSender : IEmailSender
    {
        private readonly RestClient? _client;
        private readonly string _domain = string.Empty;
        private readonly string _senderEmail = string.Empty;
        private readonly string _senderName = string.Empty;
        private readonly ILogger<MailgunEmailSender> _logger;
        private readonly bool _isConfigured;

        public MailgunEmailSender(IConfiguration configuration, ILogger<MailgunEmailSender> logger)
        {
            _logger = logger;

            var apiKey = configuration["MailgunAPIKey"];
            var domain = configuration["MailgunDomain"];
            var senderEmail = configuration["SenderEmail"];
            var senderName = configuration["SenderName"] ?? "Ribosoft";

            _logger.LogInformation("Initializing MailgunEmailSender...");
            _logger.LogDebug("Configuration check - APIKey: {HasApiKey}, Domain: {HasDomain}, SenderEmail: {HasSenderEmail}",
                !string.IsNullOrEmpty(apiKey), !string.IsNullOrEmpty(domain), !string.IsNullOrEmpty(senderEmail));

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(senderEmail))
            {
                _logger.LogWarning("Mailgun configuration incomplete. Email sending will be disabled. Missing: {MissingConfig}",
                    string.Join(", ", new[]
                    {
                        string.IsNullOrEmpty(apiKey) ? "MailgunAPIKey" : null,
                        string.IsNullOrEmpty(domain) ? "MailgunDomain" : null,
                        string.IsNullOrEmpty(senderEmail) ? "SenderEmail" : null
                    }.Where(x => x != null)));

                _isConfigured = false;
                return;
            }

            _domain = domain;
            _senderEmail = senderEmail;
            _senderName = senderName;
            _isConfigured = true;

            // Initialize RestSharp client with Mailgun API endpoint
            var options = new RestClientOptions($"https://api.mailgun.net/v3/{domain}")
            {
                Authenticator = new HttpBasicAuthenticator("api", apiKey)
            };

            _client = new RestClient(options);

            _logger.LogInformation("MailgunEmailSender configured successfully. Domain: {Domain}, Sender: {SenderName}",
                _domain, _senderName);
        }

        /// <summary>
        /// Generate a secure hash of the email content for logging/debugging purposes
        /// </summary>
        private static string GenerateContentHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "[EMPTY_CONTENT]";

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(hash)[..8]; // First 8 characters for brevity
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Generate a unique operation ID for this email send operation (not derived from sensitive data)
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var contentHash = GenerateContentHash(htmlMessage);

            _logger.LogInformation("Attempting to send email operation {OperationId} (ContentHash: {ContentHash})", 
                operationId, contentHash);

            if (!_isConfigured || _client == null)
            {
                _logger.LogWarning("Email sending skipped - Mailgun not configured. Operation: {OperationId}", 
                    operationId);
                
                // Log content hash instead of full content for debugging
                _logger.LogDebug("Email content hash that would have been sent: {ContentHash}", contentHash);

                // Also log to console for immediate visibility during development (no sensitive data)
                Console.WriteLine($"[EMAIL SKIPPED] Operation: {operationId}");
                Console.WriteLine($"[EMAIL CONTENT HASH] {contentHash}");
                return;
            }

            try
            {
                var request = new RestRequest("messages", Method.Post);

                // Add form parameters for Mailgun API
                request.AddParameter("from", $"{_senderName} <{_senderEmail}>");
                request.AddParameter("to", email);
                request.AddParameter("subject", subject);
                request.AddParameter("html", htmlMessage);

                // Also include plain text version
                var plainTextMessage = ConvertHtmlToPlainText(htmlMessage);
                request.AddParameter("text", plainTextMessage);

                // Optional: Add tags for tracking
                request.AddParameter("o:tag", "ribosoft-email");
                
                // Add specific tags based on subject content
                if (subject.Contains("password", StringComparison.OrdinalIgnoreCase))
                    request.AddParameter("o:tag", "password-reset");
                else if (subject.Contains("confirm", StringComparison.OrdinalIgnoreCase))
                    request.AddParameter("o:tag", "account-confirmation");
                else if (subject.Contains("code", StringComparison.OrdinalIgnoreCase))
                    request.AddParameter("o:tag", "verification-code");

                _logger.LogDebug("Sending email via Mailgun API. Operation: {OperationId}, ContentHash: {ContentHash}",
                    operationId, contentHash);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Email sent successfully for operation {OperationId} via Mailgun. Response: {StatusCode}",
                        operationId, response.StatusCode);

                    // Parse Mailgun response for message ID if available (don't log full response as it may contain sensitive data)
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        // Only log the response hash for debugging, not the full content
                        var responseHash = GenerateContentHash(response.Content);
                        _logger.LogDebug("Mailgun response hash: {ResponseHash}", responseHash);
                    }

                    // Also log to console for immediate visibility (no sensitive data)
                    Console.WriteLine($"[EMAIL SENT] Successfully sent email for operation {operationId} via Mailgun");
                }
                else
                {
                    // Log error without exposing sensitive response content
                    _logger.LogError("Failed to send email for operation {OperationId} via Mailgun. Status: {StatusCode}",
                        operationId, response.StatusCode);

                    Console.WriteLine($"[EMAIL ERROR] Failed to send email for operation {operationId}: {response.StatusCode}");

                    throw new Exception($"Failed to send email via Mailgun: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email for operation {OperationId}", operationId);

                // Also log to console for immediate visibility (no sensitive data)
                Console.WriteLine($"[EMAIL ERROR] Failed to send email for operation {operationId}: {ex.Message}");

                // Optionally, you could throw the exception if you want the forgot password flow to fail
                // For now, we'll just log it to avoid breaking the user experience
                // throw;
            }
        }

        /// <summary>
        /// Convert HTML content to plain text for email clients that don't support HTML
        /// </summary>
        private static string ConvertHtmlToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Remove HTML tags
            var plainText = Regex.Replace(html, "<[^>]*>", string.Empty);

            // Decode HTML entities
            plainText = System.Net.WebUtility.HtmlDecode(plainText);

            // Clean up whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

            return plainText;
        }
    }
}
