# Session Timeout Testing Guide

## Configuration

Session timeout is set to **30 minutes** of inactivity in `DashboardService.cs`.

### Session Settings

```csharp
options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expires after 30 minutes of inactivity
options.Cookie.HttpOnly = true;                 // Prevents JavaScript access
options.Cookie.IsEssential = true;              // Required for GDPR
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS when available
options.Cookie.SameSite = SameSiteMode.Strict;  // Prevents CSRF attacks
```

## Features Implemented

### 1. Automatic Session Expiration
- Session expires after **30 minutes** of no activity
- User is redirected to login page when session expires
- Session is automatically cleared on logout

### 2. Activity Tracking
- Tracks: mouse movement, key presses, clicks, scrolling
- Each activity resets the inactivity timer
- Activity timestamp stored in session as "LastActivity"

### 3. Warning System
- Warning appears **5 minutes** before session expires
- Shows countdown timer (minutes:seconds)
- Clicking the warning resets the session timer
- Toast notification in bottom-right corner

### 4. Return URL Support
- When session expires, stores the page you were trying to access
- After re-login, automatically redirects to that page
- Prevents losing navigation context

### 5. Security Features
- HttpOnly cookies (prevents XSS attacks)
- SameSite=Strict (prevents CSRF attacks)
- Secure cookies when using HTTPS
- Constant-time password comparison

## Testing Instructions

### Test 1: Normal Session Flow
1. Start dashboard: `dotnet run dashboard`
2. Login with: admin / PocketFence2026!
3. Navigate through pages - should work normally
4. Logout - session cleared, redirected to login

**Expected:** ✅ Normal operation

### Test 2: Session Timeout (Quick Test)

**Option A: Modify timeout for testing**
```csharp
// In DashboardService.cs, temporarily change:
options.IdleTimeout = TimeSpan.FromMinutes(2); // 2 minutes for testing
```

**Steps:**
1. Build and run: `dotnet build && dotnet run dashboard`
2. Login and navigate to any page
3. Don't touch mouse/keyboard for 2 minutes
4. Warning should appear at 1:30 remaining
5. Wait another 2 minutes without activity
6. Should be auto-logged out

**Expected:**
- ⏰ Warning appears 5 minutes before timeout
- 🔄 Countdown updates every 10 seconds
- 🚪 Auto-logout when time expires
- 🔐 Redirect to login page

### Test 3: Activity Reset
1. Login and navigate to dashboard
2. Wait until warning appears (25 minutes)
3. Move mouse or click anywhere
4. Warning should disappear
5. Timer resets to 30 minutes

**Expected:** ✅ Activity resets timer

### Test 4: Return URL
1. Ensure you're logged out
2. Try to access: `http://localhost:5000/blocked`
3. Should redirect to login
4. After successful login
5. Should return to `/blocked` page

**Expected:** ✅ Returns to intended page

### Test 5: Multiple Tabs
1. Open dashboard in two browser tabs
2. Login in Tab 1
3. Tab 2 should also be authenticated (same session)
4. Logout in Tab 1
5. Refresh Tab 2 - should require login

**Expected:** ✅ Session shared across tabs

## Manual Testing

### Quick Test (2-minute timeout):

```powershell
# 1. Edit DashboardService.cs timeout to 2 minutes
# 2. Build and run
dotnet build
dotnet run dashboard

# 3. In browser:
# - Login at http://localhost:5000
# - Wait 1.5 minutes → warning appears
# - Wait 2.5 total minutes → auto logout
```

### Production Test (30-minute timeout):

```powershell
# Run with default 30-minute timeout
dotnet run dashboard

# Test warning at 25 minutes of inactivity
# Test logout at 30 minutes
```

## Session Storage

Session data is stored in:
- `IsAuthenticated` - "true" when logged in
- `Username` - Current username
- `LastActivity` - ISO 8601 timestamp of last activity

## Security Best Practices

✅ **Implemented:**
- 30-minute timeout (OWASP recommended: 15-30 minutes)
- HttpOnly cookies
- SameSite=Strict
- Activity tracking
- Warning before timeout
- Return URL support
- Secure password hashing

🔒 **Production Recommendations:**
- Enable HTTPS (Secure cookies)
- Consider shorter timeout for high-security scenarios
- Implement "Remember Me" with longer-lived tokens
- Add rate limiting on login attempts
- Log session events for auditing

## Troubleshooting

**Warning doesn't appear:**
- Check browser console for JavaScript errors
- Verify Bootstrap Toast is loaded
- Check if sessionWarningToast element exists

**Session expires too quickly:**
- Check `IdleTimeout` in DashboardService.cs
- Verify activity tracking is working
- Check browser dev tools → Application → Cookies

**Return URL not working:**
- Check Login.cshtml.cs `OnPost` method
- Verify hidden `returnUrl` field in form
- Check browser dev tools → Network → request payload

## Files Modified

1. `DashboardService.cs` - Session configuration
2. `Login.cshtml.cs` - Return URL support
3. `Login.cshtml` - Return URL form field
4. `_Layout.cshtml` - Session warning toast
5. `site.js` - Activity tracking and warning system
6. `AuthenticatedPageModel.cs` - Base authentication model (new)

## Next Steps

- [ ] Add "Remember Me" functionality
- [ ] Implement session activity logging
- [ ] Add configurable timeout in Settings
- [ ] Create admin panel for session management
- [ ] Add multi-factor authentication (MFA)
