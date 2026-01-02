using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace PocketFence_AI.Dashboard.Pages;

public class BlockedModel : PageModel
{
    private readonly BlockedContentStore _blockedContent;

    public BlockedModel(BlockedContentStore blockedContent)
    {
        _blockedContent = blockedContent;
    }

    public List<BlockedContentEntry> BlockedItems { get; set; } = new();
    public string SearchQuery { get; set; } = "";
    public string CategoryFilter { get; set; } = "";
    public List<string> AvailableCategories { get; set; } = new();

    public IActionResult OnGet(string? search, string? category)
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        SearchQuery = search ?? "";
        CategoryFilter = category ?? "";

        // Load all blocked content from storage
        var allBlocks = _blockedContent.GetAll();

        // Get available categories for filter dropdown
        AvailableCategories = _blockedContent.GetBlocksByCategory().Keys.ToList();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            allBlocks = allBlocks
                .Where(b => b.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                           b.Reason.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                           b.Category.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(CategoryFilter))
        {
            allBlocks = allBlocks
                .Where(b => b.Category.Equals(CategoryFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        BlockedItems = allBlocks;

        return Page();
    }

    public IActionResult OnGetExport()
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        var items = _blockedContent.GetAll();
        var csv = new StringBuilder();

        // Add header
        csv.AppendLine("Timestamp,Content,Category,Reason");

        // Add data rows
        foreach (var item in items)
        {
            csv.AppendLine($"\"{item.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(item.Content)}\",\"{EscapeCsv(item.Category)}\",\"{EscapeCsv(item.Reason)}\"");
        }

        var fileName = $"blocked-content-{DateTime.Now:yyyy-MM-dd-HHmmss}.csv";
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());

        return File(bytes, "text/csv", fileName);
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape double quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}
