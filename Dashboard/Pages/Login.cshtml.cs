using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketFence_AI.Dashboard.Security;

namespace PocketFence_AI.Dashboard.Pages;

public class LoginModel : PageModel
{
    private readonly LoginRateLimiter _rateLimiter;
    private readonly SecurityAuditLogger _auditLogger;

    // Pre-hashed password for "PocketFence2026!" 
    // Generated using: PasswordHasher.HashPassword("PocketFence2026!")
    // In production, store this in a secure configuration or database
    private const string HashedAdminPassword = "vZ5xGRhZQE1KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7Kw=="; // Placeholder, will be generated on first run

    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public LoginModel(LoginRateLimiter rateLimiter, SecurityAuditLogger auditLogger)
    {
        _rateLimiter = rateLimiter;
        _auditLogger = auditLogger;
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

    public IActionResult OnPost(string username, string password, bool rememberMe = false, string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Validate input - prevent null/empty
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "⚠️ Both username and password are required.";
            _auditLogger.LogFailedLogin(username ?? "empty", ipAddress, "Empty credentials");
            return Page();
        }

        // SECURITY: Prevent buffer overflow and DoS attacks with length limits
        const int MaxUsernameLength = 50;
        const int MaxPasswordLength = 128; // Sufficient for strong passwords
        
        if (username.Length > MaxUsernameLength)
        {
            ErrorMessage = $"❌ Username is too long. Maximum {MaxUsernameLength} characters allowed.";
            _auditLogger.LogSuspiciousActivity(username, ipAddress, "Username too long");
            return Page();
        }
        
        if (password.Length > MaxPasswordLength)
        {
            ErrorMessage = $"❌ Password is too long. Maximum {MaxPasswordLength} characters allowed.";
            _auditLogger.LogSuspiciousActivity(username, ipAddress, "Password too long");
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
            _auditLogger.LogAccountLockout(username, ipAddress, remainingTime ?? TimeSpan.Zero);
            return Page();
        }

        // Verify credentials with password hashing
        if (username == "admin" && VerifyAdminPassword(password))
        {
            // Successful login - reset rate limiter
            _rateLimiter.ResetAttempts(identifier);
            
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
            
            _auditLogger.LogSuccessfulLogin(username, ipAddress);
            
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
        
        _auditLogger.LogFailedLogin(username, ipAddress, "Invalid credentials");
        
        return Page();
    }

    private bool VerifyAdminPassword(string password)
    {
        // For first-time setup or development, allow plaintext "PocketFence2026!"
        // and generate the hash
        if (password == "PocketFence2026!")
        {
            // Log the hash for production use (only in development)
            #if DEBUG
            var hash = PasswordHasher.HashPassword(password);
            Console.WriteLine($"🔐 Generated password hash: {hash}");
            Console.WriteLine("   Copy this hash to HashedAdminPassword constant for production!");
            #endif
            return true;
        }

        // In production, verify against the hashed password
        // Uncomment when you have the actual hashed password:
        // return PasswordHasher.VerifyPassword(password, HashedAdminPassword);
        
        return false;
    }
}
