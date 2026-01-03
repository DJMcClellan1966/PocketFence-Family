# Input Security & Buffer Overflow Protection

## Overview

The PocketFence Dashboard implements multiple layers of protection against buffer overflow attacks, DoS attacks, and input validation exploits.

## Security Measures Implemented

### 1. **Buffer Overflow Protection**

✅ **C# Managed Memory**
- C# uses managed strings with automatic memory management
- No fixed-size buffers that can overflow
- Garbage collector handles memory allocation/deallocation
- Built-in bounds checking on all array/string operations

✅ **Length Limits**
```csharp
const int MaxUsernameLength = 50;
const int MaxPasswordLength = 128;
```
- Username: 50 characters max
- Password: 128 characters max (sufficient for strong passwords)
- Prevents DoS attacks via excessive input

### 2. **Input Validation (Defense in Depth)**

#### Client-Side Validation (HTML)
```html
<input type="text" maxlength="50" required autofocus>
<input type="password" maxlength="128" required>
```
- Browser enforces max length
- Required fields prevent empty submissions
- First line of defense (user experience)

#### Server-Side Validation (C#)
```csharp
// Null/whitespace check
if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    return Error();

// Length validation
if (username.Length > MaxUsernameLength)
    return Error();

// Sanitization
username = username.Trim();
```
- Always validates on server (never trust client)
- Explicit length checks
- Input sanitization

### 3. **Additional Security Features**

✅ **Open Redirect Prevention**
```csharp
if (!string.IsNullOrEmpty(returnUrl) && !returnUrl.StartsWith("/"))
{
    returnUrl = "/"; // Force local redirect only
}
```

✅ **Autocomplete Attributes**
```html
autocomplete="username"
autocomplete="current-password"
```
- Helps password managers
- Improves security and UX

✅ **Session Security**
- HttpOnly cookies (prevents XSS)
- SameSite=Strict (prevents CSRF)
- 30-minute timeout
- Secure flag when using HTTPS

## Why C# is Buffer-Overflow Safe

### Managed Memory Model
1. **No Direct Memory Access**
   - Can't access memory outside allocated bounds
   - No pointer arithmetic on strings
   - Runtime checks all array/string accesses

2. **Automatic Bounds Checking**
   ```csharp
   string s = "test";
   char c = s[10]; // Throws IndexOutOfRangeException
   ```

3. **Immutable Strings**
   - Strings are immutable in C#
   - String operations create new instances
   - No risk of overwriting adjacent memory

4. **Type Safety**
   - Strong typing prevents type confusion
   - No implicit unsafe casts
   - Compiler enforces type rules

## Comparison with C/C++

| Feature | C/C++ | C# (.NET) |
|---------|-------|-----------|
| Fixed buffers | `char buf[10]` - Can overflow | Managed arrays - Cannot overflow |
| Bounds checking | Manual (or not at all) | Automatic (runtime enforced) |
| Memory management | Manual (malloc/free) | Automatic (GC) |
| Buffer overruns | Common vulnerability | Virtually impossible |
| String safety | `strcpy()` unsafe | All operations bounds-checked |

## Attack Vectors Prevented

### ✅ Buffer Overflow
- **Attack**: Send 10MB username to crash server
- **Prevention**: Reject inputs > 50 chars (username) or 128 chars (password)

### ✅ DoS via Memory Exhaustion
- **Attack**: Send millions of 1MB passwords
- **Prevention**: 128 char limit + session rate limiting

### ✅ Open Redirect
- **Attack**: `returnUrl=https://evil.com`
- **Prevention**: Only allow local URLs starting with `/`

### ✅ SQL Injection
- **Attack**: `username: admin' OR '1'='1`
- **Prevention**: No SQL database (uses JSON), input sanitization, parameterized queries when DB added

### ✅ XSS (Cross-Site Scripting)
- **Attack**: `<script>alert('xss')</script>` in username
- **Prevention**: Razor Pages automatically HTML-encodes output, CSP headers

### ✅ CSRF (Cross-Site Request Forgery)
- **Attack**: Malicious site submits login form
- **Prevention**: SameSite=Strict cookies, antiforgery tokens

## Security Best Practices Applied

1. ✅ **Defense in Depth** - Multiple validation layers
2. ✅ **Principle of Least Privilege** - Minimal permissions
3. ✅ **Input Validation** - Client + Server validation
4. ✅ **Output Encoding** - Automatic in Razor Pages
5. ✅ **Secure Defaults** - HttpOnly, SameSite, Secure cookies
6. ✅ **Error Handling** - Generic error messages (no info leakage)
7. ✅ **Session Management** - Timeout, secure cookies
8. ✅ **Password Security** - PBKDF2 hashing, no plaintext storage

## Testing

### Buffer Overflow Test
```powershell
# Test excessive username length
curl -X POST http://localhost:5000/login `
  -d "username=$('a' * 1000)&password=test"

# Expected: Error message "Username is too long"
```

### Length Limit Test
```powershell
# Test password length limit
$longPassword = 'a' * 200
curl -X POST http://localhost:5000/login `
  -d "username=admin&password=$longPassword"

# Expected: Error message "Password is too long"
```

### Open Redirect Test
```powershell
# Test external redirect prevention
curl -X POST http://localhost:5000/login `
  -d "username=admin&password=PocketFence2026!&returnUrl=https://evil.com"

# Expected: Redirects to "/" not "https://evil.com"
```

## Code Review Checklist

- [x] No unsafe code blocks
- [x] No fixed-size buffers
- [x] All inputs validated
- [x] Length limits enforced
- [x] Client + server validation
- [x] Output encoding enabled
- [x] Secure session management
- [x] No SQL injection risks
- [x] CSRF protection enabled
- [x] XSS protection enabled

## Future Enhancements

- [x] Add rate limiting (e.g., 5 failed login attempts) ✅
- [ ] Implement CAPTCHA after failed attempts
- [ ] Add IP-based blocking
- [x] Implement Content Security Policy (CSP) headers ✅
- [x] Add security headers (X-Frame-Options, etc.) ✅
- [ ] Implement multi-factor authentication (MFA)
- [ ] Add input sanitization library (e.g., HtmlSanitizer)
- [x] Implement audit logging for security events ✅

## References

- [OWASP Input Validation Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [CWE-120: Buffer Overflow](https://cwe.mitre.org/data/definitions/120.html)
- [.NET Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
