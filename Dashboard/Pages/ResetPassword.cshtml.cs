using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager _userManager;

    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public ResetPasswordModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    public IActionResult OnGet(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            ErrorMessage = "❌ Invalid password reset link.";
            return Page();
        }

        Email = email;
        Token = token;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string email, string token, string newPassword, string confirmPassword)
    {
        Email = email;
        Token = token;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            ErrorMessage = "❌ Invalid password reset link.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ErrorMessage = "⚠️ Please enter a new password.";
            return Page();
        }

        if (newPassword != confirmPassword)
        {
            ErrorMessage = "❌ Passwords do not match.";
            return Page();
        }

        var (success, message) = await _userManager.ResetPasswordAsync(email, token, newPassword);

        if (success)
        {
            SuccessMessage = $"✅ {message} You can now <a href='/login'>sign in</a> with your new password.";
            Console.WriteLine($"[RESET_PASSWORD] Password reset successful for: {email}");
        }
        else
        {
            ErrorMessage = $"❌ {message}";
            Console.WriteLine($"[RESET_PASSWORD] Password reset failed for: {email} - {message}");
        }

        return Page();
    }
}
