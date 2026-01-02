using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class IndexModel : PageModel
{
    public string Username { get; set; } = "Parent";
    public int BlockedToday { get; set; }
    public int BlockedThisWeek { get; set; }
    public int BlockedThisMonth { get; set; }
    public int BlockedAllTime { get; set; }
    public List<BlockedItem> RecentBlocks { get; set; } = new();
    public Dictionary<string, int> BlocksByCategory { get; set; } = new();

    public IActionResult OnGet()
    {
        // Check authentication
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        Username = HttpContext.Session.GetString("Username") ?? "Parent";

        // TODO: Load real data from your ContentFilter/SimpleAI
        // For now, using sample data for MVP
        LoadSampleData();

        return Page();
    }

    private void LoadSampleData()
    {
        BlockedToday = 3;
        BlockedThisWeek = 17;
        BlockedThisMonth = 64;
        BlockedAllTime = 248;

        RecentBlocks = new List<BlockedItem>
        {
            new() { Time = DateTime.Now.AddMinutes(-5), Content = "inappropriate-site.com", Reason = "Adult Content" },
            new() { Time = DateTime.Now.AddMinutes(-23), Content = "violent-game.com", Reason = "Violence" },
            new() { Time = DateTime.Now.AddHours(-2), Content = "how to hack wifi", Reason = "Dangerous Activity" },
        };

        BlocksByCategory = new Dictionary<string, int>
        {
            { "Adult Content", 89 },
            { "Violence", 52 },
            { "Hate Speech", 12 },
            { "Dangerous Activities", 31 },
            { "Gambling", 18 },
            { "Drugs", 24 },
            { "Other", 22 }
        };
    }
}

public class BlockedItem
{
    public DateTime Time { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
