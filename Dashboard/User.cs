using System;
using System.Collections.Generic;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Represents a user account in the PocketFence system
/// </summary>
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailVerified { get; set; } = false;
    public bool PhoneVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public string? PhoneVerificationCode { get; set; }
    public DateTime? PhoneVerificationCodeExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = "Parent"; // Parent, Child, Admin
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Stores all users in the system
/// </summary>
public class UserDatabase
{
    public List<User> Users { get; set; } = new();
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
