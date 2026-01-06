using System.Text.Json;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Stores anonymous feedback from parents about recommendations
/// </summary>
public class FeedbackStore
{
    private readonly string _dataPath;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public FeedbackStore()
    {
        _dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "feedback.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_dataPath)!);
    }

    public async Task SaveFeedbackAsync(RecommendationFeedback feedback)
    {
        await _fileLock.WaitAsync();
        try
        {
            // Load existing feedback
            var allFeedback = await LoadAllFeedbackAsync();
            
            // Add new feedback
            allFeedback.Add(feedback);
            
            // Save back to file
            var json = JsonSerializer.Serialize(allFeedback, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(_dataPath, json);
            
            Console.WriteLine($"✅ Feedback saved: {feedback.Id} (Rating: {feedback.HelpfulnessRating}/5, Implemented: {feedback.ImplementedRecommendations.Count})");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<List<RecommendationFeedback>> LoadAllFeedbackAsync()
    {
        if (!File.Exists(_dataPath))
            return new List<RecommendationFeedback>();

        try
        {
            var json = await File.ReadAllTextAsync(_dataPath);
            return JsonSerializer.Deserialize<List<RecommendationFeedback>>(json) 
                   ?? new List<RecommendationFeedback>();
        }
        catch
        {
            return new List<RecommendationFeedback>();
        }
    }

    /// <summary>
    /// Get feedback statistics for monitoring
    /// </summary>
    public async Task<FeedbackStats> GetStatsAsync()
    {
        var allFeedback = await LoadAllFeedbackAsync();
        
        if (!allFeedback.Any())
            return new FeedbackStats();

        return new FeedbackStats
        {
            TotalFeedbacks = allFeedback.Count,
            AverageRating = allFeedback.Average(f => f.HelpfulnessRating),
            MostImplemented = allFeedback
                .SelectMany(f => f.ImplementedRecommendations)
                .GroupBy(r => r)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Recommendation = g.Key, Count = g.Count() })
                .ToList(),
            AverageImplementationCount = allFeedback.Average(f => f.ImplementedRecommendations.Count),
            FeedbackByAge = allFeedback
                .GroupBy(f => f.ChildAge)
                .OrderBy(g => g.Key)
                .Select(g => new { Age = g.Key, Count = g.Count() })
                .ToList(),
            FeedbackByDevice = allFeedback
                .GroupBy(f => f.DeviceType)
                .Select(g => new { Device = g.Key, Count = g.Count() })
                .ToList()
        };
    }
}

/// <summary>
/// Anonymous feedback from parent about recommendations
/// </summary>
public class RecommendationFeedback
{
    public string Id { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public int ChildAge { get; set; }
    public List<string> Concerns { get; set; } = new();
    public List<string> RecommendationsShown { get; set; } = new();
    
    // Feedback data
    public int HelpfulnessRating { get; set; } // 1-5
    public List<string> ImplementedRecommendations { get; set; } = new();
    public List<string> SkippedRecommendations { get; set; } = new();
    public string UserComments { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Aggregated statistics from feedback
/// </summary>
public class FeedbackStats
{
    public int TotalFeedbacks { get; set; }
    public double AverageRating { get; set; }
    public object MostImplemented { get; set; } = new { };
    public double AverageImplementationCount { get; set; }
    public object FeedbackByAge { get; set; } = new { };
    public object FeedbackByDevice { get; set; } = new { };
}
