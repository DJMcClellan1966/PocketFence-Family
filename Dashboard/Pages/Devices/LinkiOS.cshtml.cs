using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketFence_AI.OAuth;
using System.Security.Cryptography;
using System.Text;

namespace PocketFence_AI.Dashboard.Pages.Devices
{
    /// <summary>
    /// Page for linking iOS devices via Apple Sign In.
    /// Step 1: Authenticate with Apple
    /// Step 2: Select child and device from Family Sharing
    /// </summary>
    public class LinkiOSModel : AuthenticatedPageModel
    {
        public bool IsAppleAuthenticated { get; set; } = false;

        public void OnGet()
        {
            // Check if user already authenticated with Apple
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                var userManager = new UserManager();
                var user = userManager.GetUserById(userId);
                
                // Check if user has Apple OAuth tokens stored
                // TODO: Check Notes field for Apple tokens (improve this in production)
                IsAppleAuthenticated = !string.IsNullOrEmpty(user?.Notes) && user.Notes.Contains("AccessToken");
            }
        }

        public IActionResult OnPost()
        {
            // Generate state parameter for CSRF protection
            var state = GenerateRandomState();
            HttpContext.Session.SetString("AppleOAuthState", state);

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

            // Generate authorization URL and redirect
            var authUrl = oauthService.GetAuthorizationUrl(state);
            return Redirect(authUrl);
        }

        /// <summary>
        /// Generate cryptographically secure random state for OAuth.
        /// </summary>
        private string GenerateRandomState()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
