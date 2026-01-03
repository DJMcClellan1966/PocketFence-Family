using System.Collections.Concurrent;

namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Implements rate limiting for login attempts to prevent brute force attacks
/// </summary>
public class LoginRateLimiter
{
    private readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
    private readonly int _maxAttempts;
    private readonly TimeSpan _lockoutDuration;
    private readonly TimeSpan _attemptWindow;

    public LoginRateLimiter(int maxAttempts = 5, int lockoutMinutes = 15, int attemptWindowMinutes = 5)
    {
        _maxAttempts = maxAttempts;
        _lockoutDuration = TimeSpan.FromMinutes(lockoutMinutes);
        _attemptWindow = TimeSpan.FromMinutes(attemptWindowMinutes);
    }

    /// <summary>
    /// Records a failed login attempt for an identifier (username or IP)
    /// </summary>
    public void RecordFailedAttempt(string identifier)
    {
        var now = DateTime.UtcNow;
        
        _attempts.AddOrUpdate(identifier,
            // Add new entry
            new LoginAttemptInfo
            {
                FailedAttempts = 1,
                FirstAttemptTime = now,
                LastAttemptTime = now,
                LockedOutUntil = null
            },
            // Update existing entry
            (key, existing) =>
            {
                // Reset if attempt window has passed
                if (now - existing.FirstAttemptTime > _attemptWindow)
                {
                    return new LoginAttemptInfo
                    {
                        FailedAttempts = 1,
                        FirstAttemptTime = now,
                        LastAttemptTime = now,
                        LockedOutUntil = null
                    };
                }

                var newAttemptCount = existing.FailedAttempts + 1;
                
                return new LoginAttemptInfo
                {
                    FailedAttempts = newAttemptCount,
                    FirstAttemptTime = existing.FirstAttemptTime,
                    LastAttemptTime = now,
                    LockedOutUntil = newAttemptCount >= _maxAttempts 
                        ? now.Add(_lockoutDuration) 
                        : null
                };
            });
    }

    /// <summary>
    /// Checks if an identifier is currently locked out
    /// </summary>
    public bool IsLockedOut(string identifier, out TimeSpan? remainingTime)
    {
        remainingTime = null;

        if (!_attempts.TryGetValue(identifier, out var info))
            return false;

        if (info.LockedOutUntil == null)
            return false;

        var now = DateTime.UtcNow;
        
        if (now < info.LockedOutUntil.Value)
        {
            remainingTime = info.LockedOutUntil.Value - now;
            return true;
        }

        // Lockout expired, clean up
        _attempts.TryRemove(identifier, out _);
        return false;
    }

    /// <summary>
    /// Resets attempts for a successful login
    /// </summary>
    public void ResetAttempts(string identifier)
    {
        _attempts.TryRemove(identifier, out _);
    }

    /// <summary>
    /// Gets the number of remaining attempts before lockout
    /// </summary>
    public int GetRemainingAttempts(string identifier)
    {
        if (!_attempts.TryGetValue(identifier, out var info))
            return _maxAttempts;

        if (info.LockedOutUntil != null && DateTime.UtcNow < info.LockedOutUntil.Value)
            return 0;

        var remaining = _maxAttempts - info.FailedAttempts;
        return Math.Max(0, remaining);
    }

    /// <summary>
    /// Cleans up old entries periodically
    /// </summary>
    public void Cleanup()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _attempts
            .Where(kvp => now - kvp.Value.LastAttemptTime > _lockoutDuration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _attempts.TryRemove(key, out _);
        }
    }

    private class LoginAttemptInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime FirstAttemptTime { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public DateTime? LockedOutUntil { get; set; }
    }
}
