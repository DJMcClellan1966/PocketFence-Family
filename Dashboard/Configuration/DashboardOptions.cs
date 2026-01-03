namespace PocketFence_AI.Dashboard.Configuration;

/// <summary>
/// Configuration options for the dashboard
/// </summary>
public class DashboardOptions
{
    public const string SectionName = "Dashboard";
    
    public SecurityOptions Security { get; set; } = new();
    public RateLimitOptions RateLimit { get; set; } = new();
    public SessionOptions Session { get; set; } = new();
}

public class SecurityOptions
{
    public int MaxUsernameLength { get; set; } = 50;
    public int MaxPasswordLength { get; set; } = 128;
    public int PasswordHashIterations { get; set; } = 100000;
    public string AdminUsername { get; set; } = "admin";
    public string AdminPasswordHash { get; set; } = ""; // Set via configuration
}

public class RateLimitOptions
{
    public int MaxAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int AttemptWindowMinutes { get; set; } = 5;
    public int CleanupIntervalMinutes { get; set; } = 5;
}

public class SessionOptions
{
    public int TimeoutMinutes { get; set; } = 30;
    public int WarningMinutes { get; set; } = 5;
}
