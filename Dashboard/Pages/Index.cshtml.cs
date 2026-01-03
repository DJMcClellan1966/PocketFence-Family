using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class IndexModel : AuthenticatedPageModel
{
    private readonly BlockedContentStore _blockedContent;

    public IndexModel(BlockedContentStore blockedContent)
    {
        _blockedContent = blockedContent;
    }

    public int BlockedToday { get; set; }
    public int BlockedThisWeek { get; set; }
    public int BlockedThisMonth { get; set; }
    public int BlockedAllTime { get; set; }
    public List<BlockedItem> RecentBlocks { get; set; } = new();
    public Dictionary<string, int> BlocksByCategory { get; set; } = new();

    public void OnGet()
    {
        // Authentication handled by AuthenticatedPageModel base class
        // Username already loaded from base class

        // Load real data from BlockedContentStore
        LoadRealData();
    }

    private void LoadRealData()
    {
        // Get statistics
        BlockedToday = _blockedContent.GetBlockedToday();
        BlockedThisWeek = _blockedContent.GetBlockedThisWeek();
        BlockedThisMonth = _blockedContent.GetBlockedThisMonth();
        BlockedAllTime = _blockedContent.GetBlockedAllTime();

        // Get recent blocks (top 5)
        var recentEntries = _blockedContent.GetRecent(5);
        RecentBlocks = recentEntries.Select(e => new BlockedItem
        {
            Time = e.Timestamp,
            Content = e.Content,
            Reason = e.Reason
        }).ToList();

        // Get blocks by category
        BlocksByCategory = _blockedContent.GetBlocksByCategory();
    }
}

public class BlockedItem
{
    public DateTime Time { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
