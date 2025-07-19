using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;

namespace Ribosoft.Services
{
    /// <summary>
    /// Service for securely handling email templates without storing sensitive content in cleartext
    /// </summary>
    public interface ISecureEmailTemplateService
    {
        string CreateEmailConfirmationTemplate(string confirmationLink);
        string CreatePasswordResetTemplate(string resetLink);
        string CreateOneTimeCodeEmailTemplate(string code, string purpose);
    }

    public class SecureEmailTemplateService : ISecureEmailTemplateService
    {
        private readonly ILogger<SecureEmailTemplateService> _logger;

        public SecureEmailTemplateService(ILogger<SecureEmailTemplateService> logger)
        {
            _logger = logger;
        }

        public string CreateEmailConfirmationTemplate(string confirmationLink)
        {
            // Validate and sanitize the confirmation link
            if (string.IsNullOrEmpty(confirmationLink))
            {
                _logger.LogWarning("Empty confirmation link provided to email template");
                throw new ArgumentException("Confirmation link cannot be empty", nameof(confirmationLink));
            }

            // Generate template hash for logging (without exposing the actual link)
            var linkHash = GenerateSecureHash(confirmationLink)[..8];
            _logger.LogDebug("Creating email confirmation template with link hash: {LinkHash}", linkHash);

            // Encode the link to prevent XSS
            var encodedLink = HtmlEncoder.Default.Encode(confirmationLink);

            return GenerateEmailConfirmationHtml(encodedLink);
        }

        public string CreatePasswordResetTemplate(string resetLink)
        {
            // Validate and sanitize the reset link
            if (string.IsNullOrEmpty(resetLink))
            {
                _logger.LogWarning("Empty reset link provided to email template");
                throw new ArgumentException("Reset link cannot be empty", nameof(resetLink));
            }

            // Generate template hash for logging (without exposing the actual link)
            var linkHash = GenerateSecureHash(resetLink)[..8];
            _logger.LogDebug("Creating password reset template with link hash: {LinkHash}", linkHash);

            // Encode the link to prevent XSS
            var encodedLink = HtmlEncoder.Default.Encode(resetLink);

            return GeneratePasswordResetHtml(encodedLink);
        }

        public string CreateOneTimeCodeEmailTemplate(string code, string purpose)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Empty code provided to email template");
                throw new ArgumentException("Code cannot be empty", nameof(code));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                purpose = "verification";
            }

            // Generate template hash for logging (without exposing the actual code)
            var codeHash = GenerateSecureHash(code)[..8];
            _logger.LogDebug("Creating one-time code template with code hash: {CodeHash}, purpose: {Purpose}", codeHash, purpose);

            // Encode the code to prevent XSS
            var encodedCode = HtmlEncoder.Default.Encode(code);
            var encodedPurpose = HtmlEncoder.Default.Encode(purpose);

            return GenerateOneTimeCodeHtml(encodedCode, encodedPurpose);
        }

        /// <summary>
        /// Generate a secure hash for logging purposes without exposing sensitive data
        /// </summary>
        private static string GenerateSecureHash(string input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Generate the HTML template for email confirmation
        /// Template is generated dynamically to avoid storing sensitive links in memory
        /// </summary>
        private static string GenerateEmailConfirmationHtml(string encodedConfirmationLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirm Your Ribosoft Account</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Welcome to Ribosoft!</h1>
        <p style='color: #f0f0f0; margin: 10px 0 0 0; font-size: 16px;'>Ribozyme Design Made Simple</p>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Confirm Your Email Address</h2>

        <p>Thank you for creating a Ribosoft account! To complete your registration and start designing ribozymes, please confirm your email address by clicking the button below:</p>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='{encodedConfirmationLink}'
               style='background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block; font-size: 16px;'>
                Confirm Email Address
            </a>
        </div>

        <p style='color: #666; font-size: 14px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='background: #f5f5f5; padding: 10px; border-radius: 5px; word-break: break-all; font-size: 12px; color: #666;'>
            {encodedConfirmationLink}
        </p>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <p style='color: #666; font-size: 14px; margin-bottom: 0;'>
            If you didn't create a Ribosoft account, you can safely ignore this email.
        </p>
    </div>

    <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
        <p>© 2025 Ribosoft - Ribozyme Design Platform</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Generate the HTML template for password reset
        /// Template is generated dynamically to avoid storing sensitive links in memory
        /// </summary>
        private static string GeneratePasswordResetHtml(string encodedResetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Ribosoft Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Password Reset</h1>
        <p style='color: #f0f0f0; margin: 10px 0 0 0; font-size: 16px;'>Ribosoft Account Security</p>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Reset Your Password</h2>

        <p>We received a request to reset the password for your Ribosoft account. If you made this request, click the button below to create a new password:</p>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='{encodedResetLink}'
               style='background: #dc3545; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block; font-size: 16px;'>
                Reset Password
            </a>
        </div>

        <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404; font-size: 14px;'>
                <strong>Security Notice:</strong> This password reset link will expire in 24 hours for your security.
            </p>
        </div>

        <p style='color: #666; font-size: 14px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='background: #f5f5f5; padding: 10px; border-radius: 5px; word-break: break-all; font-size: 12px; color: #666;'>
            {encodedResetLink}
        </p>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <div style='background: #f8f9fa; border-radius: 5px; padding: 15px;'>
            <p style='color: #666; font-size: 14px; margin: 0;'>
                <strong>Didn't request this?</strong> If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
            </p>
        </div>
    </div>

    <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
        <p>© 2025 Ribosoft - Ribozyme Design Platform</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Generate the HTML template for one-time code
        /// Template is generated dynamically to avoid storing sensitive codes in memory
        /// </summary>
        private static string GenerateOneTimeCodeHtml(string encodedCode, string encodedPurpose)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Ribosoft Verification Code</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Verification Code</h1>
        <p style='color: #f0f0f0; margin: 10px 0 0 0; font-size: 16px;'>Ribosoft Security</p>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Your {encodedPurpose} Code</h2>

        <p>Use the following verification code to complete your request:</p>

        <div style='text-align: center; margin: 30px 0;'>
            <div style='background: #f8f9fa; border: 2px solid #667eea; border-radius: 10px; padding: 20px; display: inline-block;'>
                <span style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 4px; font-family: monospace;'>
                    {encodedCode}
                </span>
            </div>
        </div>

        <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404; font-size: 14px;'>
                <strong>Security Notice:</strong> This verification code will expire in 15 minutes for your security.
            </p>
        </div>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <div style='background: #f8f9fa; border-radius: 5px; padding: 15px;'>
            <p style='color: #666; font-size: 14px; margin: 0;'>
                <strong>Didn't request this?</strong> If you didn't request this verification code, you can safely ignore this email.
            </p>
        </div>
    </div>

    <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
        <p>© 2025 Ribosoft - Ribozyme Design Platform</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
        }
    }
}
