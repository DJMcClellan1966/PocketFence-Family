using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard.SmsProviders;

/// <summary>
/// Webhook SMS provider - sends to custom HTTP endpoint
/// Useful for integrating with custom SMS gateways, GSM modems, or other services
/// </summary>
public class WebhookSmsProvider : ISmsProvider
{
    private readonly string _webhookUrl;
    private readonly string? _authToken;
    private readonly HttpClient _httpClient;

    public string Name => "Webhook (Custom)";

    public WebhookSmsProvider(string webhookUrl, string? authToken = null)
    {
        _webhookUrl = webhookUrl;
        _authToken = authToken;
        _httpClient = new HttpClient();
        
        if (!string.IsNullOrEmpty(_authToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        }
    }

    public async Task<bool> SendSmsAsync(string toPhoneNumber, string message, string? carrier = null)
    {
        try
        {
            var payload = new
            {
                to = toPhoneNumber,
                message = message,
                carrier = carrier,
                timestamp = DateTime.UtcNow,
                source = "PocketFence"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ SMS sent via webhook to {toPhoneNumber}");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Webhook SMS failed: {response.StatusCode} - {error}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Webhook SMS error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_webhookUrl);
            Console.WriteLine($"✅ Webhook endpoint reachable: {response.StatusCode}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Webhook test failed: {ex.Message}");
            return false;
        }
    }
}
