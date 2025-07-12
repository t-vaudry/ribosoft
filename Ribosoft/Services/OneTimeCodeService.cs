using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Extensions;

namespace Ribosoft.Services
{
    /*! \interface IOneTimeCodeService
     * \brief Interface for one-time code service
     */
    public interface IOneTimeCodeService
    {
        Task<string> GenerateAndSendCodeAsync(string userId, string email, string purpose);
        Task<bool> ValidateCodeAsync(string userId, string code, string purpose);
        Task CleanupExpiredCodesAsync();
    }

    /*! \class OneTimeCodeService
     * \brief Service for managing one-time authentication codes
     */
    public class OneTimeCodeService : IOneTimeCodeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<OneTimeCodeService> _logger;

        public OneTimeCodeService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<OneTimeCodeService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        /*! \method GenerateAndSendCodeAsync
         * \brief Generate a new one-time code and send it via email
         * \param userId User ID
         * \param email Email address to send to
         * \param purpose Purpose of the code
         * \return The generated code (for testing purposes)
         */
        public async Task<string> GenerateAndSendCodeAsync(string userId, string email, string purpose)
        {
            // Clean up any existing codes for this user and purpose
            await InvalidateExistingCodesAsync(userId, purpose);

            // Generate a secure 6-digit code
            var code = GenerateSecureCode();

            // Create the one-time code record
            var oneTimeCode = new OneTimeCode
            {
                UserId = userId,
                Code = code,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10-minute expiration
                Purpose = purpose
            };

            _context.OneTimeCodes.Add(oneTimeCode);
            await _context.SaveChangesAsync();

            // Send the email
            await SendCodeEmailAsync(email, code, purpose);

            _logger.LogInformation("One-time code generated and sent for user {UserId} with purpose {Purpose}", userId, purpose);

            return code; // Return for testing purposes only
        }

        /*! \method ValidateCodeAsync
         * \brief Validate a one-time code
         * \param userId User ID
         * \param code Code to validate
         * \param purpose Purpose of the code
         * \return True if valid, false otherwise
         */
        public async Task<bool> ValidateCodeAsync(string userId, string code, string purpose)
        {
            var oneTimeCode = await _context.OneTimeCodes
                .FirstOrDefaultAsync(c => 
                    c.UserId == userId && 
                    c.Code == code && 
                    c.Purpose == purpose &&
                    !c.IsUsed &&
                    c.ExpiresAt > DateTime.UtcNow);

            if (oneTimeCode == null)
            {
                _logger.LogWarning("Invalid or expired one-time code attempted for user {UserId} with purpose {Purpose}", userId, purpose);
                return false;
            }

            // Mark as used
            oneTimeCode.MarkAsUsed();
            await _context.SaveChangesAsync();

            _logger.LogInformation("One-time code successfully validated for user {UserId} with purpose {Purpose}", userId, purpose);
            return true;
        }

        /*! \method CleanupExpiredCodesAsync
         * \brief Remove expired codes from the database
         */
        public async Task CleanupExpiredCodesAsync()
        {
            var expiredCodes = await _context.OneTimeCodes
                .Where(c => c.ExpiresAt <= DateTime.UtcNow || c.IsUsed)
                .ToListAsync();

            if (expiredCodes.Any())
            {
                _context.OneTimeCodes.RemoveRange(expiredCodes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired one-time codes", expiredCodes.Count);
            }
        }

        /*! \method GenerateSecureCode
         * \brief Generate a cryptographically secure 6-digit code
         * \return 6-digit numeric code
         */
        private static string GenerateSecureCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return (randomNumber % 1000000).ToString("D6");
        }

        /*! \method InvalidateExistingCodesAsync
         * \brief Invalidate any existing codes for the user and purpose
         * \param userId User ID
         * \param purpose Purpose of codes to invalidate
         */
        private async Task InvalidateExistingCodesAsync(string userId, string purpose)
        {
            var existingCodes = await _context.OneTimeCodes
                .Where(c => c.UserId == userId && c.Purpose == purpose && !c.IsUsed)
                .ToListAsync();

            foreach (var code in existingCodes)
            {
                code.MarkAsUsed();
            }

            if (existingCodes.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        /*! \method SendCodeEmailAsync
         * \brief Send the one-time code via email
         * \param email Email address
         * \param code The code to send
         * \param purpose Purpose of the code
         */
        private async Task SendCodeEmailAsync(string email, string code, string purpose)
        {
            var subject = purpose switch
            {
                "2FA_BYPASS" => "Your Ribosoft Security Code",
                _ => "Your Ribosoft Verification Code"
            };

            var htmlMessage = CreateOneTimeCodeEmailTemplate(code, purpose);
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }

        /*! \method CreateOneTimeCodeEmailTemplate
         * \brief Create HTML email template for one-time codes
         * \param code The verification code
         * \param purpose Purpose of the code
         * \return HTML email content
         */
        private static string CreateOneTimeCodeEmailTemplate(string code, string purpose)
        {
            var title = purpose switch
            {
                "2FA_BYPASS" => "Two-Factor Authentication Bypass",
                _ => "Account Verification"
            };

            var description = purpose switch
            {
                "2FA_BYPASS" => "You requested to sign in without your authenticator app. Use this code to complete your login:",
                _ => "Use this verification code to complete your request:"
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Ribosoft Security Code</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>🔐 Security Code</h1>
        <p style='color: #f0f0f0; margin: 10px 0 0 0; font-size: 16px;'>{title}</p>
    </div>

    <div style='background: #ffffff; padding: 40px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333; margin-top: 0;'>Your Verification Code</h2>

        <p>{description}</p>

        <div style='text-align: center; margin: 30px 0;'>
            <div style='background: #f8f9fa; border: 2px solid #667eea; border-radius: 10px; padding: 20px; display: inline-block;'>
                <div style='font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: ""Courier New"", monospace;'>
                    {code}
                </div>
                <p style='margin: 10px 0 0 0; color: #666; font-size: 14px;'>Enter this code in your browser</p>
            </div>
        </div>

        <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404; font-size: 14px;'>
                <strong>⏰ Time Sensitive:</strong> This code will expire in 10 minutes for your security.
            </p>
        </div>

        <div style='background: #d1ecf1; border: 1px solid #bee5eb; border-radius: 5px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #0c5460; font-size: 14px;'>
                <strong>🛡️ Security Tip:</strong> Never share this code with anyone. Ribosoft will never ask for your verification codes.
            </p>
        </div>

        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>

        <div style='background: #f8f9fa; border-radius: 5px; padding: 15px;'>
            <p style='color: #666; font-size: 14px; margin: 0;'>
                <strong>Didn't request this?</strong> If you didn't request this verification code, someone may be trying to access your account. Please secure your account immediately.
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
