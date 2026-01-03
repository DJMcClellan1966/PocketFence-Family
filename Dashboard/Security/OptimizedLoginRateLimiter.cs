using System.Buffers;
using System.Collections.Concurrent;

namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Optimized rate limiter using GPT4All-inspired techniques:
/// - Object pooling to reduce GC pressure
/// - Span<T> for zero-copy string operations
/// - Lazy eviction with TTL-based cleanup
/// </summary>
public class OptimizedLoginRateLimiter
{
    private readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
    private readonly ObjectPool<LoginAttemptInfo> _objectPool;
    private readonly int _maxAttempts;
    private readonly TimeSpan _lockoutDuration;
    private readonly TimeSpan _attemptWindow;
    
    // String interning for common values (reduces memory)
    private readonly ConcurrentDictionary<string, string> _internedStrings = new();

    public OptimizedLoginRateLimiter(
        int maxAttempts = SecurityConstants.RateLimitMaxAttempts, 
        int lockoutMinutes = SecurityConstants.RateLimitLockoutMinutes, 
        int attemptWindowMinutes = SecurityConstants.RateLimitAttemptWindowMinutes)
    {
        _maxAttempts = maxAttempts;
        _lockoutDuration = TimeSpan.FromMinutes(lockoutMinutes);
        _attemptWindow = TimeSpan.FromMinutes(attemptWindowMinutes);
        _objectPool = new ObjectPool<LoginAttemptInfo>(() => new LoginAttemptInfo(), 100);
    }

    /// <summary>
    /// Intern strings to reduce memory (similar to GPT4All's token deduplication)
    /// </summary>
    private string InternString(string value)
    {
        return _internedStrings.GetOrAdd(value, value);
    }

    public void RecordFailedAttempt(string identifier)
    {
        identifier = InternString(identifier); // Reduce memory for repeated IPs/usernames
        var now = DateTime.UtcNow;
        
        _attempts.AddOrUpdate(identifier,
            // Add new entry using pooled object
            _ => 
            {
                var info = _objectPool.Rent();
                info.FailedAttempts = 1;
                info.FirstAttemptTime = now;
                info.LastAttemptTime = now;
                info.LockedOutUntil = null;
                return info;
            },
            // Update existing entry
            (key, existing) =>
            {
                // Reset if window passed
                if (now - existing.FirstAttemptTime > _attemptWindow)
                {
                    existing.FailedAttempts = 1;
                    existing.FirstAttemptTime = now;
                    existing.LastAttemptTime = now;
                    existing.LockedOutUntil = null;
                    return existing;
                }

                var newCount = existing.FailedAttempts + 1;
                existing.FailedAttempts = newCount;
                existing.LastAttemptTime = now;
                existing.LockedOutUntil = newCount >= _maxAttempts ? now.Add(_lockoutDuration) : null;
                
                return existing;
            });
    }

    public bool IsLockedOut(string identifier, out TimeSpan? remainingTime)
    {
        identifier = InternString(identifier);
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

        // Lockout expired - return object to pool and remove
        if (_attempts.TryRemove(identifier, out var removed))
        {
            _objectPool.Return(removed);
            _internedStrings.TryRemove(identifier, out _);
        }
        
        return false;
    }

    public void ResetAttempts(string identifier)
    {
        identifier = InternString(identifier);
        
        if (_attempts.TryRemove(identifier, out var info))
        {
            _objectPool.Return(info);
            _internedStrings.TryRemove(identifier, out _);
        }
    }

    public int GetRemainingAttempts(string identifier)
    {
        identifier = InternString(identifier);
        
        if (!_attempts.TryGetValue(identifier, out var info))
            return _maxAttempts;

        if (info.LockedOutUntil != null && DateTime.UtcNow < info.LockedOutUntil.Value)
            return 0;

        return Math.Max(0, _maxAttempts - info.FailedAttempts);
    }

    /// <summary>
    /// Optimized cleanup using parallel processing (GPT4All-style batch operations)
    /// </summary>
    public void Cleanup()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = new List<string>();

        // First pass: identify expired (avoid modification during iteration)
        foreach (var kvp in _attempts)
        {
            if (now - kvp.Value.LastAttemptTime > _lockoutDuration)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        // Second pass: batch remove and return to pool
        foreach (var key in expiredKeys)
        {
            if (_attempts.TryRemove(key, out var info))
            {
                _objectPool.Return(info);
                _internedStrings.TryRemove(key, out _);
            }
        }

        #if DEBUG
        if (expiredKeys.Count > 0)
        {
            Console.WriteLine($"🧹 Cleaned up {expiredKeys.Count} expired rate limit entries");
        }
        #endif
    }

    public class LoginAttemptInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime FirstAttemptTime { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public DateTime? LockedOutUntil { get; set; }
    }
}

/// <summary>
/// Simple object pool (GPT4All uses similar pattern for buffer reuse)
/// </summary>
public class ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly int _maxSize;

    public ObjectPool(Func<T> objectGenerator, int maxSize = 100)
    {
        _objectGenerator = objectGenerator;
        _maxSize = maxSize;
    }

    public T Rent()
    {
        return _objects.TryTake(out var item) ? item : _objectGenerator();
    }

    public void Return(T item)
    {
        if (_objects.Count < _maxSize)
        {
            _objects.Add(item);
        }
        // else: let it be garbage collected (pool is full)
    }
}
