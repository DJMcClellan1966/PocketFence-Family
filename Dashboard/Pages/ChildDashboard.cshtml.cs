using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class ChildDashboardModel : AuthenticatedPageModel
{
    private readonly BlockedContentStore _blockedContent;

    public ChildDashboardModel(BlockedContentStore blockedContent)
    {
        _blockedContent = blockedContent;
    }

    public int BlockedToday { get; set; }
    public int BlockedThisWeek { get; set; }
    public int BlockedAllTime { get; set; }
    public List<BlockedItem> RecentBlocks { get; set; } = new();

    public IActionResult OnGet()
    {
        // Check if user has child role
        var role = HttpContext.Session.GetString("Role");
        if (role != "Child")
        {
            // Redirect non-children to appropriate dashboard
            return Redirect("/");
        }

        // Load statistics
        BlockedToday = _blockedContent.GetBlockedToday();
        BlockedThisWeek = _blockedContent.GetBlockedThisWeek();
        BlockedAllTime = _blockedContent.GetBlockedAllTime();

        // Get recent blocks (top 5, but hide sensitive details)
        var recentEntries = _blockedContent.GetRecent(5);
        RecentBlocks = recentEntries.Select(e => new BlockedItem
        {
            Time = e.Timestamp,
            Content = "Protected content", // Don't show actual content to children
            Reason = GetSimplifiedReason(e.Reason)
        }).ToList();

        return Page();
    }

    private string GetSimplifiedReason(string reason)
    {
        // Simplify reasons for children
        if (reason.Contains("threat", StringComparison.OrdinalIgnoreCase))
            return "Potentially unsafe content";
        if (reason.Contains("blocked domain", StringComparison.OrdinalIgnoreCase))
            return "Blocked website";
        if (reason.Contains("keyword", StringComparison.OrdinalIgnoreCase))
            return "Inappropriate content";
        
        return "Protected by PocketFence";
    }
}
