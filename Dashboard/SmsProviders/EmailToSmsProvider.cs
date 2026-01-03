using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard.SmsProviders;

/// <summary>
/// Email-to-SMS provider - sends SMS via carrier email gateways (FREE!)
/// Works by sending email to carrier-specific addresses (e.g., 5551234567@txt.att.net)
/// </summary>
public class EmailToSmsProvider : ISmsProvider
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly bool _enableSsl;

    // Carrier email gateways (US carriers) - Updated 2026
    // Note: AT&T has deprecated email-to-SMS. Other carriers may work.
    private static readonly Dictionary<string, string> CarrierGateways = new()
    {
        { "att", "txt.att.net" },  // AT&T deprecated email-to-SMS (not working)
        { "tmobile", "tmomail.net" },
        { "verizon", "vtext.com" },
        { "sprint", "messaging.sprintpcs.com" },
        { "boost", "sms.myboostmobile.com" },
        { "cricket", "sms.cricketwireless.net" },
        { "uscellular", "email.uscc.net" },
        { "virgin", "vmobl.com" },
        { "metropcs", "mymetropcs.com" }
    };

    public string Name => "Email-to-SMS Gateway (Free)";

    public EmailToSmsProvider(string smtpServer, int smtpPort, string fromEmail, 
        string smtpUsername, string smtpPassword, bool enableSsl = true)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _fromEmail = fromEmail;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
        _enableSsl = enableSsl;
    }

    public async Task<bool> SendSmsAsync(string toPhoneNumber, string message, string? carrier = null)
    {
        try
        {
            // Extract phone number digits
            var digits = new string(toPhoneNumber.Where(char.IsDigit).ToArray());
            
            if (digits.Length < 10)
            {
                Console.WriteLine($"❌ Invalid phone number: {toPhoneNumber}");
                return false;
            }

            // Take last 10 digits (removes country code if present)
            var phone10 = digits.Length > 10 ? digits.Substring(digits.Length - 10) : digits;

            // If carrier specified, send only to that carrier
            if (!string.IsNullOrEmpty(carrier) && CarrierGateways.ContainsKey(carrier.ToLower()))
            {
                var gateway = CarrierGateways[carrier.ToLower()];
                var success = await SendToGatewayAsync(phone10, gateway, message);
                
                if (success)
                {
                    Console.WriteLine($"✅ SMS sent via {carrier} email gateway (no bounces)");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to send via {carrier} email gateway");
                }
                
                return success;
            }

            // Try sending to all major carriers (increases delivery chance but causes bounces)
            var tasks = new List<Task<bool>>();
            foreach (var gateway in CarrierGateways.Values)
            {
                tasks.Add(SendToGatewayAsync(phone10, gateway, message));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);

            if (successCount > 0)
            {
                Console.WriteLine($"✅ SMS sent via email gateway to {successCount} carriers (may cause {CarrierGateways.Count - 1} bounces)");
                return true;
            }

            Console.WriteLine("❌ Failed to send via any email gateway");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email-to-SMS error: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SendToGatewayAsync(string phone10, string gateway, string message)
    {
        try
        {
            var emailAddress = $"{phone10}@{gateway}";
            
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _enableSsl,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, "PocketFence"),
                Subject = "", // Empty subject for SMS
                Body = message,
                IsBodyHtml = false
            };
            mailMessage.To.Add(emailAddress);

            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch
        {
            // Silently fail - not all numbers are on all carriers
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _enableSsl,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            // Test SMTP connection
            await Task.Run(() => client.Send(new MailMessage(_fromEmail, _fromEmail, "Test", "Test")));
            Console.WriteLine("✅ Email-to-SMS provider SMTP connection successful");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email-to-SMS provider SMTP test failed: {ex.Message}");
            return false;
        }
    }

    public static void PrintSupportedCarriers()
    {
        Console.WriteLine("\n📱 Supported Carriers for Email-to-SMS:");
        Console.WriteLine("   • AT&T");
        Console.WriteLine("   • T-Mobile");
        Console.WriteLine("   • Verizon");
        Console.WriteLine("   • Sprint");
        Console.WriteLine("   • Boost Mobile");
        Console.WriteLine("   • Cricket Wireless");
        Console.WriteLine("   • US Cellular");
        Console.WriteLine("   • Virgin Mobile");
        Console.WriteLine("   • MetroPCS");
        Console.WriteLine("\nNote: SMS sent to ALL carriers simultaneously for best delivery");
    }
}
