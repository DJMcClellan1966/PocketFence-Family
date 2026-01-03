# Password Hashing Utility

This utility generates secure password hashes for PocketFence dashboard authentication.

## Security Features

- **PBKDF2** algorithm with SHA256
- **100,000 iterations** (OWASP recommended)
- **256-bit key** (32 bytes)
- **128-bit salt** (16 bytes) - unique per password
- **Constant-time comparison** to prevent timing attacks

## Quick Start

### Generate Password Hash

Run the dashboard in debug mode and login with the default password. The console will output:

```
🔐 Generated password hash: [BASE64_HASH_STRING]
   Copy this hash to HashedAdminPassword constant for production!
```

### How It Works

1. **Development Mode** (#if DEBUG):
   - Accepts plaintext "PocketFence2026!"
   - Generates and logs the hash
   - Still allows login for testing

2. **Production Mode** (Release build):
   - Only accepts hashed passwords
   - Verifies using `PasswordHasher.VerifyPassword()`
   - Plaintext passwords rejected

## Implementation Details

### PasswordHasher.cs

Located in `Dashboard/Security/PasswordHasher.cs`

**Methods:**
- `HashPassword(string password)` - Creates secure hash
- `VerifyPassword(string password, string hashedPassword)` - Validates password
- `GenerateSecurePassword(int length)` - Creates random secure password

### Login.cshtml.cs

Uses `VerifyAdminPassword()` method:
- Checks plaintext in DEBUG mode
- Verifies hash in production
- Logs hash generation for setup

## Usage Example

```csharp
// Hash a password
string hash = PasswordHasher.HashPassword("MySecurePassword123!");

// Verify a password
bool isValid = PasswordHasher.VerifyPassword("MySecurePassword123!", hash);

// Generate random password
string randomPass = PasswordHasher.GenerateSecurePassword(16);
```

## For Production

1. Build in Release mode: `dotnet build -c Release`
2. Run dashboard: `dotnet run dashboard`
3. Login with "PocketFence2026!"
4. Copy the generated hash from console
5. Update `HashedAdminPassword` constant in Login.cshtml.cs
6. Uncomment the hash verification line
7. Remove plaintext check

## Security Best Practices

✅ Passwords never stored in plaintext  
✅ Each hash has unique salt  
✅ Industry-standard algorithms (PBKDF2-SHA256)  
✅ Constant-time comparison prevents timing attacks  
✅ 100,000 iterations slow down brute-force attacks  

## Future Enhancements

- User management system
- Password change functionality
- Multi-factor authentication (MFA)
- Password complexity requirements
- Account lockout after failed attempts
- Password expiration policies
