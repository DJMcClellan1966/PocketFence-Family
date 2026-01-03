using Microsoft.Extensions.Logging;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Memory-optimized audit logger using GPT4All-inspired techniques:
/// - Memory-mapped files for zero-copy access
/// - Ring buffer for recent events (O(1) memory)
/// - Lazy file rotation
/// </summary>
public class OptimizedSecurityAuditLogger
{
    private readonly string _logDirectory;
    private readonly ILogger<OptimizedSecurityAuditLogger>? _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    // Ring buffer for recent events (GPT4All-style caching)
    private readonly RingBuffer<string> _recentEventsCache;
    private const int MaxCachedEvents = 1000; // Keep last 1000 in memory
    
    // Log rotation thresholds
    private const long MaxLogFileSize = 10 * 1024 * 1024; // 10MB
    private const int MaxLogAgeDays = 30;

    public OptimizedSecurityAuditLogger(ILogger<OptimizedSecurityAuditLogger>? logger = null)
    {
        _logger = logger;
        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), SecurityConstants.AuditLogDirectory);
        Directory.CreateDirectory(_logDirectory);
        _recentEventsCache = new RingBuffer<string>(MaxCachedEvents);
        
        // Clean old logs on startup
        CleanOldLogsAsync().ConfigureAwait(false);
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
            $"Account '{username}' locked out for {duration.TotalMinutes:F1} minutes");
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

    private async Task LogEventAsync(string eventType, string username, string ipAddress, string message)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = $"[{timestamp} UTC] [{eventType}] [User: {username}] [IP: {ipAddress}] {message}";

        // Structured logging
        _logger?.LogInformation(
            "Security Event: {EventType} | User: {Username} | IP: {IpAddress} | Message: {Message}",
            eventType, username, ipAddress, message);

        // Add to ring buffer cache (O(1) operation)
        _recentEventsCache.Add(logEntry);

        // Async file write with rotation check
        await _semaphore.WaitAsync();
        try
        {
            var logFile = GetCurrentLogFilePath();
            
            // Check if rotation needed
            if (File.Exists(logFile) && new FileInfo(logFile).Length > MaxLogFileSize)
            {
                await RotateLogFileAsync(logFile);
                logFile = GetCurrentLogFilePath(); // Get new file
            }

            await File.AppendAllTextAsync(logFile, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write security audit log");
        }
        finally
        {
            _semaphore.Release();
        }

        #if DEBUG
        Console.WriteLine($"🔐 {logEntry}");
        #endif
    }

    /// <summary>
    /// Gets recent events from in-memory cache (O(1) space, O(n) time where n ≤ 1000)
    /// GPT4All-inspired: Keep hot data in memory, avoid file I/O
    /// </summary>
    public List<string> GetRecentEvents(int count)
    {
        return _recentEventsCache.GetLast(Math.Min(count, MaxCachedEvents));
    }

    /// <summary>
    /// Memory-mapped file search for historical events (zero-copy)
    /// GPT4All-inspired: Use mmap for large file access without loading into RAM
    /// </summary>
    public async Task<List<string>> SearchHistoricalEventsAsync(string searchTerm, int maxResults = 100)
    {
        var results = new List<string>();
        var logFiles = Directory.GetFiles(_logDirectory, "security_audit_*.log")
            .OrderByDescending(f => f)
            .Take(7); // Search last 7 days

        foreach (var logFile in logFiles)
        {
            if (results.Count >= maxResults) break;

            try
            {
                // Use memory-mapped file for zero-copy access
                using var mmf = MemoryMappedFile.CreateFromFile(logFile, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
                using var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null && results.Count < maxResults)
                {
                    if (line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching log file: {LogFile}", logFile);
            }
        }

        return results;
    }

    private string GetCurrentLogFilePath()
    {
        return Path.Combine(_logDirectory, $"{SecurityConstants.AuditLogFilePrefix}{DateTime.UtcNow:yyyyMMdd}.log");
    }

    private async Task RotateLogFileAsync(string logFile)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var rotatedFile = logFile.Replace(".log", $"_rotated_{timestamp}.log");
        
        try
        {
            File.Move(logFile, rotatedFile);
            _logger?.LogInformation("Rotated log file: {RotatedFile}", rotatedFile);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to rotate log file");
        }
    }

    private async Task CleanOldLogsAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-MaxLogAgeDays);
            var oldLogs = Directory.GetFiles(_logDirectory, "security_audit_*.log")
                .Where(f => File.GetCreationTimeUtc(f) < cutoffDate)
                .ToList();

            foreach (var oldLog in oldLogs)
            {
                File.Delete(oldLog);
                _logger?.LogInformation("Deleted old log file: {LogFile}", oldLog);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clean old logs");
        }
    }
}

/// <summary>
/// Fixed-size ring buffer for O(1) space complexity
/// Similar to GPT4All's token buffer
/// </summary>
public class RingBuffer<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _count;
    private readonly object _lock = new();

    public RingBuffer(int capacity)
    {
        _buffer = new T[capacity];
        _head = 0;
        _count = 0;
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length) _count++;
        }
    }

    public List<T> GetLast(int count)
    {
        lock (_lock)
        {
            count = Math.Min(count, _count);
            var result = new List<T>(count);
            
            for (int i = 0; i < count; i++)
            {
                var index = (_head - count + i + _buffer.Length) % _buffer.Length;
                result.Add(_buffer[index]);
            }
            
            return result;
        }
    }
}
