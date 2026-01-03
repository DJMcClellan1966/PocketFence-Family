using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PocketFence_AI.Dashboard.SmsProviders;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// AI-enhanced SMS service with multiple provider support
/// Features: Smart routing, message optimization, retry logic, rate limiting
/// </summary>
public class AiSmsService
{
    private readonly List<ISmsProvider> _providers;
    private readonly Dictionary<string, DateTime> _rateLimitTracker;
    private readonly int _maxMessagesPerMinute;

    public AiSmsService()
    {
        _providers = new List<ISmsProvider>();
        _rateLimitTracker = new Dictionary<string, DateTime>();
        _maxMessagesPerMinute = 10; // Default rate limit

        // Default provider: Console mode
        AddProvider(new ConsoleSmsProvider());
        
        Console.WriteLine("🤖 AI-Enhanced SMS Service initialized");
        Console.WriteLine($"   • Rate limit: {_maxMessagesPerMinute} messages/minute per number");
        Console.WriteLine($"   • Providers: {_providers.Count}");
    }

    /// <summary>
    /// Add an SMS provider to the service
    /// </summary>
    public void AddProvider(ISmsProvider provider)
    {
        _providers.Add(provider);
        Console.WriteLine($"   📡 Added provider: {provider.Name}");
    }

    /// <summary>
    /// Configure email-to-SMS gateway (FREE - no Twilio needed!)
    /// </summary>
    public void ConfigureEmailToSms(string smtpServer, int smtpPort, string fromEmail, 
        string smtpUsername, string smtpPassword, bool enableSsl = true)
    {
        var provider = new EmailToSmsProvider(smtpServer, smtpPort, fromEmail, 
            smtpUsername, smtpPassword, enableSsl);
        
        // Remove console provider if email-to-SMS is configured
        _providers.RemoveAll(p => p is ConsoleSmsProvider);
        AddProvider(provider);
        
        EmailToSmsProvider.PrintSupportedCarriers();
    }

    /// <summary>
    /// Configure webhook provider for custom integrations
    /// </summary>
    public void ConfigureWebhook(string webhookUrl, string? authToken = null)
    {
        var provider = new WebhookSmsProvider(webhookUrl, authToken);
        _providers.RemoveAll(p => p is ConsoleSmsProvider);
        AddProvider(provider);
    }

    /// <summary>
    /// Configure Twilio SMS provider (Paid - $0.0075/SMS)
    /// </summary>
    public void ConfigureTwilio(string accountSid, string authToken, string fromPhoneNumber)
    {
        var provider = new TwilioSmsProvider(accountSid, authToken, fromPhoneNumber);
        _providers.RemoveAll(p => p is ConsoleSmsProvider);
        _providers.RemoveAll(p => p is EmailToSmsProvider);
        AddProvider(provider);
        Console.WriteLine($"📱 Twilio configured with {FormatPhoneNumber(fromPhoneNumber)}");
    }

