using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PocketFence_AI.Dashboard;

public static class DashboardService
{
    // Singleton instances for data storage
    public static BlockedContentStore BlockedContent { get; } = new BlockedContentStore();

    public static void ConfigureDashboard(WebApplicationBuilder builder)
    {
        // Register singleton services
        builder.Services.AddSingleton(BlockedContent);

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
            options.IdleTimeout = TimeSpan.FromHours(24);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public static void UseDashboard(WebApplication app)
    {
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

        Console.WriteLine("🛡️  PocketFence Dashboard started at http://localhost:5000");
        Console.WriteLine("📝 Login with: admin / admin");
        Console.WriteLine($"📁 Static files: {app.Environment.WebRootPath}");
        Console.WriteLine($"📊 Blocked content: {BlockedContent.GetBlockedAllTime()} total");
        
        app.Run();
    }
}
