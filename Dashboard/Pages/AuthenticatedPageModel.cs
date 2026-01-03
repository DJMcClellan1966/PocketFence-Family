using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Filters;
using PocketFence_AI.Dashboard.Security;

namespace PocketFence_AI.Dashboard.Pages;

/// <summary>
/// Base page model that enforces authentication on all pages
/// </summary>
public class AuthenticatedPageModel : PageModel
{
    public string Username { get; set; } = "Parent";
    
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        // Check if user is authenticated
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            // Store the intended destination
            var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
            
            // Redirect to login with return URL
            context.Result = Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return;
        }

        // Load username from session
        Username = HttpContext.Session.GetString("Username") ?? "Parent";
        
        // Update last activity timestamp for session timeout tracking
        HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
        
        base.OnPageHandlerExecuting(context);
    }
    
    /// <summary>
    /// Gets time remaining before session expires
    /// </summary>
    public TimeSpan GetSessionTimeRemaining()
    {
        var lastActivityStr = HttpContext.Session.GetString("LastActivity");
        if (string.IsNullOrEmpty(lastActivityStr))
        {
            return TimeSpan.Zero;
        }

        var lastActivity = DateTime.Parse(lastActivityStr);
        var sessionTimeout = TimeSpan.FromMinutes(SecurityConstants.SessionTimeoutMinutes);
        var elapsed = DateTime.UtcNow - lastActivity;
        var remaining = sessionTimeout - elapsed;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
