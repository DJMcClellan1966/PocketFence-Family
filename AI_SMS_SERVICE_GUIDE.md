# AI SMS Service Configuration Guide

## Overview
The AI SMS Service provides multiple ways to send SMS messages without requiring external paid services like Twilio. It includes AI features like message optimization, smart routing, retry logic, and rate limiting.

## Available Providers

### 1. Console Mode (Default) ✅
**Cost:** FREE  
**Setup:** None required (works out of the box)  
**Best for:** Development and testing

Messages are printed to the console in a formatted view. Perfect for testing without any configuration.

```csharp
// Already configured by default - no code needed!
```

### 2. Email-to-SMS Gateway (Recommended) ✅
**Cost:** FREE  
**Setup:** SMTP account (Gmail, Outlook, etc.)  
**Best for:** Production use without external SMS services

Sends SMS by emailing carrier-specific gateways (e.g., `5551234567@txt.att.net`). Works with all major US carriers.

**Supported Carriers:**
- AT&T
- T-Mobile
- Verizon
- Sprint
- Boost Mobile
- Cricket Wireless
- US Cellular
- Virgin Mobile
- MetroPCS

**Setup Steps:**

1. Get SMTP credentials:
   - **Gmail:** Use App Password (https://myaccount.google.com/apppasswords)
   - **Outlook:** Use regular password
   - **Custom:** Any SMTP server

2. Update `DashboardService.cs`:
```csharp
private static AiSmsService ConfigureSmsService()
{
    var service = new AiSmsService();
    
    service.ConfigureEmailToSms(
        smtpServer: "smtp.gmail.com",      // or smtp-mail.outlook.com
        smtpPort: 587,                      // 587 for TLS, 465 for SSL
        fromEmail: "yourapp@gmail.com",
        smtpUsername: "yourapp@gmail.com",
        smtpPassword: "your-app-password",  // Gmail: use app password
        enableSsl: true
    );
    
    return service;
}
```

**Advantages:**
- Completely free (no per-message charges)
- Sends to ALL carriers simultaneously (better delivery)
- No account registration needed
- Works with existing email infrastructure

**Limitations:**
- Requires knowing user's carrier (service tries all carriers)
- Some carriers may have delays (usually < 30 seconds)
- Messages may be filtered as spam if sent too frequently

### 3. Webhook Provider ✅
**Cost:** Depends on your backend  
**Setup:** Custom HTTP endpoint  
**Best for:** GSM modems, custom gateways, or other SMS APIs

Sends HTTP POST requests to your custom endpoint with SMS data.

**Request Format:**
```json
{
  "to": "+15551234567",
  "message": "Your verification code is: 123456",
  "timestamp": "2026-01-03T10:30:00Z",
  "source": "PocketFence"
}
```

**Setup:**
```csharp
private static AiSmsService ConfigureSmsService()
{
    var service = new AiSmsService();
    
    service.ConfigureWebhook(
        webhookUrl: "http://localhost:8080/send-sms",
        authToken: "your-secret-token"
    );
    
    return service;
}
```

**Use Cases:**
- GSM modem connected to local server
- Integration with existing SMS infrastructure
- Custom SMS gateway (e.g., Plivo, MessageBird, Nexmo)

### 4. Twilio (Optional) ⚠️
**Cost:** Paid service (~$0.0075 per SMS)  
**Setup:** Requires Twilio account + NuGet package  
**Best for:** Large-scale production with guaranteed delivery

If you want to use Twilio, uncomment the code in the original `SmsService.cs` or create a Twilio provider.

## AI Features

### 1. Message Optimization
Automatically shortens messages to fit SMS length limits (160 chars):
- Removes verbose phrases
- Maintains clarity and meaning
- Ensures important information is preserved

### 2. Smart Routing
- Tries multiple providers in sequence
- Automatic retry logic (2 retries by default)
- Fails over to next provider if one fails

### 3. Rate Limiting
- Prevents spam (max 10 messages per minute per number)
- Tracks message frequency
- Protects against abuse

### 4. Retry Logic
- Automatically retries failed messages
- 2-second delay between retries
- Tries all configured providers

## Testing

### Test All Providers
```csharp
await SmsService.TestAllProvidersAsync();
```

### View Statistics
```csharp
SmsService.PrintStatistics();
```

### Send Test Message
```csharp
var success = await SmsService.SendVerificationCodeAsync("+15551234567", "123456");
```

## Example Configurations

### Development Setup (Console Mode)
```csharp
// No configuration needed - uses console by default
var service = new AiSmsService();
```

### Production Setup (Email-to-SMS with Gmail)
```csharp
var service = new AiSmsService();
service.ConfigureEmailToSms(
    smtpServer: "smtp.gmail.com",
    smtpPort: 587,
    fromEmail: "noreply@pocketfence.com",
    smtpUsername: "noreply@pocketfence.com",
    smtpPassword: "xxxx xxxx xxxx xxxx", // App password
    enableSsl: true
);
```

### Hybrid Setup (Email-to-SMS + Webhook Fallback)
```csharp
var service = new AiSmsService();

// Primary: Email-to-SMS (free)
service.ConfigureEmailToSms(...);

// Fallback: Custom webhook
service.ConfigureWebhook("http://backup-sms.local/send", "token");
```

## Troubleshooting

### Email-to-SMS Not Working

**Problem:** Messages not arriving  
**Solutions:**
1. Verify SMTP credentials are correct
2. Check if Gmail/Outlook allows "Less secure app access"
3. Use Gmail App Password instead of regular password
4. Verify phone number format (+1XXXXXXXXXX)
5. Check carrier gateway is correct

**Problem:** Marked as spam  
**Solutions:**
1. Add sender to contacts
2. Reduce message frequency
3. Use professional "from" email address

### Webhook Not Working

**Problem:** Endpoint not receiving messages  
**Solutions:**
1. Verify URL is accessible
2. Check auth token is correct
3. Ensure endpoint accepts POST requests
4. Verify JSON parsing on backend

## Security Considerations

1. **SMTP Credentials:** Store in environment variables or secure configuration
2. **Auth Tokens:** Use strong, random tokens for webhooks
3. **Rate Limiting:** Enabled by default (10 msgs/min)
4. **Phone Number Privacy:** Numbers masked in logs (***-***-1234)

## Cost Comparison

| Provider | Setup Cost | Per-Message Cost | Monthly Cost (1000 msgs) |
|----------|-----------|------------------|--------------------------|
| Console | $0 | $0 | $0 |
| Email-to-SMS | $0 | $0 | $0 |
| Webhook (GSM) | Hardware cost | $0 | $0 |
| Twilio | $0 | $0.0075 | $7.50 |

## Recommended Setup

**For Development:**  
Use Console mode (default)

**For Production (Budget):**  
Use Email-to-SMS with Gmail/Outlook (FREE!)

**For Production (Enterprise):**  
Use Webhook with GSM modem or paid service

**For Production (Critical):**  
Use Email-to-SMS + Twilio fallback

## Support

For issues or questions:
1. Check console logs for detailed error messages
2. Test providers using `TestAllProvidersAsync()`
3. Verify configuration matches examples above
4. Check firewall/network settings for webhooks
