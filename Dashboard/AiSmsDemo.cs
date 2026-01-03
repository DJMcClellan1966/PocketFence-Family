using System;
using System.Threading.Tasks;
using PocketFence_AI.Dashboard;

namespace PocketFence_AI.Examples;

/// <summary>
/// Demo script showing AI SMS Service capabilities
/// </summary>
public class AiSmsDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           AI SMS Service Demo - PocketFence                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Create AI SMS service
        var smsService = new AiSmsService();
        
        Console.WriteLine("\n📊 Service Configuration:");
        smsService.PrintStatistics();

        // Demo 1: Console Mode (default)
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Demo 1: Console Mode (Development)                                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        
        await smsService.SendVerificationCodeAsync("+1-555-123-4567", "482916");

        // Demo 2: Message Optimization
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Demo 2: AI Message Optimization                                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        
        Console.WriteLine("\n📝 Testing message optimization:");
        Console.WriteLine("   Long messages are automatically shortened while keeping clarity");
        
        await smsService.SendNotificationAsync(
            "+1-555-123-4567",
            "Hello! This is a very long message that would normally exceed the standard SMS length limit of 160 characters. " +
            "The AI optimization will automatically shorten this message while maintaining the important information and ensuring clarity."
        );

        // Demo 3: Rate Limiting
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Demo 3: Rate Limiting Protection                                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        
        Console.WriteLine("\n🛡️  Testing rate limiting (max 10 messages per minute):");
        
        for (int i = 1; i <= 3; i++)
        {
            Console.WriteLine($"\n   Attempt {i}:");
            var success = await smsService.SendVerificationCodeAsync("+1-555-999-8888", "123456");
            if (!success && i > 1)
            {
                Console.WriteLine("   ⚠️  Rate limit would prevent rapid-fire messages");
            }
            await Task.Delay(100); // Small delay for demo
        }

        // Demo 4: Provider Testing
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Demo 4: Provider Health Check                                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        
        var testResults = await smsService.TestAllProvidersAsync();

        // Demo 5: Email-to-SMS Example (commented out)
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Demo 5: Email-to-SMS Configuration (Example)                         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        
        Console.WriteLine("\n📧 To use FREE email-to-SMS, add this to DashboardService.cs:");
        Console.WriteLine(@"
    service.ConfigureEmailToSms(
        smtpServer: ""smtp.gmail.com"",
        smtpPort: 587,
        fromEmail: ""yourapp@gmail.com"",
        smtpUsername: ""yourapp@gmail.com"",
        smtpPassword: ""your-app-password"",
        enableSsl: true
    );
");
        
        Console.WriteLine("\n📱 This will send SMS to ALL major US carriers:");
        Console.WriteLine("   • AT&T, T-Mobile, Verizon, Sprint");
        Console.WriteLine("   • Boost, Cricket, US Cellular, Virgin, MetroPCS");
        Console.WriteLine("   • Cost: $0 (completely free!)");

        // Demo 6: Webhook Example
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Demo 6: Webhook Configuration (Example)                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        
        Console.WriteLine("\n🔗 To use custom webhook (GSM modem, etc.):");
        Console.WriteLine(@"
    service.ConfigureWebhook(
        webhookUrl: ""http://localhost:8080/send-sms"",
        authToken: ""your-secret-token""
    );
");
        Console.WriteLine("\n   Your endpoint will receive:");
        Console.WriteLine(@"   {
       ""to"": ""+15551234567"",
       ""message"": ""Your code: 123456"",
       ""timestamp"": ""2026-01-03T10:30:00Z"",
       ""source"": ""PocketFence""
   }");

        // Summary
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Summary: AI SMS Service Features                                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine("\n✨ AI Features:");
        Console.WriteLine("   • Message optimization (auto-shortening)");
        Console.WriteLine("   • Smart routing (multiple providers)");
        Console.WriteLine("   • Retry logic (2 retries with delay)");
        Console.WriteLine("   • Rate limiting (10 msgs/min per number)");
        Console.WriteLine("\n📡 Provider Options:");
        Console.WriteLine("   • Console mode (default, free)");
        Console.WriteLine("   • Email-to-SMS (free, no Twilio needed!)");
        Console.WriteLine("   • Webhook (custom integration)");
        Console.WriteLine("   • Twilio (optional, paid)");
        Console.WriteLine("\n💰 Cost:");
        Console.WriteLine("   • Console: $0");
        Console.WriteLine("   • Email-to-SMS: $0");
        Console.WriteLine("   • Webhook: Depends on backend");
        Console.WriteLine("   • Twilio: ~$0.0075/SMS");
        Console.WriteLine("\n📚 Documentation: See AI_SMS_SERVICE_GUIDE.md");
        Console.WriteLine();
    }
}
