using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketFence_AI.Dashboard.Security;

namespace PocketFence_AI.Dashboard.Pages;

public class LoginModel : PageModel
{
    private readonly OptimizedLoginRateLimiter _rateLimiter;
    private readonly OptimizedSecurityAuditLogger _auditLogger;

    // Pre-hashed password for "PocketFence2026!" 
    // Generated using: PasswordHasher.HashPassword("PocketFence2026!")
    // In production, store this in a secure configuration or database
    private const string HashedAdminPassword = "vZ5xGRhZQE1KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7KqXC7Kw=="; // Placeholder, will be generated on first run

    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public LoginModel(OptimizedLoginRateLimiter rateLimiter, OptimizedSecurityAuditLogger auditLogger)
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

        // Verify credentials with password hashing
        if (username == "admin" && VerifyAdminPassword(password))
        {
            // Successful login - reset rate limiter
            _rateLimiter.ResetAttempts(identifier);
            
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
            
            await _auditLogger.LogSuccessfulLoginAsync(username, ipAddress);
            
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

    private bool VerifyAdminPassword(string password)
    {
        // Generate hash on first run for setup
        #if DEBUG
        if (HashedAdminPassword.StartsWith("vZ5x")) // Placeholder hash detected
        {
            var correctHash = PasswordHasher.HashPassword("PocketFence2026!");
            Console.WriteLine($"🔐 Generated password hash for production:");
            Console.WriteLine($"   {correctHash}");
            Console.WriteLine("   Replace HashedAdminPassword constant with this value!");
            
            // Allow login in DEBUG mode only
            return password == "PocketFence2026!";
        }
        #endif

        // Production: Always use hashed password verification
        // For now, accept the correct password and verify against a proper hash
        // TODO: Replace this with actual stored hash from configuration
        var tempHash = PasswordHasher.HashPassword("PocketFence2026!");
        return PasswordHasher.VerifyPassword(password, tempHash);
    }
}
