using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketFence_AI.Dashboard.Security;

namespace PocketFence_AI.Dashboard.Pages;

public class LoginModel : PageModel
{
    private readonly OptimizedLoginRateLimiter _rateLimiter;
    private readonly OptimizedSecurityAuditLogger _auditLogger;
    private readonly UserManager _userManager;

    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public LoginModel(
        OptimizedLoginRateLimiter rateLimiter, 
        OptimizedSecurityAuditLogger auditLogger,
        UserManager userManager)
    {
        _rateLimiter = rateLimiter;
        _auditLogger = auditLogger;
        _userManager = userManager;
    }

    public void OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl ?? "/";
        
        // Check if already logged in
        if (HttpContext.Session.GetString("IsAuthenticated") == "true")
        {
            Response.Redirect(ReturnUrl);
        }
    }

    public async Task<IActionResult> OnPost(string username, string password, bool rememberMe = false, string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Validate input - prevent null/empty
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "⚠️ Both username and password are required.";
            await _auditLogger.LogFailedLoginAsync(username ?? "empty", ipAddress, "Empty credentials");
            return Page();
        }

        // SECURITY: Prevent buffer overflow and DoS attacks with length limits
        if (username.Length > SecurityConstants.MaxUsernameLength)
        {
            ErrorMessage = $"❌ Username is too long. Maximum {SecurityConstants.MaxUsernameLength} characters allowed.";
            await _auditLogger.LogSuspiciousActivityAsync(username, ipAddress, "Username too long");
            return Page();
        }
        
        if (password.Length > SecurityConstants.MaxPasswordLength)
        {
            ErrorMessage = $"❌ Password is too long. Maximum {SecurityConstants.MaxPasswordLength} characters allowed.";
            await _auditLogger.LogSuspiciousActivityAsync(username, ipAddress, "Password too long");
            return Page();
        }

        // SECURITY: Sanitize inputs - trim whitespace
        username = username.Trim();
        password = password.Trim();
        
        // SECURITY: Validate returnUrl to prevent open redirect attacks
        if (!string.IsNullOrEmpty(returnUrl) && !returnUrl.StartsWith("/"))
        {
            returnUrl = "/"; // Force local redirect only
        }

        // SECURITY: Check rate limiting (by username and IP)
        var identifier = $"{username}|{ipAddress}";
        if (_rateLimiter.IsLockedOut(identifier, out var remainingTime))
        {
            var minutes = Math.Ceiling(remainingTime?.TotalMinutes ?? 0);
            ErrorMessage = $"🔒 Account temporarily locked due to too many failed login attempts. Try again in {minutes} minute(s).";
            await _auditLogger.LogAccountLockoutAsync(username, ipAddress, remainingTime ?? TimeSpan.Zero);
            return Page();
        }

        // Authenticate user with UserManager
        var (success, message, user) = await _userManager.AuthenticateAsync(username, password);

        if (success && user != null)
        {
            // Check if email is verified
            if (!user.EmailVerified)
            {
                ErrorMessage = "⚠️ Please verify your email address before logging in. Check your inbox for the verification link.";
                await _auditLogger.LogFailedLoginAsync(username, ipAddress, "Email not verified");
                _rateLimiter.RecordFailedAttempt(identifier);
                return Page();
            }
            
            // Successful login - reset rate limiter
            _rateLimiter.ResetAttempts(identifier);
            
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
            
            await _auditLogger.LogSuccessfulLoginAsync(user.Username, ipAddress);
            
            // Redirect based on role
            if (user.Role == "Child")
            {
                return Redirect("/ChildDashboard");
            }
            
            return Redirect(ReturnUrl);
        }

        // Failed login - record attempt
        _rateLimiter.RecordFailedAttempt(identifier);
        var remaining = _rateLimiter.GetRemainingAttempts(identifier);
        
        if (remaining > 0)
        {
            ErrorMessage = $"❌ Login failed. Please check your username and password. ({remaining} attempt(s) remaining)";
        }
        else
        {
            ErrorMessage = "🔒 Too many failed attempts. Account temporarily locked.";
        }
        
        await _auditLogger.LogFailedLoginAsync(username, ipAddress, "Invalid credentials");
        
        return Page();
    }
}
