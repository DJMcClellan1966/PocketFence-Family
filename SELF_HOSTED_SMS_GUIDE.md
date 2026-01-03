# Self-Hosted SMS Gateway Guide

## Why Self-Host SMS?
- **No monthly fees** - Use your existing phone plan
- **Privacy** - All data stays local
- **Full control** - No third-party dependencies
- **Perfect for verification** - Low volume use case

## Option 1: Android Phone Gateway (Recommended)

### What You Need:
- Old Android phone (Android 5.0+)
- Active SIM card with SMS plan
- WiFi connection
- Free app: "SMS Gateway API" or "SMS Gateway"

### Setup Steps:

1. **Install SMS Gateway App**
   - Download "SMS Gateway API" from Play Store
   - Grant SMS and notification permissions
   - Note the API endpoint (e.g., http://192.168.1.100:8080)

2. **Configure PocketFence**
   
   Open `DashboardService.cs` and update:
   ```csharp
   private static AiSmsService ConfigureSmsService()
   {
       var service = new AiSmsService();
       
       // Use Android phone as SMS gateway
       service.ConfigureWebhook(
           webhookUrl: "http://192.168.1.100:8080/send-sms", // Your phone's IP
           authToken: "your-app-token" // Token from app settings
       );
       
       return service;
   }
   ```

3. **Test It**
   - Register new account with phone number
   - SMS will be sent via your Android phone
   - Check phone for sent message

### Apps to Try:
- **SMS Gateway API** (Free, open source)
- **SMS Gateway** by Capcom
- **SMS Gateway for Android**

## Option 2: USB GSM Modem

### What You Need:
- USB GSM modem ($30-$100)
- SIM card
- USB port on your PC

### Popular Modems:
- Huawei E3372
- ZTE MF823
- Any USB modem with AT commands

### Setup:
1. Install modem drivers
2. Install Gammu or similar software
3. Create webhook that sends AT commands to modem
4. Configure PocketFence to use webhook

## Option 3: Raspberry Pi SMS Gateway

### Setup:
1. Raspberry Pi + USB GSM modem
2. Install Gammu SMSD
3. Create REST API with Python/Node.js
4. Point PocketFence webhook to Pi

## Already Implemented!

Your code already supports this via `WebhookSmsProvider`:

```csharp
// In DashboardService.cs - already exists!
service.ConfigureWebhook(
    webhookUrl: "http://localhost:8080/send-sms",
    authToken: "your-secret-token"
);
```

The webhook sends JSON:
```json
{
  "to": "+17064998453",
  "message": "Your PocketFence verification code is: 123456",
  "carrier": "att",
  "timestamp": "2026-01-03T16:00:00Z",
  "source": "PocketFence"
}
```

## Comparison

| Method | Cost | Setup | Reliability |
|--------|------|-------|-------------|
| Android Gateway | FREE | 10 min | High |
| USB Modem | $30-100 | 30 min | Very High |
| Raspberry Pi | $50-150 | 1 hour | Very High |
| Twilio | $0.0075/SMS | 5 min | Highest |

## For Your Use Case (Family App)

**Recommended: Android Phone Gateway**
- You probably have an old phone
- Free - uses your existing phone plan
- Easy setup
- Perfect for low-volume verification
- Already supported in your code!

## Security Notes

1. **Keep webhook local** - Don't expose to internet
2. **Use strong auth token**
3. **Rate limit** - Already built into AiSmsService
4. **Monitor usage** - Check phone for spam

## Next Steps

1. Find old Android phone
2. Install "SMS Gateway API" app
3. Update DashboardService.cs with phone's IP
4. Test with registration!

No Twilio account needed! 🎉
