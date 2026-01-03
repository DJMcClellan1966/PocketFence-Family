using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketFence_AI.OAuth;
using PocketFence_AI.Dashboard;
using System.Text.Json;

namespace PocketFence_AI.Dashboard.Pages.OAuth
{
    /// <summary>
    /// Handles OAuth callback from Apple Sign In.
    /// Receives authorization code, exchanges for tokens, stores in user account.
    /// </summary>
    public class AppleCallbackModel : PageModel
    {
        public bool Success { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        public async Task<IActionResult> OnPostAsync(
            [FromForm] string? code,
            [FromForm] string? state,
            [FromForm] string? error,
            [FromForm] string? user) // Apple sends user info on first auth only
        {
            // Check if user is authenticated with PocketFence
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/Login?returnUrl=/Devices/LinkiOS");
            }

            // Check for Apple errors
            if (!string.IsNullOrEmpty(error))
            {
                Success = false;
                ErrorMessage = $"Apple authentication error: {error}";
                return Page();
            }

            // Validate state parameter (CSRF protection)
            var sessionState = HttpContext.Session.GetString("AppleOAuthState");
            if (string.IsNullOrEmpty(state) || state != sessionState)
            {
                Success = false;
                ErrorMessage = "Invalid state parameter. Please try again.";
                return Page();
            }

            // Validate code
            if (string.IsNullOrEmpty(code))
            {
                Success = false;
                ErrorMessage = "No authorization code received from Apple.";
                return Page();
            }

            try
            {
                // Initialize OAuth service
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var appleConfig = config.GetSection("Apple");
                var oauthService = new AppleOAuthService(
                    clientId: appleConfig["ClientId"] ?? "",
                    teamId: appleConfig["TeamId"] ?? "",
                    keyId: appleConfig["KeyId"] ?? "",
                    privateKeyPath: appleConfig["PrivateKeyPath"] ?? "Data/AppleAuthKey.p8",
                    redirectUri: appleConfig["RedirectUri"] ?? "http://localhost:5000/OAuth/AppleCallback"
                );

                // Exchange code for tokens
                var tokenResponse = await oauthService.ExchangeCodeForTokenAsync(code);
                if (tokenResponse == null)
                {
                    Success = false;
                    ErrorMessage = "Failed to exchange authorization code for tokens.";
                    return Page();
                }

                // Decode ID token to get user info
                AppleUserInfo? userInfo = null;
                if (!string.IsNullOrEmpty(tokenResponse.id_token))
                {
                    userInfo = oauthService.DecodeIdToken(tokenResponse.id_token);
                }

                // Load user and update with Apple tokens
                var userManager = new UserManager();
                var pocketFenceUser = userManager.GetUserById(userId);
                if (pocketFenceUser == null)
                {
                    Success = false;
                    ErrorMessage = "User not found.";
                    return Page();
                }

                // Store OAuth tokens in user record (encrypted in production!)
                // TODO: Encrypt these tokens before storing
                var appleAuthData = new
                {
                    AccessToken = tokenResponse.access_token,
                    RefreshToken = tokenResponse.refresh_token,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in),
                    AppleUserId = userInfo?.Subject,
                    Email = userInfo?.Email,
                    IsPrivateEmail = userInfo?.IsPrivateEmail ?? false
                };

                // Store as JSON in a custom field (or create separate AppleAuth table in production)
                pocketFenceUser.Notes = JsonSerializer.Serialize(appleAuthData);
                userManager.UpdateUser(pocketFenceUser);

                // Clear OAuth state from session
                HttpContext.Session.Remove("AppleOAuthState");

                Success = true;
                return Page();
            }
            catch (Exception ex)
            {
                Success = false;
                ErrorMessage = $"Authentication error: {ex.Message}";
                Console.WriteLine($"Apple OAuth callback error: {ex}");
                return Page();
            }
        }
    }
}
