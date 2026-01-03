using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PocketFence_AI.Dashboard.Pages;

public class VerifyPhoneModel : PageModel
{
    private readonly UserManager _userManager;

    public VerifyPhoneModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Verification code is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
    public string Code { get; set; } = string.Empty;

    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string PhoneNumberDisplay { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(PhoneNumber))
        {
            ErrorMessage = "Phone number is required";
            return Page();
        }

        // Format phone for display
        PhoneNumberDisplay = AiSmsService.FormatPhoneNumber(PhoneNumber);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            PhoneNumberDisplay = AiSmsService.FormatPhoneNumber(PhoneNumber);
            return Page();
        }

        var (success, message) = await _userManager.VerifyPhoneAsync(PhoneNumber, Code);

        if (success)
        {
            Success = true;
            Console.WriteLine($"[VerifyPhone] Phone verified successfully: {PhoneNumber}");
        }
        else
        {
            ErrorMessage = message;
            PhoneNumberDisplay = AiSmsService.FormatPhoneNumber(PhoneNumber);
            Console.WriteLine($"[VerifyPhone] Verification failed for {PhoneNumber}: {message}");
        }

        return Page();
    }
}
