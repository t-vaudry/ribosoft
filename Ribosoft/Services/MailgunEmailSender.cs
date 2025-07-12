using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

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

            _logger.LogInformation("MailgunEmailSender configured successfully. Domain: {Domain}, Sender: {SenderName} <{SenderEmail}>",
                _domain, _senderName, _senderEmail);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Attempting to send email to {Email} with subject: {Subject}", email, subject);

            if (!_isConfigured || _client == null)
            {
                _logger.LogWarning("Email sending skipped - Mailgun not configured. To: {Email}, Subject: {Subject}", email, subject);
                _logger.LogDebug("Email content that would have been sent: {EmailContent}", htmlMessage);

                // Also log to console for immediate visibility during development
                Console.WriteLine($"[EMAIL SKIPPED] To: {email}, Subject: {subject}");
                Console.WriteLine($"[EMAIL CONTENT] {htmlMessage}");
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
                request.AddParameter("o:tag", "password-reset");
                request.AddParameter("o:tag", "ribosoft");

                _logger.LogDebug("Sending email via Mailgun API. From: {From}, To: {To}, Tags: password-reset,ribosoft",
                    $"{_senderName} <{_senderEmail}>", email);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Email sent successfully to {Email} via Mailgun. Response: {StatusCode}",
                        email, response.StatusCode);

                    // Parse Mailgun response for message ID if available
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        _logger.LogDebug("Mailgun response: {Response}", response.Content);
                    }

                    // Also log to console for immediate visibility
                    Console.WriteLine($"[EMAIL SENT] Successfully sent email to {email} via Mailgun");
                }
                else
                {
                    _logger.LogError("Failed to send email to {Email} via Mailgun. Status: {StatusCode}, Response: {Response}",
                        email, response.StatusCode, response.Content);

                    Console.WriteLine($"[EMAIL ERROR] Failed to send email to {email}: {response.StatusCode} - {response.Content}");

                    throw new Exception($"Failed to send email via Mailgun: {response.StatusCode} - {response.Content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {Email}", email);

                // Also log to console for immediate visibility
                Console.WriteLine($"[EMAIL ERROR] Failed to send email to {email}: {ex.Message}");

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
