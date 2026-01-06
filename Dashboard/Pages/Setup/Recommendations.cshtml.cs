using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages.Setup;

public class RecommendationsModel : PageModel
{
    public string DeviceType { get; set; } = "";
    public int ChildAge { get; set; }
    public List<string> Concerns { get; set; } = new();
    public string AiSummary { get; set; } = "";
    public List<Recommendation> Recommendations { get; set; } = new();
    public List<string> AppsToBlock { get; set; } = new();
    public List<string> AppsToAllow { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Get data from TempData
        if (TempData["DeviceType"] is string deviceType &&
            TempData["ChildAge"] is int age)
        {
            DeviceType = deviceType;
            ChildAge = age;
            
            if (TempData["Concerns"] is string concernsStr)
            {
                Concerns = concernsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // Generate AI recommendations
            GenerateRecommendations();
        }
        else
        {
            ErrorMessage = "No setup data found. Please complete the setup wizard first.";
        }
    }

    private void GenerateRecommendations()
    {
        var ai = new SimpleAI();
        var recommendationData = ai.GenerateSetupRecommendations(DeviceType, ChildAge, Concerns);

        AiSummary = recommendationData.Summary;
        Recommendations = recommendationData.Recommendations;
        AppsToBlock = recommendationData.AppsToBlock;
        AppsToAllow = recommendationData.AppsToAllow;
    }
}
