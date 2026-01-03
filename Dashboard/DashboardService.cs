using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PocketFence_AI.Dashboard.Security;

namespace PocketFence_AI.Dashboard;

public static class DashboardService
{
    // Singleton instances for data storage and security
    public static BlockedContentStore BlockedContent { get; } = new BlockedContentStore();
    public static LoginRateLimiter RateLimiter { get; } = new LoginRateLimiter(maxAttempts: 5, lockoutMinutes: 15);
    public static SecurityAuditLogger AuditLogger { get; } = new SecurityAuditLogger();

    public static void ConfigureDashboard(WebApplicationBuilder builder)
    {
        // Register singleton services
        builder.Services.AddSingleton(BlockedContent);
        builder.Services.AddSingleton(RateLimiter);
        builder.Services.AddSingleton(AuditLogger);

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

        Console.WriteLine("🛡️  PocketFence Dashboard started at http://localhost:5000");
        Console.WriteLine("📝 Login with: admin / PocketFence2026!");
        Console.WriteLine($"📁 Static files: {app.Environment.WebRootPath}");
        Console.WriteLine($"📊 Blocked content: {BlockedContent.GetBlockedAllTime()} total");
        Console.WriteLine($"🧹 Rate limiter cleanup task started (runs every {SecurityConstants.RateLimitCleanupIntervalMinutes} minutes)");
        
        app.Run();
    }
}
