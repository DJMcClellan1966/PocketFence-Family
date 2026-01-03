# Network Access Setup Guide

## What Changed

✅ **Child Role Added:**
- Registration now asks for account type (Parent/Guardian or Child)
- Children are automatically redirected to a simplified dashboard
- Children cannot access parent settings, statistics, or controls
- Parents and Admins have full access

✅ **Network Accessibility:**
- Dashboard now listens on all network interfaces (0.0.0.0:5000)
- Can be accessed from any device on your local network
- Email verification links use configurable base URL

## How to Use

### 1. Start the Dashboard
```powershell
dotnet run dashboard
```

You'll see output like:
```
🛡️  PocketFence Dashboard started!
📡 Local:    http://localhost:5000
📡 Network:  http://192.168.1.100:5000
📝 Login with: admin / PocketFence2026!
```

### 2. Access from Other Devices

**From your computer:**
- Use: http://localhost:5000

**From phones, tablets, other computers:**
- Use: http://192.168.1.100:5000 (replace with your actual IP)
- Make sure devices are on the same Wi-Fi network

### 3. Firewall Configuration (Windows)

If other devices can't connect, allow port 5000:

```powershell
# Run as Administrator
New-NetFirewallRule -DisplayName "PocketFence Dashboard" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

### 4. Set Base URL for Email Links

For email verification links to work from other devices:

**Windows PowerShell:**
```powershell
$env:POCKETFENCE_BASE_URL = "http://192.168.1.100:5000"
dotnet run dashboard
```

**Or permanently (Windows):**
```powershell
[System.Environment]::SetEnvironmentVariable("POCKETFENCE_BASE_URL", "http://192.168.1.100:5000", "User")
```

### 5. Create Child Accounts

1. Go to registration page
2. Fill in details
3. Select **"Child"** as account type
4. Child will see simplified dashboard with:
   - Blocked content statistics
   - Internet safety tips
   - Cannot access parent settings

### 6. Find Your IP Address

```powershell
# Windows
ipconfig | Select-String "IPv4"

# Or dashboard shows it when starting
```

## Security Notes

⚠️ **Important:**
- This setup works on your local network only (not internet-accessible)
- Children can only see their own blocked content (simplified view)
- Parents can see full details and manage all settings
- Session timeout is 30 minutes
- Rate limiting prevents brute force attacks

## Troubleshooting

**Can't connect from another device:**
1. Check firewall (allow port 5000)
2. Verify devices are on same network
3. Try disabling Windows Defender Firewall temporarily to test
4. Ensure dashboard is running

**Email links don't work on other devices:**
- Set POCKETFENCE_BASE_URL environment variable to your network IP

**Child sees parent dashboard:**
- Make sure role was set to "Child" during registration
- Check session - logout and login again
- Verify role in users.json file

## Account Types

**Admin:**
- Full system access
- Can manage all users
- Cannot be deleted (last admin)

**Parent:**
- Full dashboard access
- Can view all statistics
- Can modify settings
- Can manage content filters

**Child:**
- Limited dashboard
- Can see own blocked content (sanitized)
- Cannot access settings
- Cannot see detailed threat information
- Internet safety tips displayed

## Next Steps

1. Create parent accounts for family guardians
2. Create child accounts for kids
3. Test access from phones/tablets
4. Configure firewall if needed
5. Set up email base URL for network access
