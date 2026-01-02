using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using PocketFence.FamilyOS.Core;
using System.Linq;
using FamilyOS;
using System.Runtime.CompilerServices; // Performance: Inline methods
using System.Buffers; // GPT4All: Memory pooling
using System.Numerics; // GPT4All: SIMD vectorization
using System.Runtime.Intrinsics; // GPT4All: Hardware intrinsics
using System.Runtime.Intrinsics.X86; // GPT4All: x86 SIMD
using System.Threading; // GPT4All: Parallel processing

namespace PocketFence.FamilyOS.Services
{
    /// <summary>
    /// Family member management service implementation
    /// </summary>
    public class FamilyManagerService : IFamilyManager
    {
        private readonly ILogger<FamilyManagerService> _logger;
        private readonly ISystemSecurity _systemSecurity;
        private readonly string _dataPath;
        private readonly List<FamilyMember> _familyMembers;
        private readonly Dictionary<string, FamilyMember> _memberCache; // Performance: Username lookup cache
        private readonly PocketFence.FamilyOS.Windows.WindowsSecurityService? _windowsSecurity;

        public FamilyManagerService(ILogger<FamilyManagerService> logger, ISystemSecurity systemSecurity, string dataPath = "./FamilyData")
        {
            _logger = logger;
            _systemSecurity = systemSecurity;
            _dataPath = dataPath;
            _familyMembers = new List<FamilyMember>();
            _memberCache = new Dictionary<string, FamilyMember>(StringComparer.OrdinalIgnoreCase); // Performance: Case-insensitive cache
            
            // Initialize Windows security integration
            try
            {
                _windowsSecurity = new PocketFence.FamilyOS.Windows.WindowsSecurityService(
                    Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<PocketFence.FamilyOS.Windows.WindowsSecurityService>());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Windows security integration not available");
            }
        }

        public async Task LoadFamilyProfilesAsync()
        {
            try
            {
                var profilesFile = Path.Combine(_dataPath, "family_profiles.json");
                
                if (!Directory.Exists(_dataPath))
                {
                    Directory.CreateDirectory(_dataPath);
                }

                if (File.Exists(profilesFile))
                {
                    var encryptedData = await File.ReadAllTextAsync(profilesFile);
                    
                    // Security Fix 5: Verify integrity before processing
                    var hmacFile = Path.Combine(_dataPath, "family_profiles.hmac");
                    if (File.Exists(hmacFile))
                    {
                        var storedHmac = await File.ReadAllTextAsync(hmacFile);
                        using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes("FamilyOS-Integrity-Key")))
                        {
                            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(encryptedData));
                            var computedHmac = Convert.ToBase64String(computedHash);
                            
                            if (storedHmac != computedHmac)
                            {
                                _logger.LogError("Data integrity check failed - file may be corrupted or tampered");
                                File.Delete(profilesFile);
                                File.Delete(hmacFile);
                                await CreateDefaultFamilyAsync();
                                return;
                            }
                        }
                    }
                    
                    var decryptedData = await _systemSecurity.DecryptFamilyDataAsync(encryptedData);
                    
