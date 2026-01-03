namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Centralized security configuration constants
/// </summary>
public static class SecurityConstants
{
    // Input Validation Limits
    public const int MaxUsernameLength = 50;
    public const int MaxPasswordLength = 128;
    public const int MaxEmailLength = 254; // RFC 5321 maximum
    
    // Password Hashing Parameters
    public const int PasswordSaltSize = 16; // 128 bits
    public const int PasswordKeySize = 32; // 256 bits
    public const int PasswordHashIterations = 100000; // OWASP recommended
    
    // Rate Limiting Configuration
    public const int RateLimitMaxAttempts = 5;
    public const int RateLimitLockoutMinutes = 15;
    public const int RateLimitAttemptWindowMinutes = 5;
    public const int RateLimitCleanupIntervalMinutes = 5;
    
    // Session Configuration
    public const int SessionTimeoutMinutes = 30;
    public const int SessionWarningMinutes = 5;
    
    // Audit Log Configuration
    public const int DefaultAuditLogEventCount = 100;
    public const string AuditLogDirectory = "Logs";
    public const string AuditLogFilePrefix = "security_audit_";
}
