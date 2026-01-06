using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class SettingsModel : AuthenticatedPageModel
{
    private readonly SettingsManager _settingsManager;

    public string NotificationEmail { get; set; } = "";
    public string FilterLevel { get; set; } = "moderate";
    public List<string> BlockedCategories { get; set; } = new();

    public SettingsModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public async Task OnGetAsync()
    {
        // Authentication handled by AuthenticatedPageModel base class

        // Load actual settings from file
        var settings = await _settingsManager.LoadSettingsAsync();
        NotificationEmail = settings.NotificationEmail;
        FilterLevel = settings.FilterLevel;
        BlockedCategories = settings.BlockedCategories;
    }

    public async Task<IActionResult> OnPostAsync(string filterLevel, string? notificationEmail, List<string>? blockedCategories)
    {
        // Authentication handled by AuthenticatedPageModel base class
        
        Console.WriteLine($"[SETTINGS] POST received - FilterLevel: {filterLevel}, Email: {notificationEmail}, Categories: {blockedCategories?.Count ?? 0}");

        // Validate email if provided
        if (!string.IsNullOrWhiteSpace(notificationEmail))
        {
            if (!IsValidEmail(notificationEmail))
            {
                Console.WriteLine($"[SETTINGS] Invalid email: {notificationEmail}");
                TempData["ErrorMessage"] = "❌ Invalid email address format. Please enter a valid email (e.g., parent@example.com).";
                return RedirectToPage();
            }
        }

        // Validate filter level
        if (!new[] { "strict", "moderate", "relaxed" }.Contains(filterLevel?.ToLower()))
        {
            Console.WriteLine($"[SETTINGS] Invalid filter level: {filterLevel}");
            TempData["ErrorMessage"] = "❌ Invalid filtering level selected. Please choose Strict, Moderate, or Relaxed.";
            return RedirectToPage();
        }

        try
        {
            Console.WriteLine("[SETTINGS] Loading current settings...");
            // Load current settings, update them, and save
            var settings = await _settingsManager.LoadSettingsAsync();
            
            Console.WriteLine("[SETTINGS] Updating settings...");
            settings.FilterLevel = filterLevel?.ToLower() ?? "medium";
            settings.NotificationEmail = notificationEmail ?? "";
            settings.BlockedCategories = blockedCategories ?? new List<string>();
            
            Console.WriteLine($"[SETTINGS] Saving settings - FilterLevel: {settings.FilterLevel}, Categories: {settings.BlockedCategories.Count}");
            await _settingsManager.SaveSettingsAsync(settings);
            
            Console.WriteLine("[SETTINGS] Settings saved successfully!");
            TempData["SuccessMessage"] = "✅ Settings saved successfully! Your protection preferences have been updated.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SETTINGS] ERROR: {ex.Message}");
            Console.WriteLine($"[SETTINGS] Stack trace: {ex.StackTrace}");
            TempData["ErrorMessage"] = $"❌ Error saving settings: {ex.Message}";
            return RedirectToPage();
        }
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
