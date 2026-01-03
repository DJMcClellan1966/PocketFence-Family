using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PocketFence_AI.Dashboard.Security;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Manages user accounts with secure storage and operations
/// </summary>
public class UserManager
{
    private readonly string _usersFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private UserDatabase? _cachedDatabase;

    public UserManager(string? dataDirectory = null)
    {
        var directory = dataDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        Directory.CreateDirectory(directory);
        _usersFilePath = Path.Combine(directory, "users.json");
    }

    /// <summary>
    /// Load users database from file
    /// </summary>
    private async Task<UserDatabase> LoadDatabaseAsync()
    {
        // Return cached version if available
        if (_cachedDatabase != null)
            return _cachedDatabase;

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!File.Exists(_usersFilePath))
            {
                // Create default admin user on first run
                var database = new UserDatabase();
                var adminUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",
                    PasswordHash = PasswordHasher.HashPassword("PocketFence2026!"),
                    Email = "",
                    Role = "Admin",
                    IsActive = true,
                    EmailVerified = true, // Admin doesn't require verification
                    CreatedAt = DateTime.UtcNow
                };
                database.Users.Add(adminUser);
                
                // Save to file
                await File.WriteAllTextAsync(_usersFilePath, 
                    JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true }))
                    .ConfigureAwait(false);
                
                Console.WriteLine($"[UserManager] Created default admin user in {_usersFilePath}");
                _cachedDatabase = database;
                return database;
            }

            var json = await File.ReadAllTextAsync(_usersFilePath).ConfigureAwait(false);
            var loaded = JsonSerializer.Deserialize<UserDatabase>(json) ?? new UserDatabase();
            _cachedDatabase = loaded;
            return loaded;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Save users database to file
    /// </summary>
    private async Task SaveDatabaseAsync(UserDatabase database)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            database.LastModified = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_usersFilePath, json).ConfigureAwait(false);
            _cachedDatabase = database; // Update cache
            Console.WriteLine($"[UserManager] Saved users database to {_usersFilePath}");
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> CreateUserAsync(
        string username, string password, string email, string? phoneNumber = null, string role = "Parent")
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, "Username must be at least 3 characters long", null);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return (false, "Password must be at least 8 characters long", null);

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return (false, "Valid email address is required", null);

        if (role != "Parent" && role != "Child" && role != "Admin")
            return (false, "Invalid role specified", null);

        // Load database
        var database = await LoadDatabaseAsync().ConfigureAwait(false);

        // Check if username or email already exists
        if (database.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return (false, "Username already exists", null);

        if (database.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return (false, "Email address already registered", null);

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            PasswordHash = PasswordHasher.HashPassword(password),
            Email = email.ToLowerInvariant(),
            PhoneNumber = phoneNumber,
            Role = role,
            IsActive = true,
            EmailVerified = false,
            EmailVerificationToken = GenerateToken(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        // Generate phone verification code if phone provided
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            user.PhoneVerificationCode = GenerateVerificationCode();
            user.PhoneVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        }

        database.Users.Add(user);
        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] Created new user: {username} ({email})");
        return (true, "Account created successfully", user);
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> AuthenticateAsync(string username, string password)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return (false, "Invalid username or password", null);

        if (!user.IsActive)
            return (false, "Account is disabled", null);

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            return (false, "Invalid username or password", null);

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await SaveDatabaseAsync(database).ConfigureAwait(false);

        return (true, "Login successful", user);
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        return database.Users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        return database.Users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        return database.Users.FirstOrDefault(u => u.Id == userId);
    }

    /// <summary>
    /// Update user's email
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateEmailAsync(string userId, string newEmail)
    {
        if (!IsValidEmail(newEmail))
            return (false, "Invalid email format");

        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.Id == userId);
        
        if (user == null)
            return (false, "User not found");

        // Check if email already in use by another user
        if (database.Users.Any(u => u.Id != userId && u.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase)))
            return (false, "Email already in use");

        user.Email = newEmail.ToLowerInvariant();
        user.EmailVerified = false; // Require re-verification
        user.EmailVerificationToken = GenerateToken();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await SaveDatabaseAsync(database).ConfigureAwait(false);
        return (true, "Email updated. Verification email sent.");
    }

    /// <summary>
    /// Update user's password
    /// </summary>
    public async Task<(bool Success, string Message)> UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        if (newPassword.Length < 8)
            return (false, "New password must be at least 8 characters long");

        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.Id == userId);
        
        if (user == null)
            return (false, "User not found");

        // Verify current password
        if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] Password updated for user: {user.Username}");
        return (true, "Password updated successfully");
    }

    /// <summary>
    /// Update user's username
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateUsernameAsync(string userId, string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername) || newUsername.Length < 3)
            return (false, "Username must be at least 3 characters long");

        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.Id == userId);
        
        if (user == null)
            return (false, "User not found");

        // Check if username already in use
        if (database.Users.Any(u => u.Id != userId && u.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
            return (false, "Username already in use");

        user.Username = newUsername;
        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] Username updated to: {newUsername}");
        return (true, "Username updated successfully");
    }

    /// <summary>
    /// Verify email with token
    /// </summary>
    public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string token)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return (false, "User not found");

        if (user.EmailVerified)
            return (true, "Email already verified");

        if (user.EmailVerificationToken != token)
            return (false, "Invalid verification token");

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            return (false, "Verification token has expired");

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await SaveDatabaseAsync(database).ConfigureAwait(false);
        Console.WriteLine($"[UserManager] Email verified for: {user.Username}");
        return (true, "Email verified successfully");
    }

    /// <summary>
    /// Generate password reset token
    /// </summary>
    public async Task<(bool Success, string Message, string? Token)> GeneratePasswordResetTokenAsync(string email)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return (false, "No account found with that email", null);

        var token = GenerateToken();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry

        await SaveDatabaseAsync(database).ConfigureAwait(false);
        Console.WriteLine($"[UserManager] Password reset token generated for: {user.Username}");
        return (true, "Password reset token generated", token);
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        if (newPassword.Length < 8)
            return (false, "Password must be at least 8 characters long");

        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return (false, "User not found");

        if (user.PasswordResetToken != token)
            return (false, "Invalid reset token");

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return (false, "Reset token has expired");

        // Check if new password is same as current password
        if (PasswordHasher.VerifyPassword(newPassword, user.PasswordHash))
            return (false, "New password cannot be the same as your current password");

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await SaveDatabaseAsync(database).ConfigureAwait(false);
        Console.WriteLine($"[UserManager] Password reset for: {user.Username}");
        return (true, "Password reset successfully");
    }

    /// <summary>
    /// Get all users (admin only)
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        return database.Users.ToList();
    }

    /// <summary>
    /// Delete user account
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteUserAsync(string userId)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return (false, "User not found");

        // Prevent deleting the last admin
        if (user.Role == "Admin" && database.Users.Count(u => u.Role == "Admin") == 1)
            return (false, "Cannot delete the last admin account");

        database.Users.Remove(user);
        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] Deleted user: {user.Username}");
        return (true, "User deleted successfully");
    }

    /// <summary>
    /// Update user's phone number and generate verification code
    /// </summary>
    public async Task<(bool Success, string Message, string? VerificationCode)> UpdatePhoneAsync(string userId, string newPhone)
    {
        if (string.IsNullOrWhiteSpace(newPhone))
            return (false, "Phone number is required", null);

        // Basic phone format validation (at least 10 digits)
        var digitsOnly = new string(newPhone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length < 10)
            return (false, "Phone number must have at least 10 digits", null);

        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.Id == userId);
        
        if (user == null)
            return (false, "User not found", null);

        // Check if phone already in use by another user
        if (database.Users.Any(u => u.Id != userId && u.PhoneNumber == newPhone))
            return (false, "Phone number already in use", null);

        // Generate 6-digit verification code
        var code = GenerateVerificationCode();
        
        user.PhoneNumber = newPhone;
        user.PhoneVerified = false; // Require re-verification
        user.PhoneVerificationCode = code;
        user.PhoneVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10); // 10-minute expiry

        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] Phone updated for {user.Username}, verification code: {code}");
        return (true, "Phone updated, verification code sent", code);
    }

    /// <summary>
    /// Generate a new phone verification code for existing phone
    /// </summary>
    public async Task<(bool Success, string Message, string? VerificationCode)> GeneratePhoneVerificationCodeAsync(string userId)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.Id == userId);
        
        if (user == null)
            return (false, "User not found", null);

        if (string.IsNullOrEmpty(user.PhoneNumber))
            return (false, "No phone number on file", null);

        if (user.PhoneVerified)
            return (false, "Phone already verified", null);

        // Generate new 6-digit verification code
        var code = GenerateVerificationCode();
        
        user.PhoneVerificationCode = code;
        user.PhoneVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);

        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] New verification code generated for {user.Username}: {code}");
        return (true, "Verification code generated", code);
    }

    /// <summary>
    /// Verify phone with 6-digit code
    /// </summary>
    public async Task<(bool Success, string Message)> VerifyPhoneAsync(string phoneNumber, string code)
    {
        var database = await LoadDatabaseAsync().ConfigureAwait(false);
        var user = database.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);

        if (user == null)
            return (false, "User not found");

        if (user.PhoneVerified)
            return (true, "Phone already verified");

        if (user.PhoneVerificationCode != code)
            return (false, "Invalid verification code");

        if (user.PhoneVerificationCodeExpiry < DateTime.UtcNow)
            return (false, "Verification code has expired");

        user.PhoneVerified = true;
        user.PhoneVerificationCode = null;
        user.PhoneVerificationCodeExpiry = null;

        await SaveDatabaseAsync(database).ConfigureAwait(false);

        Console.WriteLine($"[UserManager] Phone verified for: {user.Username}");
        return (true, "Phone verified successfully");
    }

    // Helper methods
    private static string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateVerificationCode()
    {
        // Generate random 6-digit code
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0);
        return (number % 1000000).ToString("D6"); // Ensures 6 digits with leading zeros
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
