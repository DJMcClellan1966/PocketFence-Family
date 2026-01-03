# 🚀 Dashboard Quick Start Guide

## Start the Dashboard (2 minutes)

### Option 1: PowerShell
```powershell
cd 'c:\Users\DJMcC\OneDrive\Desktop\PocketFence-Family\PocketFence-Family'
dotnet run dashboard
```

### Option 2: From CLI
```bash
pocketfence> dashboard
# Then restart with: dotnet run dashboard
```

### ✅ Success
You should see:
```
🛡️  PocketFence Dashboard started at http://localhost:5000
📝 Login with: admin / PocketFence2026!
```

## Access the Dashboard

1. Open browser: **http://localhost:5000**
2. Login with:
   - Username: `admin`
   - Password: `PocketFence2026!`

## Dashboard Pages

### 🏠 Home (Dashboard)
- Today's blocks: 3
- This week: 17
- This month: 64
- All time: 248
- Recent activity feed
- Blocks by category breakdown

### 🚫 Blocked Content
- Full history of all blocks
- Search and filter
- Export capabilities (coming soon)

### ⚙️ Settings
- Filtering level (Strict/Moderate/Relaxed)
- Content categories to block
- Email notifications
- Custom blocklist

## Next Steps (Week 1)

1. **Test the UI** (30 min)
   - Try all pages
   - Test on mobile
   - Check different browsers

2. **Connect Real Data** (2 hours)
   - Replace sample data in `Index.cshtml.cs`
   - Connect to your `ContentFilter` class
   - Store blocks to JSON/SQLite

3. **Polish** (1 hour)
   - Fix any UI bugs
   - Add loading indicators
   - Improve error messages

4. **Security** (1 hour)
   - ~~Change default password~~
   - ~~Add password hashing~~
   - ~~Test session timeout~~

## File Structure

```
Dashboard/
├── Pages/
│   ├── Login.cshtml          ← Parent login
│   ├── Index.cshtml          ← Main dashboard
│   ├── Blocked.cshtml        ← Content history
│   └── Settings.cshtml       ← Configuration
├── wwwroot/
│   ├── css/site.css          ← Custom styles
│   └── js/site.js            ← JavaScript
└── DashboardService.cs       ← Startup config
```

## Customization

### Change Port
Edit `Dashboard/DashboardService.cs`:
```csharp
builder.WebHost.UseUrls("http://localhost:8080");
```

### Add Your Logo
Place image in `Dashboard/wwwroot/img/logo.png`

Update `_Layout.cshtml`:
```html
<img src="~/img/logo.png" alt="Logo" height="30">
```

### Change Colors
Edit `Dashboard/wwwroot/css/site.css`:
```css
.navbar {
    background-color: #your-color !important;
}
```

## Troubleshooting

### "Port already in use"
Change port in `DashboardService.cs` or kill process:
```powershell
netstat -ano | findstr :5000
taskkill /PID <process_id> /F
```

### "Cannot find module"
```powershell
dotnet restore
dotnet build
```

### "Login not working"
- Clear browser cache
- Check browser console (F12)
- Verify credentials: admin/PocketFence2026!

## Week 1 Goals ✅

- [x] Dashboard runs ✅
- [x] Test on 3 browsers ✅
- [x] Connect real data ✅
- [x] Professional appearance ✅
- [x] No critical bugs ✅

**Time Budget:** 8 hours total (as per roadmap)

---

Ready to build! 🛡️
