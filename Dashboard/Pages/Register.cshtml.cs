using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager _userManager;
    private readonly EmailService _emailService;
    private readonly AiSmsService _smsService;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public RegisterModel(UserManager userManager, EmailService emailService, AiSmsService smsService)
    {
        _userManager = userManager;
        _emailService = emailService;
        _smsService = smsService;
    }

    public void OnGet()
    {
        // Check if already logged in
        if (HttpContext.Session.GetString("IsAuthenticated") == "true")
        {
            Response.Redirect("/");
        }
    }

    public async Task<IActionResult> OnPost(
        string username, 
        string email, 
        string password, 
        string confirmPassword,
        string role,
        string? phoneNumber,
        string? countryCode,
        string? carrier)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Validate inputs
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
        {
            ErrorMessage = "⚠️ All required fields must be filled out.";
            return Page();
        }

        if (role != "Parent" && role != "Child")
        {
            ErrorMessage = "⚠️ Please select a valid account type.";
            return Page();
        }

        if (password != confirmPassword)
        {
            ErrorMessage = "❌ Passwords do not match.";
            return Page();
        }

        // Combine country code with phone number
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var code = string.IsNullOrWhiteSpace(countryCode) ? "+1" : countryCode;
            phoneNumber = $"{code}{phoneNumber.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")}";
        }

        // Sanitize inputs
        username = username.Trim();
        email = email.Trim();
        phoneNumber = phoneNumber?.Trim();

        // Create user with specified role
        var (success, message, user) = await _userManager.CreateUserAsync(
            username, password, email, phoneNumber, role);

        if (!success)
        {
            ErrorMessage = $"❌ {message}";
            Console.WriteLine($"[REGISTER] Failed registration for {username}: {message} from IP: {ipAddress}");
            return Page();
        }

        // Success - send verification email and redirect to login
        Console.WriteLine($"[REGISTER] New user registered: {username} ({email}) from IP: {ipAddress}");

        // Send verification email with token
        if (user != null && user.EmailVerificationToken != null)
        {
            await _emailService.SendVerificationEmailAsync(user.Email, user.Username, user.EmailVerificationToken);
        }

        // Send phone verification code if phone provided
        if (user != null && !string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneVerificationCode != null)
        {
            await _smsService.SendVerificationCodeAsync(user.PhoneNumber, user.PhoneVerificationCode, carrier);
            Console.WriteLine($"[REGISTER] Phone verification code sent to {AiSmsService.FormatPhoneNumber(user.PhoneNumber)}" + 
                             (string.IsNullOrEmpty(carrier) ? " (all carriers)" : $" ({carrier})"));
        }

        // Redirect to login page with success message
        var verificationMessage = !string.IsNullOrEmpty(phoneNumber) 
            ? $"Please check your email ({email}) and phone for verification codes, then sign in below."
            : $"Please check your email ({email}) to verify your account, then sign in below.";
        TempData["SuccessMessage"] = $"✅ Account created successfully! {verificationMessage}";
        return RedirectToPage("/Login");
    }
}
