namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Logs security-related events for audit trail
/// </summary>
public class SecurityAuditLogger
{
    private readonly string _logFilePath;
    private readonly object _lockObj = new();

    public SecurityAuditLogger()
    {
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, $"security_audit_{DateTime.UtcNow:yyyyMMdd}.log");
    }

    public void LogSuccessfulLogin(string username, string ipAddress)
    {
        LogEvent("LOGIN_SUCCESS", username, ipAddress, $"User '{username}' logged in successfully");
    }

    public void LogFailedLogin(string username, string ipAddress, string reason)
    {
        LogEvent("LOGIN_FAILED", username, ipAddress, $"Failed login attempt for '{username}': {reason}");
    }

    public void LogAccountLockout(string username, string ipAddress, TimeSpan duration)
    {
        LogEvent("ACCOUNT_LOCKOUT", username, ipAddress, 
            $"Account '{username}' locked out for {duration.TotalMinutes:F1} minutes due to excessive failed attempts");
    }

    public void LogLogout(string username, string ipAddress)
    {
        LogEvent("LOGOUT", username, ipAddress, $"User '{username}' logged out");
    }

    public void LogSessionExpired(string username, string ipAddress)
    {
        LogEvent("SESSION_EXPIRED", username, ipAddress, $"Session expired for user '{username}'");
    }

    public void LogSuspiciousActivity(string username, string ipAddress, string details)
    {
        LogEvent("SUSPICIOUS_ACTIVITY", username, ipAddress, details);
    }

    public void LogPasswordChange(string username, string ipAddress)
    {
        LogEvent("PASSWORD_CHANGE", username, ipAddress, $"Password changed for user '{username}'");
    }

    public void LogSettingsChange(string username, string ipAddress, string settingChanged)
    {
        LogEvent("SETTINGS_CHANGE", username, ipAddress, 
            $"User '{username}' changed setting: {settingChanged}");
    }

    private void LogEvent(string eventType, string username, string ipAddress, string message)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = $"[{timestamp} UTC] [{eventType}] [User: {username}] [IP: {ipAddress}] {message}";

        lock (_lockObj)
        {
            try
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write security audit log: {ex.Message}");
            }
        }

        // Also log to console in debug mode
        #if DEBUG
        Console.WriteLine($"🔐 {logEntry}");
        #endif
    }

    /// <summary>
    /// Gets recent security events from the log
    /// </summary>
    public List<string> GetRecentEvents(int count = 100)
    {
        lock (_lockObj)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                    return new List<string>();

                var lines = File.ReadAllLines(_logFilePath);
                return lines.TakeLast(count).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
