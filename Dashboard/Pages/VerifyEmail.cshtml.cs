using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class VerifyEmailModel : PageModel
{
    private readonly UserManager _userManager;

    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public VerifyEmailModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            Success = false;
            ErrorMessage = "Invalid verification link. Email or token is missing.";
            return Page();
        }

        // Verify the email with the token
        var (success, message) = await _userManager.VerifyEmailAsync(email, token);

        Success = success;
        if (!success)
        {
            ErrorMessage = message;
        }

        if (success)
        {
            Console.WriteLine($"[EMAIL_VERIFIED] Email verified successfully: {email}");
        }
        else
        {
            Console.WriteLine($"[EMAIL_VERIFICATION_FAILED] Failed to verify {email}: {message}");
        }

        return Page();
    }
}
