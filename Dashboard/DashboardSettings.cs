namespace PocketFence_AI.Dashboard;

/// <summary>
/// Dashboard settings that persist across restarts
/// </summary>
public class DashboardSettings
{
    public string FilterLevel { get; set; } = "moderate";
    public string NotificationEmail { get; set; } = "";
    public List<string> BlockedCategories { get; set; } = new() 
    { 
        "adult", "violence", "hate", "drugs", "gambling" 
    };
    public List<string> CustomBlocklist { get; set; } = new();
}
