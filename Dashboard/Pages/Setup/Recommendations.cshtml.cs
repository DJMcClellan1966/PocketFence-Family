using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PocketFence_AI.Dashboard.Pages.Setup;

public class RecommendationsModel : PageModel
{
    private readonly FeedbackStore _feedbackStore;

    public string DeviceType { get; set; } = "";
    public int ChildAge { get; set; }
    public List<string> Concerns { get; set; } = new();
    public string AiSummary { get; set; } = "";
    public List<Recommendation> Recommendations { get; set; } = new();
    public List<string> AppsToBlock { get; set; } = new();
    public List<string> AppsToAllow { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool FeedbackSubmitted { get; set; }

    [BindProperty]
    public int Rating { get; set; }

    [BindProperty]
    public List<string> ImplementedIds { get; set; } = new();

    [BindProperty]
    public string? Comments { get; set; }

    public RecommendationsModel()
    {
        _feedbackStore = new FeedbackStore();
    }

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

            // Preserve data for potential feedback submission
            TempData.Keep("DeviceType");
            TempData.Keep("ChildAge");
            TempData.Keep("Concerns");
        }
        else
        {
            ErrorMessage = "No setup data found. Please complete the setup wizard first.";
        }
    }

    public async Task<IActionResult> OnPostSubmitFeedbackAsync()
    {
        // Validate model state
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors and try again.";
            
            // Restore data for display
            if (TempData["DeviceType"] is string dt &&
                TempData["ChildAge"] is int ca)
            {
                DeviceType = dt;
                ChildAge = ca;
                if (TempData["Concerns"] is string cs)
                {
                    Concerns = cs.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                GenerateRecommendations();
                TempData.Keep("DeviceType");
                TempData.Keep("ChildAge");
                TempData.Keep("Concerns");
            }
            return Page();
        }

        // Restore recommendation data
        if (TempData["DeviceType"] is string deviceType &&
            TempData["ChildAge"] is int age)
        {
            DeviceType = deviceType;
            ChildAge = age;
            
            if (TempData["Concerns"] is string concernsStr)
            {
                Concerns = concernsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            GenerateRecommendations();

            // Additional validation
            if (Rating < 1 || Rating > 5)
            {
                ErrorMessage = "Rating must be between 1 and 5 stars.";
                TempData.Keep("DeviceType");
                TempData.Keep("ChildAge");
                TempData.Keep("Concerns");
                return Page();
            }

            // Create feedback object
            var feedback = new RecommendationFeedback
            {
                Id = Guid.NewGuid().ToString(),
                DeviceType = DeviceType,
                ChildAge = ChildAge,
                Concerns = Concerns,
                RecommendationsShown = Recommendations.Select(r => r.Id).ToList(),
                HelpfulnessRating = Rating,
                ImplementedRecommendations = ImplementedIds ?? new List<string>(),
                SkippedRecommendations = Recommendations
                    .Select(r => r.Id)
                    .Except(ImplementedIds ?? new List<string>())
                    .ToList(),
                UserComments = Comments ?? "",
                CreatedAt = DateTime.UtcNow
            };

            // Save feedback with error handling
            var success = await _feedbackStore.SaveFeedbackAsync(feedback);

            if (success)
            {
                FeedbackSubmitted = true;
            }
            else
            {
                ErrorMessage = "Failed to save feedback. Please try again later.";
            }

            // Keep data for display
            TempData.Keep("DeviceType");
            TempData.Keep("ChildAge");
            TempData.Keep("Concerns");
        }
        else
        {
            ErrorMessage = "Session expired. Please complete the setup wizard again.";
        }

        return Page();
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
