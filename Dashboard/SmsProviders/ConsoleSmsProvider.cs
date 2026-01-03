using System;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard.SmsProviders;

/// <summary>
/// Console SMS provider - prints messages to console (development mode)
/// </summary>
public class ConsoleSmsProvider : ISmsProvider
{
    public string Name => "Console (Development)";

    public Task<bool> SendSmsAsync(string toPhoneNumber, string message, string? carrier = null)
    {
        Console.WriteLine("\n" + new string('═', 80));
        Console.WriteLine("📱 SMS MESSAGE (CONSOLE MODE)");
        Console.WriteLine(new string('═', 80));
        Console.WriteLine($"To: {toPhoneNumber}");
        Console.WriteLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        if (!string.IsNullOrEmpty(carrier))
        {
            Console.WriteLine($"Carrier: {carrier}");
        }
        Console.WriteLine(new string('─', 80));
        Console.WriteLine(message);
        Console.WriteLine(new string('═', 80));
        Console.WriteLine();
        
        return Task.FromResult(true);
    }

    public Task<bool> TestConnectionAsync()
    {
        Console.WriteLine("✅ Console SMS provider is always available");
        return Task.FromResult(true);
    }
}
