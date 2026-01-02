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
    public string DateRangeFilter { get; set; } = "";
    public List<string> AvailableCategories { get; set; } = new();
    public int TotalCount { get; set; } = 0;

    public IActionResult OnGet(string? search, string? category, string? dateRange)
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        SearchQuery = search ?? "";
        CategoryFilter = category ?? "";
        DateRangeFilter = dateRange ?? "";

        // Load all blocked content from storage
        var allBlocks = _blockedContent.GetAll();
        TotalCount = allBlocks.Count;

        // Get available categories for filter dropdown
        AvailableCategories = _blockedContent.GetBlocksByCategory().Keys.ToList();

        // Apply date range filter first
        if (!string.IsNullOrWhiteSpace(DateRangeFilter))
        {
            var cutoffDate = DateRangeFilter switch
            {
                "today" => DateTime.Today,
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddDays(-30),
                _ => DateTime.MinValue
            };

            if (cutoffDate != DateTime.MinValue)
            {
                allBlocks = allBlocks.Where(b => b.Timestamp >= cutoffDate).ToList();
            }
        }

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

    public int GetCategoryCount(string category)
    {
        return _blockedContent.GetAll()
            .Count(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public IActionResult OnGetExport()
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        var items = _blockedContent.GetAll();
        var csv = GenerateCsv(items, "all");

        return csv;
    }

    public IActionResult OnGetExportFiltered(string? search, string? category, string? dateRange)
    {
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Redirect("/login");
        }

        // Apply same filtering logic as OnGet
        var items = _blockedContent.GetAll();

        // Apply date range filter
        if (!string.IsNullOrWhiteSpace(dateRange))
        {
            var cutoffDate = dateRange switch
            {
                "today" => DateTime.Today,
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddDays(-30),
                _ => DateTime.MinValue
            };

            if (cutoffDate != DateTime.MinValue)
            {
                items = items.Where(b => b.Timestamp >= cutoffDate).ToList();
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items
                .Where(b => b.Content.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                           b.Reason.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                           b.Category.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            items = items
                .Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var csv = GenerateCsv(items, "filtered");

        return csv;
    }

    private FileContentResult GenerateCsv(List<BlockedContentEntry> items, string exportType)
    {
        var csv = new StringBuilder();

        // Add header with metadata
        csv.AppendLine($"# PocketFence Blocked Content Report");
        csv.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"# Total Records: {items.Count}");
        csv.AppendLine($"# Export Type: {exportType}");
        csv.AppendLine();

        // Add column headers
        csv.AppendLine("Timestamp,Content,Category,Reason");

        // Add data rows
        foreach (var item in items)
        {
            csv.AppendLine($"\"{item.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(item.Content)}\",\"{EscapeCsv(item.Category)}\",\"{EscapeCsv(item.Reason)}\"");
        }

        // Add summary statistics
        csv.AppendLine();
        csv.AppendLine("# Category Summary:");
        var categoryGroups = items.GroupBy(x => x.Category)
            .OrderByDescending(g => g.Count());
        
        foreach (var group in categoryGroups)
        {
            csv.AppendLine($"# {group.Key}: {group.Count()} blocks");
        }

        var fileName = $"blocked-content-{exportType}-{DateTime.Now:yyyy-MM-dd-HHmmss}.csv";
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
