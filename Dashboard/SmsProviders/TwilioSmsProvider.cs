using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard.SmsProviders;

/// <summary>
/// Twilio SMS provider - sends SMS via Twilio API
/// Requires Twilio account (https://www.twilio.com)
/// Cost: ~$0.0075 per SMS in US
/// </summary>
public class TwilioSmsProvider : ISmsProvider
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;
    private readonly HttpClient _httpClient;

    public string Name => "Twilio SMS";

    public TwilioSmsProvider(string accountSid, string authToken, string fromPhoneNumber)
    {
        _accountSid = accountSid;
        _authToken = authToken;
        _fromPhoneNumber = fromPhoneNumber;
        _httpClient = new HttpClient();

        // Set up Basic Authentication
        var authBytes = Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}");
        var base64Auth = Convert.ToBase64String(authBytes);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Auth}");
    }

    public async Task<bool> SendSmsAsync(string toPhoneNumber, string message, string? carrier = null)
    {
        try
        {
            // Twilio API endpoint
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

            // Build form data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", _fromPhoneNumber),
                new KeyValuePair<string, string>("To", toPhoneNumber),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await _httpClient.PostAsync(url, formData);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ SMS sent via Twilio to {FormatPhoneNumber(toPhoneNumber)}");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Twilio SMS failed: {response.StatusCode}");
            Console.WriteLine($"   Error: {error}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Twilio SMS error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            // Test by fetching account info
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Twilio connection successful");
                return true;
            }

            Console.WriteLine($"❌ Twilio connection failed: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Twilio connection error: {ex.Message}");
            return false;
        }
    }

    private string FormatPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length >= 4)
            return $"***-***-{phoneNumber.Substring(phoneNumber.Length - 4)}";
        return "***";
    }
}
