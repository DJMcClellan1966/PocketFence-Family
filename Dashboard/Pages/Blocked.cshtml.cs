using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages;

public class BlockedModel : PageModel
{
    private readonly BlockedContentStore _blockedContent;

    public BlockedModel(BlockedContentStore blockedContent)
    {
        _blockedContent = blockedContent;
    }

    public List<BlockedContentEntry> BlockedItems { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        // Load all blocked content from storage
        BlockedItems = _blockedContent.GetAll();

        return Page();
    }
}
