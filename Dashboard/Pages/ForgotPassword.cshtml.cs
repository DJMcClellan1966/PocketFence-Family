using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager _userManager;
    private readonly EmailService _emailService;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public ForgotPasswordModel(UserManager userManager, EmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "⚠️ Please enter your email address.";
            return Page();
        }

        email = email.Trim();

        // Generate reset token
        var (success, message, token) = await _userManager.GeneratePasswordResetTokenAsync(email);

        if (success && token != null)
        {
            var user = await _userManager.GetUserByEmailAsync(email);
            if (user != null)
            {
                // Send reset email
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, token);
            }
        }

        // Always show success message (even if email doesn't exist) to prevent email enumeration
        SuccessMessage = "✅ If an account exists with that email, a password reset link has been sent. Please check your inbox and spam folder.";
        Console.WriteLine($"[FORGOT_PASSWORD] Reset requested for: {email}");

        return Page();
    }
}
