using System;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Service for sending SMS messages (phone verification codes, notifications)
/// Supports console mode for development and Twilio for production
/// NOTE: Twilio package not included by default. Install via: dotnet add package Twilio
/// </summary>
public class SmsService
{
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromPhoneNumber;
    private readonly bool _consoleMode;

    /// <summary>
    /// Creates SMS service in console mode (for development - no Twilio needed)
    /// </summary>
    public SmsService()
    {
        _consoleMode = true;
        Console.WriteLine("📱 SmsService initialized in CONSOLE MODE (development)");
        Console.WriteLine("   SMS messages will be printed to console instead of sent");
    }

    /// <summary>
    /// Creates SMS service with Twilio configuration (for production)
    /// Requires Twilio NuGet package: dotnet add package Twilio
    /// </summary>
    /// <param name="accountSid">Twilio Account SID</param>
    /// <param name="authToken">Twilio Auth Token</param>
    /// <param name="fromPhoneNumber">Your Twilio phone number (format: +1234567890)</param>
    public SmsService(string accountSid, string authToken, string fromPhoneNumber)
    {
        _accountSid = accountSid;
        _authToken = authToken;
        _fromPhoneNumber = fromPhoneNumber;
        _consoleMode = false;

        // NOTE: Twilio initialization would go here if package is installed
        // TwilioClient.Init(_accountSid, _authToken);
        Console.WriteLine($"📱 SmsService initialized with Twilio configuration");
        Console.WriteLine("   WARNING: Twilio package not installed. Install via: dotnet add package Twilio");
        Console.WriteLine($"   To use real SMS, uncomment Twilio code in SmsService.cs");
    }

    /// <summary>
    /// Sends phone verification code to user
    /// </summary>
    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string verificationCode)
    {
        var message = $"Your PocketFence verification code is: {verificationCode}\n\nThis code expires in 10 minutes.";
        
        return await SendSmsAsync(phoneNumber, message);
    }

    /// <summary>
    /// Sends phone number change notification
    /// </summary>
    public async Task<bool> SendPhoneChangeNotificationAsync(string phoneNumber, string username)
    {
        var message = $"Hello {username}! Your phone number has been updated on your PocketFence account. If you didn't make this change, please contact support immediately.";
        
        return await SendSmsAsync(phoneNumber, message);
    }

    /// <summary>
    /// Sends a generic notification SMS
    /// </summary>
    public async Task<bool> SendNotificationAsync(string phoneNumber, string message)
    {
        return await SendSmsAsync(phoneNumber, message);
    }

    /// <summary>
    /// Core method to send SMS (console or Twilio)
    /// </summary>
    private async Task<bool> SendSmsAsync(string toPhoneNumber, string message)
    {
        try
        {
            if (_consoleMode)
            {
                // Console mode - print SMS to console
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("📱 SMS MESSAGE (CONSOLE MODE)");
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"To: {toPhoneNumber}");
                Console.WriteLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine(message);
                Console.WriteLine(new string('=', 80));
                Console.WriteLine();
                
                await Task.CompletedTask; // Async for consistency
                return true;
            }
            else
            {
                // Production mode - would send via Twilio if package installed
                Console.WriteLine($"❌ Cannot send SMS - Twilio package not installed");
                Console.WriteLine($"   To enable real SMS, run: dotnet add package Twilio");
                Console.WriteLine($"   Then uncomment Twilio code in SmsService.cs");
                
                /* UNCOMMENT THIS CODE AFTER INSTALLING TWILIO PACKAGE:
                if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_fromPhoneNumber))
                {
                    Console.WriteLine("❌ Twilio credentials not configured");
                    return false;
                }

                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_fromPhoneNumber),
                    to: new PhoneNumber(toPhoneNumber)
                );

                Console.WriteLine($"✅ SMS sent successfully! SID: {messageResource.Sid}");
                return true;
                */
                
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send SMS: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tests the SMS service configuration
    /// </summary>
    public async Task<bool> TestConfigurationAsync(string testPhoneNumber)
    {
        Console.WriteLine("Testing SMS service configuration...");
        return await SendSmsAsync(testPhoneNumber, "This is a test message from PocketFence SMS service.");
    }

    /// <summary>
    /// Formats phone number for display (masks middle digits)
    /// </summary>
    public static string FormatPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return phoneNumber;

        // Show last 4 digits: +1 (555) ***-1234
        var last4 = phoneNumber.Substring(phoneNumber.Length - 4);
        return $"***-***-{last4}";
    }
}
