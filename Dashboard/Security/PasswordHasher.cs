using System.Security.Cryptography;
using System.Text;

namespace PocketFence_AI.Dashboard.Security;

/// <summary>
/// Provides secure password hashing and verification using PBKDF2
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a password using PBKDF2
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns>Base64 encoded hash with salt</returns>
    public static string HashPassword(string password)
    {
        // Generate random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );

        // Combine salt and hash: salt + hash
        byte[] hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        // Return as Base64 string
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against a hashed password
    /// </summary>
    /// <param name="password">The password to verify</param>
    /// <param name="hashedPassword">The hashed password (Base64 encoded)</param>
    /// <returns>True if password matches, false otherwise</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            // Decode the Base64 hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract the salt (first 16 bytes)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract the hash (remaining bytes)
            byte[] storedHash = new byte[KeySize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, KeySize);

            // Hash the input password with the same salt
            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            // Compare the hashes using constant-time comparison
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a secure random password
    /// </summary>
    /// <param name="length">Length of the password (default: 16)</param>
    /// <returns>A secure random password</returns>
    public static string GenerateSecurePassword(int length = 16)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        byte[] randomBytes = RandomNumberGenerator.GetBytes(length);
        
        StringBuilder password = new StringBuilder(length);
        foreach (byte b in randomBytes)
        {
            password.Append(validChars[b % validChars.Length]);
        }
        
        return password.ToString();
    }
}
