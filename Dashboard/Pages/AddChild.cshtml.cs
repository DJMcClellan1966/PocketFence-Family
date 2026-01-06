using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PocketFence_AI.Dashboard.Pages;

public class AddChildModel : AuthenticatedPageModel
{
    private readonly UserManager _userManager;

    [BindProperty]
    [Required(ErrorMessage = "Please enter your child's name")]
    [MaxLength(50)]
    public string ChildName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Range(0, 17, ErrorMessage = "Age must be between 0 and 17")]
    public int ChildAge { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select a device type")]
    public string DeviceType { get; set; } = string.Empty;

    [BindProperty]
    public List<string> Concerns { get; set; } = new();

    [BindProperty]
    [MaxLength(500)]
    public string? Notes { get; set; }

    public string? ErrorMessage { get; set; }

    public AddChildModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    public void OnGet()
    {
        // Authentication handled by base class
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors and try again.";
            return Page();
        }

        try
        {
            // Get current user
            var user = await _userManager.GetUserByUsernameAsync(Username);
            if (user == null)
            {
                ErrorMessage = "User not found. Please log in again.";
                return RedirectToPage("/Login");
            }

            // Create new child profile
            var child = new ChildProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = ChildName.Trim(),
                Age = ChildAge,
                DeviceType = DeviceType,
                Concerns = Concerns ?? new List<string>(),
                Notes = Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            // Add to user's children list
            user.Children.Add(child);

            // Save user with internal method
            await _userManager.SaveDatabaseInternalAsync(user);

            // Redirect to dashboard with success message
            TempData["SuccessMessage"] = $"✅ {ChildName} has been added to your family!";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to add child: {ex.Message}";
            return Page();
        }
    }
}