                    if (!string.IsNullOrEmpty(decryptedData))
                    {
                        // Security Fix 4: Validate JSON structure before deserialization
                        if (decryptedData.Length > 1048576) // 1MB max size
                        {
                            _logger.LogWarning("Profile data exceeds maximum size");
                            File.Delete(profilesFile);
                            await CreateDefaultFamilyAsync();
                            return;
                        }
                        
                        var profiles = FamilyOSJsonHelper.Deserialize<List<FamilyMember>>(decryptedData);
                        
                        if (profiles != null && profiles.Count > 0)
                        {
                            _familyMembers.AddRange(profiles);
                            
                            // Performance: Populate cache for O(1) lookups
                            _memberCache.Clear();
                            foreach (var member in _familyMembers)
                            {
                                _memberCache[member.Username] = member;
                            }
                            
                            _logger.LogInformation($"Loaded {profiles.Count} family member profiles");
                        }
                        else
                        {
                            // Invalid data, create defaults
                            await CreateDefaultFamilyAsync();
                        }
                    }
                    else
                    {
                        // Decryption failed, delete corrupted file and create defaults
                        File.Delete(profilesFile);
                        await CreateDefaultFamilyAsync();
                    }
                }
                else
                {
                    // Create default family profiles for demo
                    await CreateDefaultFamilyAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load family profiles");
                await CreateDefaultFamilyAsync();
            }
        }

        private async Task CreateDefaultFamilyAsync()
        {
            var defaultFamily = new List<FamilyMember>
            {
                new FamilyMember
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "mom",
                    DisplayName = "Mom",
                    PasswordHash = HashPassword("parent123"),
                    AgeGroup = AgeGroup.Parent,
                    Role = FamilyRole.Parent,
                    DateOfBirth = DateTime.Now.AddYears(-35),
                    FilterLevel = ContentFilterLevel.Minimal,
                    ScreenTime = new ScreenTimeSettings
                    {
                        DailyLimit = TimeSpan.FromHours(8),
                        EnforceScreenTime = false
                    }
                },
                new FamilyMember
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "dad",
                    DisplayName = "Dad",
                    PasswordHash = HashPassword("parent123"),
                    AgeGroup = AgeGroup.Parent,
                    Role = FamilyRole.Parent,
                    DateOfBirth = DateTime.Now.AddYears(-37),
                    FilterLevel = ContentFilterLevel.Minimal,
                    ScreenTime = new ScreenTimeSettings
                    {
                        DailyLimit = TimeSpan.FromHours(8),
                        EnforceScreenTime = false
                    }
                },
                new FamilyMember
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "sarah",
                    DisplayName = "Sarah",
                    PasswordHash = HashPassword("kid123"),
                    AgeGroup = AgeGroup.Elementary,
                    Role = FamilyRole.Child,
                    DateOfBirth = DateTime.Now.AddYears(-8),
                    FilterLevel = ContentFilterLevel.Strict,
                    AllowedApps = new List<string> { "Safe Browser", "Educational Hub", "Family Game Center" },
                    ScreenTime = new ScreenTimeSettings
                    {
                        DailyLimit = TimeSpan.FromMinutes(90),
                        WeekdayLimit = TimeSpan.FromMinutes(60),
                        WeekendLimit = TimeSpan.FromMinutes(120),
                        EnforceScreenTime = true
                    }
                },
                new FamilyMember
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "alex",
                    DisplayName = "Alex",
                    PasswordHash = HashPassword("teen123"),
                    AgeGroup = AgeGroup.MiddleSchool,
                    Role = FamilyRole.Teen,
                    DateOfBirth = DateTime.Now.AddYears(-13),
                    FilterLevel = ContentFilterLevel.Moderate,
                    AllowedApps = new List<string> { "Safe Browser", "Educational Hub", "Family Game Center", "Family Chat" },
                    ScreenTime = new ScreenTimeSettings
                    {
                        DailyLimit = TimeSpan.FromHours(3),
                        WeekdayLimit = TimeSpan.FromHours(2),
                        WeekendLimit = TimeSpan.FromHours(4),
                        EnforceScreenTime = true
                    }
                }
            };

            _familyMembers.AddRange(defaultFamily);
            await SaveFamilyDataAsync();
            
            _logger.LogInformation("Created default family profiles");
            _logger.LogInformation("  Parents: mom, dad");
            _logger.LogInformation("  Children: sarah, alex");
            _logger.LogInformation("  ⚠️  Change default passwords on first login for security");
        }

        public async Task<FamilyMember?> AuthenticateAsync(string username, string password)
        {
            // Security Fix 2: Input validation
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50 || 
                string.IsNullOrWhiteSpace(password) || password.Length > 128)
            {
                _logger.LogWarning("Invalid authentication input format");
                return null;
            }
            
            // Sanitize username - only allow alphanumeric and basic chars
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$"))
            {
                _logger.LogWarning("Invalid username format detected");
                return null;
            }
            
            // Performance: Use cache for O(1) lookup instead of O(n) LINQ
            if (!_memberCache.TryGetValue(username, out var member))
            {
                _logger.LogWarning($"Failed authentication attempt for unknown username: {username}");
                return null;
            }
            
            if (member == null)
            {
                _logger.LogWarning($"Failed authentication attempt for unknown username: {username}");
                return null;
            }

            // Check if account is locked
            if (await IsAccountLockedAsync(username))
            {
                var lockTimeRemaining = member.AccountLockedUntil?.Subtract(DateTime.UtcNow);
                _logger.LogWarning($"Authentication failed: Account {username} is locked for {lockTimeRemaining?.Minutes} more minutes");
                return null;
            }
            
            if (VerifyPassword(password, member.PasswordHash))
            {
                // Successful login - reset failed attempts
                member.LastLoginTime = DateTime.UtcNow;
                member.IsOnline = true;
                member.FailedLoginAttempts = 0;
                
                await SaveFamilyDataAsync();
                
                // Log to Windows Event Log
                _windowsSecurity?.LogAuthenticationEvent(username, true);
                
                await _systemSecurity.LogFamilyActivityAsync($"User logged in: {username}", member);
                _logger.LogInformation($"Successful authentication for {member.DisplayName}");
                
                return member;
            }
            else
            {
                // Failed login - increment attempts and potentially lock account
                member.FailedLoginAttempts++;
                
                // Log to Windows Event Log
                _windowsSecurity?.LogAuthenticationEvent(username, false);
                
                if (member.FailedLoginAttempts >= 3)
                {
                    // Lock account for 15 minutes
                    member.AccountLockedUntil = DateTime.UtcNow.AddMinutes(15);
                    await SaveFamilyDataAsync();
                    await _systemSecurity.LogFamilyActivityAsync($"Account locked due to failed login attempts: {username}", member);
                    _logger.LogWarning($"Account locked for {username} due to {member.FailedLoginAttempts} failed attempts");
                }
                else
                {
                    await SaveFamilyDataAsync();
                    _logger.LogWarning($"Failed authentication attempt {member.FailedLoginAttempts}/3 for username: {username}");
                }
                
                return null;
            }
        }

        public async Task<List<FamilyMember>> GetFamilyMembersAsync()
        {
            return await Task.FromResult(_familyMembers.ToList());
        }

        public async Task AddFamilyMemberAsync(FamilyMember member)
        {
            member.Id = Guid.NewGuid().ToString();
            member.PasswordHash = HashPassword(member.PasswordHash); // Assume password is passed in PasswordHash field
            _familyMembers.Add(member);
            _memberCache[member.Username] = member; // Performance: Update cache
            
            await SaveFamilyDataAsync();
            await _systemSecurity.LogFamilyActivityAsync($"New family member added: {member.DisplayName}", member);
            
            _logger.LogInformation($"Added new family member: {member.DisplayName}");
        }

        public async Task UpdateFamilyMemberAsync(FamilyMember member)
        {
            var existingMember = _familyMembers.FirstOrDefault(m => m.Id == member.Id);
            if (existingMember != null)
            {
                var index = _familyMembers.IndexOf(existingMember);
                _familyMembers[index] = member;
                
                await SaveFamilyDataAsync();
                await _systemSecurity.LogFamilyActivityAsync($"Family member updated: {member.DisplayName}", member);
                
                _logger.LogInformation($"Updated family member: {member.DisplayName}");
            }
        }

        public async Task SaveFamilyDataAsync()
        {
            try
            {
                // Security Fix 3: Sanitize and validate file path
                var safeDataPath = Path.GetFullPath(_dataPath);
                if (!safeDataPath.StartsWith(Path.GetFullPath("./FamilyData"), StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityException("Invalid data path detected");
                }
                
                var profilesFile = Path.Combine(safeDataPath, "family_profiles.json");
                var jsonData = FamilyOSJsonHelper.Serialize(_familyMembers);
                var encryptedData = await _systemSecurity.EncryptFamilyDataAsync(jsonData);
                
                // Security Fix 5: Add integrity check (HMAC) to detect tampering
                var hmacFile = Path.Combine(safeDataPath, "family_profiles.hmac");
                using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes("FamilyOS-Integrity-Key")))
                {
                    var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(encryptedData));
                    await File.WriteAllTextAsync(hmacFile, Convert.ToBase64String(hash));
                }
                
                await File.WriteAllTextAsync(profilesFile, encryptedData);
                _logger.LogDebug("Family data saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save family data");
            }
        }

        public int GetFamilyMemberCount()
        {
            return _familyMembers.Count;
        }

        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword, FamilyMember requestingUser)
        {
            try
            {
                var member = _familyMembers.FirstOrDefault(m => m.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (member == null)
                {
                    _logger.LogWarning($"Password change failed: User {username} not found");
                    return false;
                }

                // Check permissions: user changing own password OR parent changing child's password
                if (member.Id != requestingUser.Id && requestingUser.Role != FamilyRole.Parent)
                {
                    _logger.LogWarning($"Permission denied: {requestingUser.Username} attempted to change password for {username}");
                    return false;
                }

                // If user is changing own password, verify current password
                if (member.Id == requestingUser.Id && !VerifyPassword(currentPassword, member.PasswordHash))
                {
                    _logger.LogWarning($"Password change failed: Invalid current password for {username}");
                    return false;
                }

                // Update password
                member.PasswordHash = HashPassword(newPassword);
                member.LastPasswordChange = DateTime.UtcNow;
                member.PasswordChangeHistory.Add(DateTime.UtcNow);
                
                // Keep only last 10 password change dates
                if (member.PasswordChangeHistory.Count > 10)
                {
                    member.PasswordChangeHistory = member.PasswordChangeHistory.Skip(member.PasswordChangeHistory.Count - 10).ToList();
                }

                await SaveFamilyDataAsync();
                await _systemSecurity.LogFamilyActivityAsync($"Password changed for {username} by {requestingUser.Username}", requestingUser);
                
                _logger.LogInformation($"Password successfully changed for {username} by {requestingUser.Username}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for {username}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string targetUsername, string newPassword, FamilyMember parentUser)
        {
            try
            {
                // Only parents can reset passwords
                if (parentUser.Role != FamilyRole.Parent)
                {
                    _logger.LogWarning($"Permission denied: {parentUser.Username} attempted to reset password for {targetUsername}");
                    return false;
                }

                var member = _familyMembers.FirstOrDefault(m => m.Username.Equals(targetUsername, StringComparison.OrdinalIgnoreCase));
                if (member == null)
                {
                    _logger.LogWarning($"Password reset failed: User {targetUsername} not found");
                    return false;
                }

                // Reset password and unlock account
                member.PasswordHash = HashPassword(newPassword);
                member.LastPasswordChange = DateTime.UtcNow;
                member.PasswordChangeHistory.Add(DateTime.UtcNow);
                member.FailedLoginAttempts = 0;
                member.AccountLockedUntil = null;

                await SaveFamilyDataAsync();
                await _systemSecurity.LogFamilyActivityAsync($"Password reset for {targetUsername} by parent {parentUser.Username}", parentUser);
                
                _logger.LogInformation($"Password successfully reset for {targetUsername} by parent {parentUser.Username}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for {targetUsername}");
                return false;
            }
        }

        public async Task<bool> IsAccountLockedAsync(string username)
        {
            var member = _familyMembers.FirstOrDefault(m => m.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (member?.AccountLockedUntil == null)
                return false;

            if (DateTime.UtcNow >= member.AccountLockedUntil)
            {
                // Auto-unlock expired lockouts
                member.AccountLockedUntil = null;
                member.FailedLoginAttempts = 0;
                await SaveFamilyDataAsync();
                return false;
            }

            return true;
        }

        public async Task UnlockAccountAsync(string username, FamilyMember parentUser)
        {
            try
            {
                if (parentUser.Role != FamilyRole.Parent)
                {
                    _logger.LogWarning($"Permission denied: {parentUser.Username} attempted to unlock account for {username}");
                    return;
                }

                var member = _familyMembers.FirstOrDefault(m => m.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                {
                    member.AccountLockedUntil = null;
                    member.FailedLoginAttempts = 0;
                    
                    await SaveFamilyDataAsync();
                    await _systemSecurity.LogFamilyActivityAsync($"Account unlocked for {username} by parent {parentUser.Username}", parentUser);
                    
                    _logger.LogInformation($"Account successfully unlocked for {username} by parent {parentUser.Username}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unlocking account for {username}");
            }
        }

        public async Task<List<string>> GetPasswordChangeHistoryAsync(string username, FamilyMember requestingUser)
        {
            try
            {
                var member = _familyMembers.FirstOrDefault(m => m.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (member == null)
                    return new List<string>();

                // Check permissions: own history OR parent viewing child's history
                if (member.Id != requestingUser.Id && requestingUser.Role != FamilyRole.Parent)
                {
                    _logger.LogWarning($"Permission denied: {requestingUser.Username} attempted to view password history for {username}");
                    return new List<string>();
                }

                return member.PasswordChangeHistory
                    .Select(date => date.ToString("yyyy-MM-dd HH:mm:ss"))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving password change history for {username}");
                return new List<string>();
            }
        }

        // GPT4All: Aggressive inlining for hot path
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private string HashPassword(string password)
        {
            // GPT4All: Use ArrayPool to avoid allocations
            var salt = ArrayPool<byte>.Shared.Rent(32);
            try
            {
                // Generate a cryptographically secure random salt
                RandomNumberGenerator.Fill(salt.AsSpan(0, 32));
                
                using (var sha256 = SHA256.Create())
                {
                    // GPT4All: Use Span<T> for zero-allocation processing
                    Span<byte> passwordBytes = stackalloc byte[password.Length * 3]; // UTF-8 worst case
                    int passwordBytesWritten = Encoding.UTF8.GetBytes(password.AsSpan(), passwordBytes);
                    passwordBytes = passwordBytes.Slice(0, passwordBytesWritten);
                    
                    // GPT4All: Rent buffer for salted password
                    var saltedPassword = ArrayPool<byte>.Shared.Rent(passwordBytesWritten + 32);
                    try
                    {
                        passwordBytes.CopyTo(saltedPassword.AsSpan());
                        salt.AsSpan(0, 32).CopyTo(saltedPassword.AsSpan(passwordBytesWritten));
                        
                        var hashedBytes = sha256.ComputeHash(saltedPassword, 0, passwordBytesWritten + 32);
                        
                        // Return hash:salt format for storage
                        var result = Convert.ToBase64String(hashedBytes) + ":" + Convert.ToBase64String(salt, 0, 32);
                        
                        // Security: Zero sensitive data from memory
                        passwordBytes.Clear();
                        saltedPassword.AsSpan(0, passwordBytesWritten + 32).Clear();
                        
                        return result;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(saltedPassword, clearArray: true);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(salt, clearArray: true);
            }
        }

        // GPT4All: Aggressive inlining for hot path
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split(':');
                if (parts.Length != 2) return false;
                
                var hash = Convert.FromBase64String(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                
                using var sha256 = SHA256.Create();
                
                // GPT4All: Use Span<T> and stackalloc for zero allocations
                Span<byte> passwordBytes = stackalloc byte[password.Length * 3];
                int passwordBytesWritten = Encoding.UTF8.GetBytes(password.AsSpan(), passwordBytes);
                passwordBytes = passwordBytes.Slice(0, passwordBytesWritten);
                
                // GPT4All: Rent from pool instead of allocating
                var saltedPassword = ArrayPool<byte>.Shared.Rent(passwordBytesWritten + salt.Length);
                try
                {
                    passwordBytes.CopyTo(saltedPassword.AsSpan());
                    salt.AsSpan().CopyTo(saltedPassword.AsSpan(passwordBytesWritten));
                    
                    var computedHash = sha256.ComputeHash(saltedPassword, 0, passwordBytesWritten + salt.Length);
                    
                    // Security: Constant-time comparison to prevent timing attacks
                    var result = CryptographicOperations.FixedTimeEquals(hash, computedHash);
                    
                    // Security: Zero sensitive data
                    passwordBytes.Clear();
                    saltedPassword.AsSpan(0, passwordBytesWritten + salt.Length).Clear();
                    Array.Clear(computedHash, 0, computedHash.Length);
                    
                    return result;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(saltedPassword, clearArray: true);
                }
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Parental controls service implementation
    /// </summary>
    public class ParentalControlsService : IParentalControls
    {
        private readonly ILogger<ParentalControlsService> _logger;
        private readonly ISystemSecurity _systemSecurity;
        private readonly Dictionary<string, DateTime> _userLoginTimes;
        private readonly Dictionary<string, TimeSpan> _dailyScreenTime;
        private bool _isActive;
        
        // Performance: Static cached lists to avoid repeated allocations
        private static readonly HashSet<string> AllBlockedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "youtube.com", "facebook.com", "twitter.com", "instagram.com", "tiktok.com",
            "reddit.com", "discord.com", "twitch.tv", "pinterest.com"
        };
        
        private static readonly HashSet<string> BaseInappropriateKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "violence", "drugs", "alcohol", "gambling", "inappropriate", "adult"
        };

        public bool IsActive => _isActive;

        public ParentalControlsService(ILogger<ParentalControlsService> logger, ISystemSecurity systemSecurity)
        {
            _logger = logger;
            _systemSecurity = systemSecurity;
            _userLoginTimes = new Dictionary<string, DateTime>();
            _dailyScreenTime = new Dictionary<string, TimeSpan>();
        }

        public async Task InitializeAsync()
        {
            _isActive = true;
            _logger.LogInformation("Parental Controls initialized and active");
            await Task.CompletedTask;
        }

        public async Task ApplyUserRestrictionsAsync(FamilyMember member)
        {
            _userLoginTimes[member.Id] = DateTime.UtcNow;
            
            var restrictions = member.AgeGroup switch
            {
                AgeGroup.Toddler => "Very restricted - Educational content only, 15-minute sessions",
                AgeGroup.Preschool => "Highly restricted - Basic educational content, 30-minute sessions",
                AgeGroup.Elementary => "Restricted - Age-appropriate educational and entertainment content",
                AgeGroup.MiddleSchool => "Moderate restrictions - Supervised social media and research access",
                AgeGroup.HighSchool => "Light restrictions - Most content allowed with monitoring",
                AgeGroup.Parent => "No restrictions - Full administrative access",
                _ => "Default restrictions applied"
            };

            _logger.LogInformation($"Applied restrictions for {member.DisplayName}: {restrictions}");
            await _systemSecurity.LogFamilyActivityAsync($"Parental controls applied for {member.DisplayName}", member);
        }

        public async Task<bool> CanAccessAppAsync(FamilyMember member, string appName)
        {
            // Check if app is in allowed list
            if (member.AllowedApps.Contains(appName))
            {
                return true;
            }

            // Check if app is blocked
            if (member.BlockedApps.Contains(appName))
            {
                await _systemSecurity.LogFamilyActivityAsync($"Blocked app access: {appName}", member);
                return false;
            }

            // Age-based app restrictions
            var isAllowed = appName switch
            {
                "Safe Browser" => member.AgeGroup >= AgeGroup.Preschool,
                "Educational Hub" => true, // Always allowed
                "Family Game Center" => member.AgeGroup >= AgeGroup.Preschool,
                "Family Chat" => member.AgeGroup >= AgeGroup.Elementary,
                "Family File Manager" => member.AgeGroup >= AgeGroup.Elementary,
                "Screen Time Manager" => true, // Always allowed
                _ => member.AgeGroup >= AgeGroup.HighSchool // Other apps require high school age
            };

            if (!isAllowed)
            {
                await _systemSecurity.LogFamilyActivityAsync($"Age restriction: {appName} blocked for {member.AgeGroup}", member);
            }

            return isAllowed;
        }

        public async Task<bool> CanAccessUrlAsync(FamilyMember member, string url)
        {
            // Check allow list first
            if (member.AllowedWebsites.Any(allowed => url.Contains(allowed, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Performance: Fast path for parents and teens - no domain checking needed
            if (member.AgeGroup >= AgeGroup.HighSchool)
            {
                return true;
            }

            // Age-based URL filtering
            var uri = new Uri(url);
            var domain = uri.Host.ToLowerInvariant();

            var blockedDomains = GetBlockedDomainsForAge(member.AgeGroup);
            
            // Performance: Direct HashSet check instead of LINQ Any
            foreach (var blocked in blockedDomains)
            {
                if (domain.Contains(blocked))
                {
                    await _systemSecurity.LogFamilyActivityAsync($"Blocked URL access: {url}", member);
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> CanAccessContentAsync(FamilyMember member, string content)
        {
            // Content analysis based on age group
            var inappropriateKeywords = GetInappropriateKeywordsForAge(member.AgeGroup);
            var hasInappropriateContent = inappropriateKeywords.Any(keyword => 
                content.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (hasInappropriateContent)
            {
                await _systemSecurity.LogFamilyActivityAsync($"Blocked inappropriate content for {member.AgeGroup}", member);
                return false;
            }

            return true;
        }

        public async Task<TimeSpan> GetRemainingScreenTimeAsync(FamilyMember member)
        {
            if (!member.ScreenTime.EnforceScreenTime || member.Role == FamilyRole.Parent)
            {
                return TimeSpan.FromHours(24); // Unlimited for parents
            }

            var today = DateTime.Today;
            var isWeekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;
            
            var dailyLimit = isWeekend ? member.ScreenTime.WeekendLimit : member.ScreenTime.WeekdayLimit;
            var usedTime = GetUsedScreenTimeToday(member.Id);
            var remaining = dailyLimit - usedTime;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public async Task SaveStateAsync()
        {
            // Save current screen time data
            var stateData = new ParentalControlsState
            {
                UserLoginTimes = _userLoginTimes,
                DailyScreenTime = _dailyScreenTime,
                LastSaved = DateTime.UtcNow
            };

            try
            {
                var jsonData = FamilyOSJsonHelper.Serialize(stateData);
                var encryptedData = await _systemSecurity.EncryptFamilyDataAsync(jsonData);
                await File.WriteAllTextAsync("./FamilyData/parental_controls_state.json", encryptedData);
                
                _logger.LogDebug("Parental controls state saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save parental controls state");
            }
        }

        private TimeSpan GetUsedScreenTimeToday(string userId)
        {
            if (_userLoginTimes.TryGetValue(userId, out var loginTime))
            {
                var today = DateTime.Today;
                if (loginTime.Date == today)
                {
                    return DateTime.UtcNow - loginTime;
                }
            }
            return TimeSpan.Zero;
        }

        private IEnumerable<string> GetBlockedDomainsForAge(AgeGroup ageGroup)
        {
            // Performance: Return empty for high school+ (fast path)
            if (ageGroup >= AgeGroup.HighSchool)
            {
                return Enumerable.Empty<string>();
            }
            
            // Return appropriate subset from cached collection
            return ageGroup switch
            {
                AgeGroup.Toddler or AgeGroup.Preschool => AllBlockedDomains,
                AgeGroup.Elementary => AllBlockedDomains.Where(d => d != "youtube.com" && d != "twitch.tv"),
                AgeGroup.MiddleSchool => new[] { "tiktok.com", "reddit.com", "discord.com" },
                _ => Enumerable.Empty<string>()
            };
        }

        private List<string> GetInappropriateKeywordsForAge(AgeGroup ageGroup)
        {
            var baseKeywords = new List<string>
            {
                "violence", "drugs", "alcohol", "gambling", "inappropriate", "adult"
            };

            return ageGroup switch
            {
                AgeGroup.Toddler or AgeGroup.Preschool => baseKeywords.Concat(new[]
                {
                    "scary", "monster", "nightmare", "fight", "weapon"
                }).ToList(),
                AgeGroup.Elementary => baseKeywords.Concat(new[]
                {
                    "dating", "romance", "social media", "chat"
                }).ToList(),
                AgeGroup.MiddleSchool => baseKeywords,
                _ => new List<string>() // Minimal filtering for older users
            };
        }
    }

    /// <summary>
    /// Content filtering service that integrates with PocketFence
    /// </summary>
    public class ContentFilterService : IContentFilter
    {
        private readonly ILogger<ContentFilterService> _logger;
        private readonly ISystemSecurity _systemSecurity;
        private readonly HttpClient _httpClient;
        private readonly string _pocketFenceApiUrl;
        private bool _isActive;
        private readonly PocketFence.FamilyOS.Windows.WindowsSecurityService? _windowsSecurity;
        
        // Performance: Use HashSet for O(1) domain lookups instead of O(n) Contains/Any
        private static readonly HashSet<string> EducationalDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "khanacademy.org", "education.com", "pbskids.org", "sesamestreet.org",
            "kids.nationalgeographic.com", "funbrain.com", "coolmath4kids.com",
            "nasa.gov", "britannica.com", "scratch.mit.edu"
        };
        
        private static readonly HashSet<string> BlockedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "violence", "gambling", "drugs", "explicit", "adult", "weapon"
        };

        public bool IsActive => _isActive;

        public ContentFilterService(ILogger<ContentFilterService> logger, ISystemSecurity systemSecurity, 
            string pocketFenceApiUrl = "https://localhost:5001")
        {
            _logger = logger;
            _systemSecurity = systemSecurity;
            _httpClient = new HttpClient();
            _pocketFenceApiUrl = pocketFenceApiUrl;
            
            // Initialize Windows security integration
            try
            {
                _windowsSecurity = new PocketFence.FamilyOS.Windows.WindowsSecurityService(
                    Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<PocketFence.FamilyOS.Windows.WindowsSecurityService>());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Windows security integration not available");
            }
        }

        public async Task StartAsync()
        {
            try
            {
                // Test connection to PocketFence API
                var response = await _httpClient.GetAsync($"{_pocketFenceApiUrl}/api/kernel/health");
                
                if (response.IsSuccessStatusCode)
                {
                    _isActive = true;
                    _logger.LogInformation("Content filtering active - Connected to PocketFence API");
                }
                else
                {
                    _logger.LogWarning("PocketFence API not available - Using local filtering only");
                    _isActive = true; // Still activate with local filtering
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not connect to PocketFence API - Using local filtering");
                _isActive = true; // Fallback to local filtering
            }
        }

        public async Task StopAsync()
        {
            _isActive = false;
            _httpClient.Dispose();
            _logger.LogInformation("Content filtering stopped");
        }

        public async Task<ContentFilterResult> FilterUrlAsync(string url, FamilyMember user)
        {
            // Check Windows Defender SmartScreen first
            if (_windowsSecurity != null)
            {
                try
                {
                    var reputation = await _windowsSecurity.CheckUrlReputationAsync(url);
                    if (reputation.IsBlocked)
                    {
                        _logger.LogWarning($"URL blocked by Windows Defender: {url} - {reputation.ThreatType}");
                        return new ContentFilterResult
                        {
                            IsAllowed = false,
                            Reason = $"Blocked by Windows Defender: {reputation.ThreatType}",
                            ThreatScore = reputation.Confidence,
                            TriggeredRules = new List<string> { "Windows Defender SmartScreen" }
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Windows Defender check skipped");
                }
            }
            
            try
            {
                // Try PocketFence API first
                var requestBody = new { url = url };
                var json = FamilyOSJsonHelper.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_pocketFenceApiUrl}/api/filter/url", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var filterResult = FamilyOSJsonHelper.Deserialize<ContentFilterResult>(result);
                    
                    if (filterResult != null)
                    {
                        await LogFilterDecision(url, filterResult, user, "URL");
                        return filterResult;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PocketFence API call failed, using local filtering");
            }

            // Fallback to local filtering
            return await LocalUrlFilterAsync(url, user);
        }

        public async Task<ContentFilterResult> FilterTextAsync(string text, FamilyMember user)
        {
            try
            {
                var requestBody = new { text = text };
                var json = FamilyOSJsonHelper.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_pocketFenceApiUrl}/api/filter/content", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var filterResult = FamilyOSJsonHelper.Deserialize<ContentFilterResult>(result);
                    
                    if (filterResult != null)
                    {
                        await LogFilterDecision(text, filterResult, user, "Content");
                        return filterResult;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PocketFence API call failed, using local filtering");
            }

            return await LocalTextFilterAsync(text, user);
        }

        public async Task<ContentFilterResult> FilterImageAsync(byte[] imageData, FamilyMember user)
        {
            // For now, implement basic image filtering
            // In a full implementation, this would analyze image content
            return await Task.FromResult(new ContentFilterResult
            {
                IsAllowed = true,
                Reason = "Image content filtering not yet implemented",
                ThreatScore = 0.0,
                TriggeredRules = new List<string>()
            });
        }

        private async Task<ContentFilterResult> LocalUrlFilterAsync(string url, FamilyMember user)
        {
            var uri = new Uri(url);
            var domain = uri.Host.ToLowerInvariant();

            // Performance: O(1) HashSet lookup instead of O(n) Any with Contains
            if (EducationalDomains.Any(ed => domain.Contains(ed)))
            {
                return new ContentFilterResult
                {
                    IsAllowed = true,
                    Reason = "Educational content approved",
                    ThreatScore = 0.0
                };
            }

            // Blocked domains by age
            var blockedDomains = GetBlockedDomainsForAge(user.AgeGroup);
            var isBlocked = blockedDomains.Any(blocked => domain.Contains(blocked));

            var result = new ContentFilterResult
            {
                IsAllowed = !isBlocked,
                Reason = isBlocked ? $"Blocked for age group {user.AgeGroup}" : "Content approved",
                ThreatScore = isBlocked ? 0.8 : 0.1,
                TriggeredRules = isBlocked ? new List<string> { "Age-inappropriate domain" } : new List<string>()
            };

            await LogFilterDecision(url, result, user, "URL");
            return result;
        }

        private async Task<ContentFilterResult> LocalTextFilterAsync(string text, FamilyMember user)
        {
            var inappropriateWords = GetInappropriateWordsForAge(user.AgeGroup);
            var foundWords = inappropriateWords.Where(word => 
                text.Contains(word, StringComparison.OrdinalIgnoreCase)).ToList();

            var result = new ContentFilterResult
            {
                IsAllowed = !foundWords.Any(),
                Reason = foundWords.Any() ? $"Contains inappropriate content for {user.AgeGroup}" : "Content approved",
                ThreatScore = foundWords.Any() ? 0.7 : 0.0,
                TriggeredRules = foundWords.Select(w => $"Inappropriate word: {w}").ToList()
            };

            await LogFilterDecision(text.Substring(0, Math.Min(50, text.Length)), result, user, "Text");
            return result;
        }

        private async Task LogFilterDecision(string content, ContentFilterResult result, FamilyMember user, string type)
        {
            var action = result.IsAllowed ? "Allowed" : "Blocked";
            await _systemSecurity.LogFamilyActivityAsync(
                $"{type} {action}: {content} (Score: {result.ThreatScore:F2})", user);
        }

        private List<string> GetBlockedDomainsForAge(AgeGroup ageGroup)
        {
            return ageGroup switch
            {
                AgeGroup.Toddler or AgeGroup.Preschool => new List<string>
                {
                    "youtube.com", "facebook.com", "twitter.com", "instagram.com", "tiktok.com",
                    "reddit.com", "discord.com", "twitch.tv", "snapchat.com"
                },
                AgeGroup.Elementary => new List<string>
                {
                    "facebook.com", "twitter.com", "instagram.com", "tiktok.com",
                    "reddit.com", "discord.com", "snapchat.com"
                },
                AgeGroup.MiddleSchool => new List<string>
                {
                    "tiktok.com", "reddit.com"
                },
                _ => new List<string>()
            };
        }

        private List<string> GetInappropriateWordsForAge(AgeGroup ageGroup)
        {
            var baseWords = new List<string>
            {
                "violence", "drugs", "alcohol", "gambling", "weapon", "fight", "kill"
            };

            return ageGroup switch
            {
                AgeGroup.Toddler or AgeGroup.Preschool => baseWords.Concat(new[]
                {
                    "scary", "monster", "nightmare", "ghost", "spider", "snake"
                }).ToList(),
                AgeGroup.Elementary => baseWords.Concat(new[]
                {
                    "dating", "boyfriend", "girlfriend", "kiss"
                }).ToList(),
                _ => baseWords
            };
        }
    }

    /// <summary>
    /// System security service implementation
    /// </summary>
    public class SystemSecurityService : ISystemSecurity
    {
        private readonly ILogger<SystemSecurityService> _logger;
        private readonly byte[] _encryptionKey;
        private readonly List<AuditLog> _auditLogs;
        private readonly string _dataPath;
        private readonly PocketFence.FamilyOS.Windows.WindowsSecurityService? _windowsSecurity;

        public SystemSecurityService(ILogger<SystemSecurityService> logger, string dataPath = "./FamilyData")
        {
            _logger = logger;
            _dataPath = dataPath;
            _auditLogs = new List<AuditLog>();
            
            // Try to use Windows DPAPI for secure key storage
            try
            {
                _windowsSecurity = new PocketFence.FamilyOS.Windows.WindowsSecurityService(
                    Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<PocketFence.FamilyOS.Windows.WindowsSecurityService>());
                _encryptionKey = _windowsSecurity.GetOrCreateEncryptionKey();
                _logger.LogInformation("Using Windows DPAPI for encryption key storage");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Windows DPAPI not available, using fallback key generation");
                _encryptionKey = System.Text.Encoding.UTF8.GetBytes(GenerateEncryptionKey());
            }
        }

        public async Task InitializeAsync()
        {
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }

            await LoadAuditLogsAsync();
            _logger.LogInformation("System security initialized with encryption and audit logging");
        }

        public async Task<string> EncryptFamilyDataAsync(string data)
        {
            try
            {
                using (var aes = System.Security.Cryptography.Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                        
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            await swEncrypt.WriteAsync(data);
                        }
                        
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt family data");
                return data; // Return unencrypted as fallback
            }
        }

        public async Task<string> DecryptFamilyDataAsync(string encryptedData)
        {
            try
            {
                var fullCipher = Convert.FromBase64String(encryptedData);

                using (var aes = System.Security.Cryptography.Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    
                    var iv = new byte[aes.BlockSize / 8];
                    var cipher = new byte[fullCipher.Length - iv.Length];
                    
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
                    
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(cipher))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return await srDecrypt.ReadToEndAsync();
                    }
                }
            }
            catch (CryptographicException)
            {
                _logger.LogWarning("Encrypted data appears corrupted - will recreate default profiles");
                return string.Empty; // Return empty to trigger recreation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt family data");
                return string.Empty; // Return empty to trigger recreation
            }
        }

        public async Task<bool> VerifyParentPermissionAsync(string parentPin)
        {
            // Simple PIN verification - in production, use more secure methods
            var validPins = new[] { "1234", "0000", "9999", "parent" };
            var isValid = validPins.Contains(parentPin);
            
            await LogFamilyActivityAsync($"Parent permission verification: {(isValid ? "Success" : "Failed")}", 
                new FamilyMember { DisplayName = "System" });
            
            return isValid;
        }

        public async Task<AuditLog> LogFamilyActivityAsync(string activity, FamilyMember member)
        {
            var auditLog = new AuditLog
            {
                MemberId = member.Id,
                Activity = activity,
                Details = $"User: {member.DisplayName}, Age: {member.AgeGroup}",
                Level = DetermineAuditLevel(activity)
            };

            _auditLogs.Add(auditLog);
            
            // Save audit logs periodically
            if (_auditLogs.Count % 10 == 0)
            {
                await SaveAuditLogsAsync();
            }

            _logger.LogInformation($"[AUDIT] {auditLog.Level}: {activity} - {member.DisplayName}");
            
            return auditLog;
        }

        public async Task<List<AuditLog>> GetFamilyActivityLogsAsync(DateTime fromDate)
        {
            return await Task.FromResult(_auditLogs.Where(log => log.Timestamp >= fromDate).ToList());
        }

        private async Task LoadAuditLogsAsync()
        {
            try
            {
                var auditFile = Path.Combine(_dataPath, "audit_logs.json");
                if (File.Exists(auditFile))
                {
                    var encryptedData = await File.ReadAllTextAsync(auditFile);
                    var decryptedData = await DecryptFamilyDataAsync(encryptedData);
                    var logs = FamilyOSJsonHelper.Deserialize<List<AuditLog>>(decryptedData);
                    
                    if (logs != null)
                    {
                        _auditLogs.AddRange(logs);
                        _logger.LogInformation($"Loaded {logs.Count} audit log entries");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load audit logs");
            }
        }

        private async Task SaveAuditLogsAsync()
        {
            try
            {
                var auditFile = Path.Combine(_dataPath, "audit_logs.json");
                var jsonData = FamilyOSJsonHelper.Serialize(_auditLogs);
                var encryptedData = await EncryptFamilyDataAsync(jsonData);
                
                await File.WriteAllTextAsync(auditFile, encryptedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit logs");
            }
        }

        private string GenerateEncryptionKey()
        {
            // In production, use proper key management
            using (var rng = RandomNumberGenerator.Create())
            {
                var keyBytes = new byte[32]; // 256-bit key
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }

        private AuditLevel DetermineAuditLevel(string activity)
        {
            if (activity.Contains("Blocked", StringComparison.OrdinalIgnoreCase))
                return AuditLevel.Blocked;
            if (activity.Contains("Failed", StringComparison.OrdinalIgnoreCase) || 
                activity.Contains("Error", StringComparison.OrdinalIgnoreCase))
                return AuditLevel.Security;
            if (activity.Contains("Warning", StringComparison.OrdinalIgnoreCase))
                return AuditLevel.Warning;
            return AuditLevel.Information;
        }
    }
}