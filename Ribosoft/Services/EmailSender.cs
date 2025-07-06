using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Ribosoft.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly SendGridClient? _sendGridClient;
        private readonly string _senderEmail = string.Empty;

        public EmailSender(IConfiguration configuration)
        {
            if (configuration["SendGridAPIKey"] == null)
            {
                return;
            }

            _sendGridClient = new SendGridClient(configuration["SendGridAPIKey"]);
            _senderEmail = configuration["SenderEmail"] ?? string.Empty;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            if (_sendGridClient == null)
            {
                return Task.CompletedTask;
            }

            var from = new EmailAddress(_senderEmail, "Ribosoft");
            var to = new EmailAddress(email);
            var plainTextContent = Regex.Replace(message, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, message);
            return _sendGridClient.SendEmailAsync(msg);
        }
    }
}
