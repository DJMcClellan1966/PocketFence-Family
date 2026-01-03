using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PocketFence_AI.Dashboard.Security;
using System.Net;
using System.Net.Sockets;

namespace PocketFence_AI.Dashboard;

public static class DashboardService
{
    // Singleton instances for data storage and security (GPT4All-optimized)
    public static BlockedContentStore BlockedContent { get; } = new BlockedContentStore();
    public static OptimizedLoginRateLimiter RateLimiter { get; } = new OptimizedLoginRateLimiter(maxAttempts: 5, lockoutMinutes: 15);
    public static OptimizedSecurityAuditLogger AuditLogger { get; } = new OptimizedSecurityAuditLogger();
    public static SettingsManager Settings { get; } = new SettingsManager();
    public static UserManager UserManager { get; } = new UserManager();
    public static EmailService EmailService { get; } = ConfigureEmailService();
    
    // AI-Enhanced SMS Service (replaces simple SmsService)
    public static AiSmsService SmsService { get; } = ConfigureSmsService();

    private static EmailService ConfigureEmailService()
    {
        // Get base URL from environment variable or use localhost as default
        var baseUrl = Environment.GetEnvironmentVariable("POCKETFENCE_BASE_URL") ?? "http://localhost:5000";
        
        // Configure email service with Gmail SMTP
        var service = new EmailService(
            smtpServer: "smtp.gmail.com",
            smtpPort: 587,
            fromEmail: "DJMcClellan1966@gmail.com",
            fromName: "PocketFence Family",
            smtpUsername: "DJMcClellan1966@gmail.com",
            smtpPassword: "gxen oitm nkec upvp", // Gmail app password
            enableSsl: true,
            baseUrl: baseUrl
        );
        
        Console.WriteLine("[EmailService] ✅ Configured with Gmail SMTP");
        return service;
    }

    private static AiSmsService ConfigureSmsService()
    {
        var service = new AiSmsService();
        
        // OPTION 1: Use Twilio (Paid - $0.0075/SMS, most reliable) ✅
        service.ConfigureTwilio(
            accountSid: "YOUR_TWILIO_ACCOUNT_SID",
            authToken: "YOUR_TWILIO_AUTH_TOKEN",
            fromPhoneNumber: "+18443511339" // Your Twilio phone number
        );
        
        /* OPTION 2: Use FREE email-to-SMS gateway (AT&T deprecated, may not work)
        service.ConfigureEmailToSms(
            smtpServer: "smtp.gmail.com",
            smtpPort: 587,
            fromEmail: "DJMcClellan1966@gmail.com",
            smtpUsername: "DJMcClellan1966@gmail.com",
            smtpPassword: "gxen oitm nkec upvp",
            enableSsl: true
        );
        */
        
        /* OPTION 3: Use custom webhook (for GSM modems, custom gateways)
        service.ConfigureWebhook(
            webhookUrl: "http://localhost:8080/send-sms",
            authToken: "your-secret-token"
        );
        */
        
        return service;
    }

    public static void ConfigureDashboard(WebApplicationBuilder builder)
    {
        // Register singleton services (optimized versions)
        builder.Services.AddSingleton(BlockedContent);
        builder.Services.AddSingleton(RateLimiter);
        builder.Services.AddSingleton(AuditLogger);
        builder.Services.AddSingleton(Settings);
        builder.Services.AddSingleton(UserManager);
        builder.Services.AddSingleton(EmailService);
        builder.Services.AddSingleton(SmsService);

        // Add Razor Pages services with custom root path
        builder.Services.AddRazorPages()
            .AddRazorPagesOptions(options =>
            {
                options.RootDirectory = "/Dashboard/Pages";
            });
        
        // Add session support for authentication
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(SecurityConstants.SessionTimeoutMinutes);
            options.Cookie.HttpOnly = true; // Prevents JavaScript access to cookie
            options.Cookie.IsEssential = true; // Required for GDPR compliance
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS when available
            options.Cookie.SameSite = SameSiteMode.Strict; // Prevents CSRF attacks
        });
    }

    public static void UseDashboard(WebApplication app)
    {
        // Security Headers Middleware
        app.Use(async (context, next) =>
        {
            // Content Security Policy
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "font-src 'self' https://cdn.jsdelivr.net; " +
                "img-src 'self' data:; " +
                "connect-src 'self';");
            
            // Prevent clickjacking
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            // Prevent MIME type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Enable XSS protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            // Referrer policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Permissions policy
            context.Response.Headers.Append("Permissions-Policy", 
                "geolocation=(), microphone=(), camera=()");
            
            await next();
        });
        
        // Serve static files (CSS, JS, images)
        app.UseStaticFiles();

        // Enable session middleware
        app.UseSession();

        // Map Razor Pages
        app.MapRazorPages();

        // Default route to login page
        app.MapGet("/", (HttpContext context) =>
        {
            if (context.Session.GetString("IsAuthenticated") == "true")
            {
                return Results.Redirect("/index");
            }
            return Results.Redirect("/login");
        });

        // Logout endpoint
        app.MapGet("/logout", (HttpContext context) =>
        {
            context.Session.Clear();
            return Results.Redirect("/login");
        });
    }

    public static void StartDashboard(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Dashboard", "wwwroot")
        });
        
        // Configure web server
        //builder.WebHost.UseUrls("http://localhost:5000");
        builder.WebHost.UseUrls("http://0.0.0.0:5000");
        ConfigureDashboard(builder);

        var app = builder.Build();

        UseDashboard(app);

        // Add sample data if no blocks exist (for testing)
        if (BlockedContent.GetBlockedAllTime() == 0)
        {
            Console.WriteLine("📝 No blocked content found, adding sample data...");
            SeedData.AddSampleBlocks(BlockedContent);
        }

        // Start background cleanup task for rate limiter
        var cleanupTimer = new System.Threading.Timer(
            callback: _ => 
            {
                RateLimiter.Cleanup();
                #if DEBUG
                Console.WriteLine($"🧹 Rate limiter cleanup completed at {DateTime.Now:HH:mm:ss}");
                #endif
            },
            state: null,
            dueTime: TimeSpan.FromMinutes(SecurityConstants.RateLimitCleanupIntervalMinutes),
            period: TimeSpan.FromMinutes(SecurityConstants.RateLimitCleanupIntervalMinutes)
        );

        Console.WriteLine("🛡️  PocketFence Dashboard started!");
        Console.WriteLine($"📡 Local:    http://localhost:5000");
        Console.WriteLine($"📡 Network:  http://{GetLocalIPAddress()}:5000");
        Console.WriteLine("📝 Login with: admin / PocketFence2026!");
        Console.WriteLine($"📁 Static files: {app.Environment.WebRootPath}");
        Console.WriteLine($"📊 Blocked content: {BlockedContent.GetBlockedAllTime()} total");
        Console.WriteLine($"🧹 Rate limiter cleanup task started (runs every {SecurityConstants.RateLimitCleanupIntervalMinutes} minutes)");
        Console.WriteLine();
        Console.WriteLine("💡 To access from other devices:");
        Console.WriteLine($"   1. Make sure your firewall allows port 5000");
        Console.WriteLine($"   2. Connect from other device: http://{GetLocalIPAddress()}:5000");
        Console.WriteLine($"   3. Set base URL environment variable: POCKETFENCE_BASE_URL=http://{GetLocalIPAddress()}:5000");
        
        app.Run();
    }

    private static string GetLocalIPAddress()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString() ?? "localhost";
        }
        catch
        {
            return "localhost";
        }
    }
}
