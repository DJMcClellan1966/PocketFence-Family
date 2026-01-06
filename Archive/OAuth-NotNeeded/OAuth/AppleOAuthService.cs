using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace PocketFence_AI.OAuth
{
    /// <summary>
    /// Handles Apple Sign in with Apple OAuth 2.0 flow.
    /// Generates client secrets using private key, exchanges codes for tokens.
    /// </summary>
    public class AppleOAuthService
    {
        private readonly string _clientId;
        private readonly string _teamId;
        private readonly string _keyId;
        private readonly string _privateKeyPath;
        private readonly string _redirectUri;
        private readonly HttpClient _httpClient;

        // Apple OAuth endpoints
        private const string AuthorizationEndpoint = "https://appleid.apple.com/auth/authorize";
        private const string TokenEndpoint = "https://appleid.apple.com/auth/token";
        private const string RevokeEndpoint = "https://appleid.apple.com/auth/revoke";

        public AppleOAuthService(
            string clientId,
            string teamId,
            string keyId,
            string privateKeyPath,
            string redirectUri,
            HttpClient? httpClient = null)
        {
            _clientId = clientId;
            _teamId = teamId;
            _keyId = keyId;
            _privateKeyPath = privateKeyPath;
            _redirectUri = redirectUri;
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Generate the authorization URL to redirect users to Apple's consent screen.
        /// </summary>
        /// <param name="state">CSRF protection token (random string, store in session)</param>
        /// <returns>Full Apple authorization URL</returns>
        public string GetAuthorizationUrl(string state)
        {
            var scopes = new[] { "name", "email" };
            var queryParams = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["redirect_uri"] = _redirectUri,
                ["response_type"] = "code",
                ["state"] = state,
                ["scope"] = string.Join(" ", scopes),
                ["response_mode"] = "form_post" // Apple posts to callback instead of GET
            };

            var queryString = string.Join("&", 
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            return $"{AuthorizationEndpoint}?{queryString}";
        }

        /// <summary>
        /// Exchange authorization code for access token and refresh token.
        /// </summary>
        /// <param name="code">Authorization code from Apple callback</param>
        /// <returns>Access token, refresh token, expiration time</returns>
        public async Task<AppleTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            var clientSecret = GenerateClientSecret();

            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = _redirectUri
            };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(TokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Apple token exchange failed: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AppleTokenResponse>(json);
        }

        /// <summary>
        /// Refresh an expired access token using the refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token from previous authorization</param>
        /// <returns>New access token and expiration time</returns>
        public async Task<AppleTokenResponse?> RefreshAccessTokenAsync(string refreshToken)
        {
            var clientSecret = GenerateClientSecret();

            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(TokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Apple token refresh failed: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AppleTokenResponse>(json);
        }

        /// <summary>
        /// Revoke access token or refresh token (logout).
        /// </summary>
        /// <param name="token">Access token or refresh token to revoke</param>
        /// <returns>True if revocation successful</returns>
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var clientSecret = GenerateClientSecret();

            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = clientSecret,
                ["token"] = token,
                ["token_type_hint"] = "refresh_token"
            };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(RevokeEndpoint, content);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Generate JWT client secret signed with private key.
        /// Apple requires a new JWT for each token request (expires in 6 months).
        /// </summary>
        private string GenerateClientSecret()
        {
            // Load private key from .p8 file
            var privateKeyContent = File.ReadAllText(_privateKeyPath)
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "");

            var privateKeyBytes = Convert.FromBase64String(privateKeyContent);

            // Create ECDsa key from private key bytes
            var ecdsa = ECDsa.Create();
            ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            var signingCredentials = new SigningCredentials(
                new ECDsaSecurityKey(ecdsa) { KeyId = _keyId },
                SecurityAlgorithms.EcdsaSha256
            );

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _teamId,
                Audience = "https://appleid.apple.com",
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sub", _clientId)
                }),
                IssuedAt = now,
                Expires = now.AddMonths(6), // Max 6 months
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate and decode the ID token returned by Apple (contains user info).
        /// </summary>
        /// <param name="idToken">JWT ID token from Apple</param>
        /// <returns>User information (Apple ID, email, name)</returns>
        public AppleUserInfo? DecodeIdToken(string idToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(idToken);

                return new AppleUserInfo
                {
                    Subject = token.Subject, // Unique Apple user ID
                    Email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                    EmailVerified = bool.Parse(token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value ?? "false"),
                    IsPrivateEmail = token.Claims.FirstOrDefault(c => c.Type == "is_private_email")?.Value == "true"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decode Apple ID token: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Apple OAuth token response.
    /// </summary>
    public class AppleTokenResponse
    {
        public string access_token { get; set; } = "";
        public string token_type { get; set; } = "Bearer";
        public int expires_in { get; set; } // Seconds until expiration
        public string? refresh_token { get; set; }
        public string? id_token { get; set; } // JWT with user info
    }

    /// <summary>
    /// User information extracted from Apple ID token.
    /// </summary>
    public class AppleUserInfo
    {
        public string Subject { get; set; } = ""; // Unique Apple user ID
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsPrivateEmail { get; set; } // Apple's "Hide My Email" feature
    }
}
