using Microsoft.Extensions.Logging;

namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Logs security-related events for audit trail with async I/O and structured logging
/// </summary>
public class SecurityAuditLogger
{
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<SecurityAuditLogger>? _logger;

    public SecurityAuditLogger(ILogger<SecurityAuditLogger>? logger = null)
    {
        _logger = logger;
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), SecurityConstants.AuditLogDirectory);
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, $"{SecurityConstants.AuditLogFilePrefix}{DateTime.UtcNow:yyyyMMdd}.log");
    }

    public async Task LogSuccessfulLoginAsync(string username, string ipAddress)
    {
        await LogEventAsync("LOGIN_SUCCESS", username, ipAddress, $"User '{username}' logged in successfully");
    }

    public async Task LogFailedLoginAsync(string username, string ipAddress, string reason)
    {
        await LogEventAsync("LOGIN_FAILED", username, ipAddress, $"Failed login attempt for '{username}': {reason}");
    }

    public async Task LogAccountLockoutAsync(string username, string ipAddress, TimeSpan duration)
    {
        await LogEventAsync("ACCOUNT_LOCKOUT", username, ipAddress, 
            $"Account '{username}' locked out for {duration.TotalMinutes:F1} minutes due to excessive failed attempts");
    }

    public async Task LogLogoutAsync(string username, string ipAddress)
    {
        await LogEventAsync("LOGOUT", username, ipAddress, $"User '{username}' logged out");
    }

    public async Task LogSessionExpiredAsync(string username, string ipAddress)
    {
        await LogEventAsync("SESSION_EXPIRED", username, ipAddress, $"Session expired for user '{username}'");
    }

    public async Task LogSuspiciousActivityAsync(string username, string ipAddress, string details)
    {
        await LogEventAsync("SUSPICIOUS_ACTIVITY", username, ipAddress, details);
    }

    public async Task LogPasswordChangeAsync(string username, string ipAddress)
    {
        await LogEventAsync("PASSWORD_CHANGE", username, ipAddress, $"Password changed for user '{username}'");
    }

    public async Task LogSettingsChangeAsync(string username, string ipAddress, string settingChanged)
    {
        await LogEventAsync("SETTINGS_CHANGE", username, ipAddress, 
            $"User '{username}' changed setting: {settingChanged}");
    }

    private async Task LogEventAsync(string eventType, string username, string ipAddress, string message)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = $"[{timestamp} UTC] [{eventType}] [User: {username}] [IP: {ipAddress}] {message}";

        // Structured logging with ILogger
        _logger?.LogInformation(
            "Security Event: {EventType} | User: {Username} | IP: {IpAddress} | Message: {Message}",
            eventType, username, ipAddress, message);

        // File logging with async I/O
        await _semaphore.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write security audit log");
            Console.WriteLine($"Failed to write security audit log: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }

        // Also log to console in debug mode
        #if DEBUG
        Console.WriteLine($"🔐 {logEntry}");
        #endif
    }

    /// <summary>
    /// Gets recent security events from the log
    /// </summary>
    public async Task<List<string>> GetRecentEventsAsync(int count = SecurityConstants.DefaultAuditLogEventCount)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_logFilePath))
                return new List<string>();

            var lines = await File.ReadAllLinesAsync(_logFilePath);
            return lines.TakeLast(count).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to read security audit log");
            return new List<string>();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
