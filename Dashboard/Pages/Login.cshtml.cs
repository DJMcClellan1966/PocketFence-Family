using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class LoginModel : PageModel
{
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Check if already logged in
        if (HttpContext.Session.GetString("IsAuthenticated") == "true")
        {
            Response.Redirect("/");
        }
    }

    public IActionResult OnPost(string username, string password, bool rememberMe = false)
    {
        // TODO: Replace with proper authentication
        // For MVP, use simple hardcoded credentials (change in production!)
        if (username == "admin" && password == "admin")
        {
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("Username", username);
            
            return Redirect("/");
        }

        ErrorMessage = "Invalid username or password";
        return Page();
    }
}
