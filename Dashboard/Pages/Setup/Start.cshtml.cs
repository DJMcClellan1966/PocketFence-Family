using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PocketFence_AI.Dashboard.Pages.Setup;

public class StartModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select a device type")]
    public string DeviceType { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Please select your child's age")]
    [Range(0, 17, ErrorMessage = "Age must be between 0 and 17")]
    public int ChildAge { get; set; }

    [BindProperty]
    public List<string> Concerns { get; set; } = new();

    public void OnGet()
    {
        // Display the setup form
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Store data in TempData to pass to recommendations page
        TempData["DeviceType"] = DeviceType;
        TempData["ChildAge"] = ChildAge;
        TempData["Concerns"] = string.Join(",", Concerns);

        // Redirect to recommendations page
        return RedirectToPage("/Setup/Recommendations");
    }
}
