using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class SettingsModel : PageModel
{
    public string NotificationEmail { get; set; } = "";

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        // TODO: Load settings from config file
        NotificationEmail = "parent@example.com";

        return Page();
    }

    public IActionResult OnPost(string filterLevel, string notificationEmail)
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        // TODO: Save settings to config file
        NotificationEmail = notificationEmail;

        TempData["SuccessMessage"] = "Settings saved successfully!";
        return RedirectToPage();
    }
}
