namespace PocketFence_AI.Dashboard;

/// <summary>
/// Represents a child in the family with their device restrictions and settings
/// </summary>
public class ChildProfile
{
    /// <summary>
    /// Unique identifier for the child profile
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Child's name (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Child's age in years (0-17)
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Device type: iOS, Android, or Windows
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Parent's concerns for this child (e.g., SocialMedia, Gaming, ScreenTime)
    /// </summary>
    public List<string> Concerns { get; set; } = new();

    /// <summary>
    /// When this profile was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this profile was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// History of AI recommendations for this child
    /// </summary>
    public List<RecommendationSnapshot> RecommendationHistory { get; set; } = new();

    /// <summary>
    /// Total blocks for this child (tracked separately per child)
    /// </summary>
    public int TotalBlocks { get; set; }

    /// <summary>
    /// Optional notes from parent about this child
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Snapshot of recommendations generated at a point in time
/// </summary>
public class RecommendationSnapshot
{
    public DateTime GeneratedAt { get; set; }
    public int ChildAgeAtTime { get; set; }
    public List<string> RecommendationsGiven { get; set; } = new();
    public string AiSummary { get; set; } = string.Empty;
}
