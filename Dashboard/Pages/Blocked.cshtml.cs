using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class BlockedModel : PageModel
{
    public List<BlockedContentItem> BlockedItems { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        // TODO: Load from actual blocking log
        LoadSampleData();

        return Page();
    }

    private void LoadSampleData()
    {
        BlockedItems = new List<BlockedContentItem>
        {
            new() { 
                Time = DateTime.Now.AddMinutes(-5), 
                Content = "inappropriate-site.com/page", 
                Category = "Adult Content",
                Reason = "Contains explicit material"
            },
            new() { 
                Time = DateTime.Now.AddMinutes(-23), 
                Content = "violent-game-downloads.com", 
                Category = "Violence",
                Reason = "Graphic violence detected"
            },
            new() { 
                Time = DateTime.Now.AddHours(-2), 
                Content = "Search: how to hack wifi password", 
                Category = "Dangerous",
                Reason = "Potentially harmful activity"
            },
        };
    }
}

public class BlockedContentItem
{
    public DateTime Time { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
