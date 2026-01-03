using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Email service for sending verification and notification emails
/// Note: This is a basic SMTP implementation. For production, consider using services like SendGrid, AWS SES, or Azure Communication Services.
/// </summary>
public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;
    private readonly bool _enableSsl;
    private readonly bool _isConfigured;
    private readonly string _baseUrl;

    public EmailService(
        string? smtpServer = null,
        int smtpPort = 587,
        string? fromEmail = null,
        string? fromName = "PocketFence",
        string? smtpUsername = null,
        string? smtpPassword = null,
        bool enableSsl = true,
        string baseUrl = "http://localhost:5000")
    {
        _smtpServer = smtpServer ?? "";
        _smtpPort = smtpPort;
        _fromEmail = fromEmail ?? "noreply@pocketfence.local";
        _fromName = fromName ?? "PocketFence";
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
        _enableSsl = enableSsl;
        _baseUrl = baseUrl;

        // Check if SMTP is configured
        _isConfigured = !string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(fromEmail);

        if (!_isConfigured)
        {
            Console.WriteLine("[EmailService] ⚠️ Email service not configured. Emails will be logged to console only.");
            Console.WriteLine("[EmailService] To enable email: Configure SMTP settings in appsettings.json");
        }
    }

    /// <summary>
    /// Send email verification
    /// </summary>
    public async Task<bool> SendVerificationEmailAsync(string toEmail, string username, string verificationToken)
    {
        var verificationUrl = $"{_baseUrl}/VerifyEmail?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(verificationToken)}";
        
        var subject = "Verify Your PocketFence Account";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🛡️ Welcome to PocketFence!</h1>
        </div>
        <div class=""content"">
            <p>Hi {username},</p>
            <p>Thank you for creating a PocketFence account! To complete your registration, please verify your email address by clicking the button below:</p>
            <p style=""text-align: center;"">
                <a href=""{verificationUrl}"" class=""button"">Verify Email Address</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; background: #fff; padding: 10px; border-radius: 5px;"">{verificationUrl}</p>
            <p>This verification link will expire in 24 hours.</p>
            <p>If you didn't create this account, please ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>© 2026 PocketFence - Protecting Families Online</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetToken)
    {
        var resetUrl = $"{_baseUrl}/ResetPassword?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";
        
        var subject = "Reset Your PocketFence Password";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class=""content"">
            <p>Hi {username},</p>
            <p>We received a request to reset your PocketFence password. Click the button below to create a new password:</p>
            <p style=""text-align: center;"">
                <a href=""{resetUrl}"" class=""button"">Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; background: #fff; padding: 10px; border-radius: 5px;"">{resetUrl}</p>
            <div class=""warning"">
                <strong>⚠️ Security Notice:</strong> This password reset link will expire in 1 hour. If you didn't request this reset, please ignore this email and your password will remain unchanged.
            </div>
        </div>
        <div class=""footer"">
            <p>© 2026 PocketFence - Protecting Families Online</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }

    /// <summary>
    /// Send generic notification email
    /// </summary>
    public async Task<bool> SendNotificationEmailAsync(string toEmail, string subject, string message)
    {
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🛡️ PocketFence Notification</h1>
        </div>
        <div class=""content"">
            {message}
        </div>
        <div class=""footer"">
            <p>© 2026 PocketFence - Protecting Families Online</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }

    /// <summary>
    /// Core email sending method
    /// </summary>
    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        // If SMTP not configured, log to console instead
        if (!_isConfigured)
        {
            Console.WriteLine($"\n{'=' ,60}");
            Console.WriteLine("[EmailService] 📧 EMAIL (Console Mode)");
            Console.WriteLine($"{'=',60}");
            Console.WriteLine($"To: {toEmail}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body:\n{body}");
            Console.WriteLine($"{'=',60}\n");
            return true; // Simulate success
        }

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _enableSsl,
                Credentials = !string.IsNullOrEmpty(_smtpUsername) && !string.IsNullOrEmpty(_smtpPassword)
                    ? new NetworkCredential(_smtpUsername, _smtpPassword)
                    : null
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[EmailService] ✅ Email sent to {toEmail}: {subject}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] ❌ Failed to send email to {toEmail}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test email configuration
    /// </summary>
    public async Task<bool> TestConfigurationAsync(string testEmail)
    {
        return await SendEmailAsync(
            testEmail,
            "PocketFence Email Test",
            "<p>This is a test email from PocketFence. If you received this, your email configuration is working correctly!</p>",
            isHtml: true);
    }
}
