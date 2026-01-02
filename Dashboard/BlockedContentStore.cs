using System.Text.Json;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Stores and retrieves blocked content history
/// </summary>
public class BlockedContentStore
{
    private const string DataFile = "FamilyData/blocked_content.json";
    private static readonly object _lock = new();
    private List<BlockedContentEntry> _entries = new();

    public BlockedContentStore()
    {
        EnsureDataDirectory();
        LoadFromDisk();
    }

    private void EnsureDataDirectory()
    {
        var directory = Path.GetDirectoryName(DataFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void AddBlock(string content, string reason, string category = "Other")
    {
        lock (_lock)
        {
            var entry = new BlockedContentEntry
            {
                Timestamp = DateTime.Now,
                Content = content,
                Reason = reason,
                Category = DetermineCategory(reason, category)
            };

            _entries.Add(entry);
            SaveToDisk();
        }
    }

    public List<BlockedContentEntry> GetAll()
    {
        lock (_lock)
        {
            return _entries.OrderByDescending(e => e.Timestamp).ToList();
        }
    }

    public List<BlockedContentEntry> GetRecent(int count)
    {
        lock (_lock)
        {
            return _entries
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    public int GetBlockedToday()
    {
        var today = DateTime.Today;
        lock (_lock)
        {
            return _entries.Count(e => e.Timestamp.Date == today);
        }
    }

    public int GetBlockedThisWeek()
    {
        var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        lock (_lock)
        {
            return _entries.Count(e => e.Timestamp >= weekStart);
        }
    }

    public int GetBlockedThisMonth()
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        lock (_lock)
        {
            return _entries.Count(e => e.Timestamp >= monthStart);
        }
    }

    public int GetBlockedAllTime()
    {
        lock (_lock)
        {
            return _entries.Count;
        }
    }

    public Dictionary<string, int> GetBlocksByCategory()
    {
        lock (_lock)
        {
            return _entries
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    private string DetermineCategory(string reason, string defaultCategory)
    {
        var reasonLower = reason.ToLowerInvariant();

        if (reasonLower.Contains("adult") || reasonLower.Contains("explicit"))
            return "Adult Content";
        if (reasonLower.Contains("violence") || reasonLower.Contains("violent"))
            return "Violence";
        if (reasonLower.Contains("hate") || reasonLower.Contains("racist"))
            return "Hate Speech";
        if (reasonLower.Contains("hack") || reasonLower.Contains("dangerous"))
            return "Dangerous Activities";
        if (reasonLower.Contains("gambling") || reasonLower.Contains("casino"))
            return "Gambling";
        if (reasonLower.Contains("drug") || reasonLower.Contains("substance"))
            return "Drugs";
        if (reasonLower.Contains("malware") || reasonLower.Contains("phishing"))
            return "Malware/Phishing";

        return defaultCategory;
    }

    private void LoadFromDisk()
    {
        try
        {
            if (File.Exists(DataFile))
            {
                var json = File.ReadAllText(DataFile);
                _entries = JsonSerializer.Deserialize<List<BlockedContentEntry>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Warning: Could not load blocked content history: {ex.Message}");
            _entries = new();
        }
    }

    private void SaveToDisk()
    {
        try
        {
            var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(DataFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Warning: Could not save blocked content: {ex.Message}");
        }
    }
}

public class BlockedContentEntry
{
    public DateTime Timestamp { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";
}
