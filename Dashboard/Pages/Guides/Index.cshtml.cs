using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages.Guides;

public class GuidesModel : PageModel
{
    public string DeviceType { get; set; } = "";

    public void OnGet(string deviceType)
    {
        DeviceType = deviceType ?? "iOS";
    }
}
