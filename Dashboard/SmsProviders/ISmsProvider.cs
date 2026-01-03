using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard.SmsProviders;

/// <summary>
/// Interface for SMS providers (console, email-to-SMS, webhook, Twilio, etc.)
/// </summary>
public interface ISmsProvider
{
    string Name { get; }
    Task<bool> SendSmsAsync(string toPhoneNumber, string message, string? carrier = null);
    Task<bool> TestConnectionAsync();
}
