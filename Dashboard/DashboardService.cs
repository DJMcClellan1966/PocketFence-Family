using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PocketFence_AI.Dashboard;

public static class DashboardService
{
    public static void ConfigureDashboard(WebApplicationBuilder builder)
    {
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
        builder.WebHost.UseUrls("http://localhost:5000");

        ConfigureDashboard(builder);

        var app = builder.Build();

        UseDashboard(app);

        Console.WriteLine("🛡️  PocketFence Dashboard started at http://localhost:5000");
        Console.WriteLine("📝 Login with: admin / admin");
        Console.WriteLine($"📁 Static files: {app.Environment.WebRootPath}");
        
        app.Run();
    }
}
