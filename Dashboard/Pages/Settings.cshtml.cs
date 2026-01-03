using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class SettingsModel : AuthenticatedPageModel
{
    public string NotificationEmail { get; set; } = "";

    public void OnGet()
    {
        // Authentication handled by AuthenticatedPageModel base class

        // TODO: Load settings from config file
        NotificationEmail = "parent@example.com";
    }

    public IActionResult OnPost(string filterLevel, string notificationEmail)
    {
        // Authentication handled by AuthenticatedPageModel base class

        // Validate email if provided
        if (!string.IsNullOrWhiteSpace(notificationEmail))
        {
            if (!IsValidEmail(notificationEmail))
            {
                TempData["ErrorMessage"] = "❌ Invalid email address format. Please enter a valid email (e.g., parent@example.com).";
                return RedirectToPage();
            }
        }

        // Validate filter level
        if (!new[] { "strict", "moderate", "relaxed" }.Contains(filterLevel?.ToLower()))
        {
            TempData["ErrorMessage"] = "❌ Invalid filtering level selected. Please choose Strict, Moderate, or Relaxed.";
            return RedirectToPage();
        }

        // TODO: Save settings to config file
        NotificationEmail = notificationEmail;

        TempData["SuccessMessage"] = "✅ Settings saved successfully! Your protection preferences have been updated.";
        return RedirectToPage();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
