using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Ribosoft.Services;

namespace Ribosoft.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            var htmlMessage = CreateEmailConfirmationTemplate(link);
            return emailSender.SendEmailAsync(email, "Confirm your Ribosoft account", htmlMessage);
        }

        public static Task SendPasswordResetAsync(this IEmailSender emailSender, string email, string link)
        {
            var htmlMessage = CreatePasswordResetTemplate(link);
            return emailSender.SendEmailAsync(email, "Reset your Ribosoft password", htmlMessage);
        }

        private static string CreateEmailConfirmationTemplate(string confirmationLink)
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
            <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'
               style='background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block; font-size: 16px;'>
                Confirm Email Address
            </a>
        </div>

        <p style='color: #666; font-size: 14px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='background: #f5f5f5; padding: 10px; border-radius: 5px; word-break: break-all; font-size: 12px; color: #666;'>
            {HtmlEncoder.Default.Encode(confirmationLink)}
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

        private static string CreatePasswordResetTemplate(string resetLink)
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
            <a href='{HtmlEncoder.Default.Encode(resetLink)}'
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
            {HtmlEncoder.Default.Encode(resetLink)}
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
    }
}
