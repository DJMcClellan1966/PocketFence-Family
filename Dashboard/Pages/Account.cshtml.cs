using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class AccountModel : AuthenticatedPageModel
{
    private readonly UserManager _userManager;
    private readonly EmailService _emailService;
    private readonly AiSmsService _smsService;

    public string? CurrentUsername { get; set; }
    public string? CurrentEmail { get; set; }
    public string? CurrentPhone { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public new string? Role { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public AccountModel(UserManager userManager, EmailService emailService, AiSmsService smsService)
    {
        _userManager = userManager;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync(string? newUsername)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);

        if (string.IsNullOrWhiteSpace(newUsername))
        {
            ErrorMessage = "❌ Please enter a new username.";
            return Page();
        }

        var (success, message) = await _userManager.UpdateUsernameAsync(user.Id, newUsername);
        
        if (success)
        {
            // Update session with new username
            HttpContext.Session.SetString("Username", newUsername);
            SuccessMessage = $"✅ {message}";
            CurrentUsername = newUsername;
        }
        else
        {
            ErrorMessage = $"❌ {message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(
        string currentPassword, string newPassword, string confirmNewPassword)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            ErrorMessage = "❌ All password fields are required.";
            return Page();
        }

        if (newPassword != confirmNewPassword)
        {
            ErrorMessage = "❌ New passwords do not match.";
            return Page();
        }

        var (success, message) = await _userManager.UpdatePasswordAsync(user.Id, currentPassword, newPassword);
        
        if (success)
        {
            SuccessMessage = $"✅ {message}";
        }
        else
        {
            ErrorMessage = $"❌ {message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateEmailAsync(string? newEmail)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);

        if (string.IsNullOrWhiteSpace(newEmail))
        {
            ErrorMessage = "❌ Please enter a new email address.";
            return Page();
        }

        var (success, message) = await _userManager.UpdateEmailAsync(user.Id, newEmail);
        
        if (success)
        {
            // Send verification email
            var updatedUser = await _userManager.GetUserByIdAsync(user.Id);
            if (updatedUser?.EmailVerificationToken != null)
            {
                await _emailService.SendVerificationEmailAsync(
                    updatedUser.Email, 
                    updatedUser.Username, 
                    updatedUser.EmailVerificationToken);
            }

            SuccessMessage = $"✅ {message}";
            CurrentEmail = newEmail;
            EmailVerified = false;
        }
        else
        {
            ErrorMessage = $"❌ {message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostResendVerificationAsync()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);

        if (user.EmailVerified)
        {
            SuccessMessage = "✅ Email is already verified.";
            return Page();
        }

        if (user.EmailVerificationToken != null)
        {
            await _emailService.SendVerificationEmailAsync(
                user.Email, 
                user.Username, 
                user.EmailVerificationToken);
            SuccessMessage = "✅ Verification email sent! Please check your inbox.";
        }
        else
        {
            ErrorMessage = "❌ Unable to send verification email.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdatePhoneAsync(string? newPhone, string? phoneCountryCode, string? carrier)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);

        if (string.IsNullOrWhiteSpace(newPhone))
        {
            ErrorMessage = "❌ Phone number is required";
            return Page();
        }

        // Combine country code with phone number
        var code = string.IsNullOrWhiteSpace(phoneCountryCode) ? "+1" : phoneCountryCode;
        var fullPhone = $"{code}{newPhone.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")}";

        var (success, message, verificationCode) = await _userManager.UpdatePhoneAsync(user.Id, fullPhone);
        
        if (success && verificationCode != null)
        {
            // Send verification code via SMS
            await _smsService.SendVerificationCodeAsync(fullPhone, verificationCode, carrier);
            var carrierInfo = string.IsNullOrEmpty(carrier) ? "" : $" ({carrier})";
            SuccessMessage = $"✅ Phone updated! Verification code sent to {AiSmsService.FormatPhoneNumber(fullPhone)}{carrierInfo}";
            
            // Reload user data to show updated phone
            user = await _userManager.GetUserByUsernameAsync(username);
            if (user != null)
                LoadUserData(user);
        }
        else
        {
            ErrorMessage = $"❌ {message}";
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostResendPhoneVerificationAsync()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        LoadUserData(user);

        if (user.PhoneVerified)
        {
            SuccessMessage = "✅ Phone is already verified.";
            return Page();
        }

        if (string.IsNullOrEmpty(user.PhoneNumber))
        {
            ErrorMessage = "❌ No phone number on file";
            return Page();
        }

        var (success, message, code) = await _userManager.GeneratePhoneVerificationCodeAsync(user.Id);
        
        if (success && code != null)
        {
            await _smsService.SendVerificationCodeAsync(user.PhoneNumber, code);
            SuccessMessage = "✅ Verification code sent! Please check your phone.";
        }
        else
        {
            ErrorMessage = $"❌ {message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login");

        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
            return RedirectToPage("/Login");

        var (success, message) = await _userManager.DeleteUserAsync(user.Id);
        
        if (success)
        {
            // Clear session and redirect to login
            HttpContext.Session.Clear();
            return RedirectToPage("/Login", new { message = "Account deleted successfully" });
        }
        else
        {
            LoadUserData(user);
            ErrorMessage = $"❌ {message}";
            return Page();
        }
    }

    private void LoadUserData(User user)
    {
        CurrentUsername = user.Username;
        CurrentEmail = user.Email;
        CurrentPhone = user.PhoneNumber;
        EmailVerified = user.EmailVerified;
        PhoneVerified = user.PhoneVerified;
        CreatedAt = user.CreatedAt;
        LastLogin = user.LastLoginAt;
        Role = user.Role;
    }
}