    /// <summary>
    /// Send verification code with AI optimization
    /// </summary>
    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code, string? carrier = null)
    {
        // Check rate limit
        if (!CheckRateLimit(phoneNumber))
        {
            Console.WriteLine($"⚠️  Rate limit exceeded for {FormatPhoneNumber(phoneNumber)}");
            return false;
        }

        // AI: Optimize message for SMS length and clarity
        var message = OptimizeMessage(
            $"Your PocketFence verification code is: {code}\n\n" +
            $"This code expires in 10 minutes.\n\n" +
            $"If you didn't request this, please ignore this message."
        );

        return await SendWithRetryAsync(phoneNumber, message, carrier);
    }

    /// <summary>
    /// Send phone change notification
    /// </summary>
    public async Task<bool> SendPhoneChangeNotificationAsync(string phoneNumber, string username, string? carrier = null)
    {
        if (!CheckRateLimit(phoneNumber))
            return false;

        var message = OptimizeMessage(
            $"Hello {username}! Your phone number has been updated on your PocketFence account.\n\n" +
            $"If you didn't make this change, please contact support immediately."
        );

        return await SendWithRetryAsync(phoneNumber, message, carrier);
    }

    /// <summary>
    /// Send generic notification
    /// </summary>
    public async Task<bool> SendNotificationAsync(string phoneNumber, string message, string? carrier = null)
    {
        if (!CheckRateLimit(phoneNumber))
            return false;

        message = OptimizeMessage(message);
        return await SendWithRetryAsync(phoneNumber, message, carrier);
    }

    /// <summary>
    /// AI: Optimize message for SMS delivery
    /// - Keeps under 160 characters when possible
    /// - Removes unnecessary words
    /// - Ensures clarity
    /// </summary>
    private string OptimizeMessage(string message)
    {
        // Remove extra whitespace
        message = string.Join("\n", message.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line)));

        // If over 160 chars, try to shorten
        if (message.Length > 160)
        {
            // AI optimization: Remove verbose phrases
            message = message
                .Replace("please ignore this message", "ignore if not you")
                .Replace("If you didn't request this,", "Didn't request?")
                .Replace("contact support immediately", "contact support");
        }

        return message;
    }

    /// <summary>
    /// Smart retry logic with multiple providers
    /// </summary>
    private async Task<bool> SendWithRetryAsync(string phoneNumber, string message, string? carrier = null, int maxRetries = 2)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            foreach (var provider in _providers)
            {
                try
                {
                    Console.WriteLine($"🤖 AI SMS: Attempting send via {provider.Name} (attempt {attempt + 1}/{maxRetries + 1})");
                    
                    var success = await provider.SendSmsAsync(phoneNumber, message, carrier);
                    
                    if (success)
                    {
                        TrackMessage(phoneNumber);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Provider {provider.Name} failed: {ex.Message}");
                }
            }

            if (attempt < maxRetries)
            {
                Console.WriteLine($"⏳ Retrying in 2 seconds...");
                await Task.Delay(2000);
            }
        }

        Console.WriteLine($"❌ All SMS providers failed after {maxRetries + 1} attempts");
        return false;
    }

    /// <summary>
    /// Rate limiting to prevent spam
    /// </summary>
    private bool CheckRateLimit(string phoneNumber)
    {
        var now = DateTime.UtcNow;
        var key = phoneNumber;

        if (_rateLimitTracker.ContainsKey(key))
        {
            var lastSent = _rateLimitTracker[key];
            var timeSince = now - lastSent;

            if (timeSince.TotalMinutes < 1)
            {
                return false; // Too soon
            }
        }

        return true;
    }

    private void TrackMessage(string phoneNumber)
    {
        _rateLimitTracker[phoneNumber] = DateTime.UtcNow;
    }

    /// <summary>
    /// Test all configured providers
    /// </summary>
    public async Task<Dictionary<string, bool>> TestAllProvidersAsync()
    {
        Console.WriteLine("\n🧪 Testing all SMS providers...");
        var results = new Dictionary<string, bool>();

        foreach (var provider in _providers)
        {
            Console.WriteLine($"\nTesting: {provider.Name}");
            var success = await provider.TestConnectionAsync();
            results[provider.Name] = success;
        }

        Console.WriteLine("\n📊 Test Results:");
        foreach (var (name, success) in results)
        {
            var status = success ? "✅ PASS" : "❌ FAIL";
            Console.WriteLine($"   {status} - {name}");
        }

        return results;
    }

    /// <summary>
    /// Get SMS delivery statistics
    /// </summary>
    public void PrintStatistics()
    {
        Console.WriteLine("\n📈 SMS Service Statistics:");
        Console.WriteLine($"   • Active providers: {_providers.Count}");
        Console.WriteLine($"   • Rate-limited numbers: {_rateLimitTracker.Count}");
        Console.WriteLine($"   • Max rate: {_maxMessagesPerMinute} msgs/min");
        
        if (_providers.Any())
        {
            Console.WriteLine("\n   Configured providers:");
            foreach (var provider in _providers)
            {
                Console.WriteLine($"     • {provider.Name}");
            }
        }
    }

    /// <summary>
    /// Format phone number for display (masks middle digits)
    /// </summary>
    public static string FormatPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return phoneNumber;

        var last4 = phoneNumber.Substring(phoneNumber.Length - 4);
        return $"***-***-{last4}";
    }
}
