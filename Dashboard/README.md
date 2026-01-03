# PocketFence Dashboard

ASP.NET Core Razor Pages dashboard for PocketFence-AI parent monitoring.

## 🚀 Quick Start

### Run the Dashboard

```powershell
# From project root
dotnet run dashboard
```

Then open browser to: **http://localhost:5000**

### Default Login
- **Username:** `admin`
- **Password:** `admin`

⚠️ **Change these credentials in production!**

## 📁 Structure

```
Dashboard/
├── Pages/
│   ├── Login.cshtml         # Parent login page
│   ├── Index.cshtml         # Main dashboard with stats
│   ├── Blocked.cshtml       # Blocked content history
│   ├── Settings.cshtml      # Configuration page
│   └── *.cshtml.cs          # Page models
├── wwwroot/
│   ├── css/site.css         # Custom styling
│   └── js/site.js           # Client-side JavaScript
└── DashboardService.cs      # Dashboard startup logic
```

## 🎨 Features

### ✅ Implemented (MVP)
- [x] Parent login with session authentication
- [x] Dashboard with statistics (today/week/month/all-time)
- [x] Recent blocks view
- [x] Blocks by category breakdown
- [x] Blocked content history page with search
- [x] Settings page for filtering configuration
- [x] Responsive Bootstrap 5 UI
- [x] Clean, professional design

### 🔜 Todo (Post-MVP)
- [ ] Connect to real ContentFilter data
- [ ] Persistent storage (SQLite or JSON files)
- [ ] Email notifications for blocks
- [ ] Family member profiles
- [ ] Schedule-based filtering (bedtime mode)
- [ ] Export reports (PDF/CSV)
- [ ] Two-factor authentication
- [ ] Activity charts/graphs

## 🔧 Development

### Add New Page

1. Create `NewPage.cshtml` in `Dashboard/Pages/`:
```html
@page
@model NewPageModel
@{ ViewData["Title"] = "New Page"; }

<h1>Your content here</h1>
```

2. Create `NewPage.cshtml.cs`:
```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

public class NewPageModel : PageModel
{
    public void OnGet() { }
}
```

3. Add to navigation in `_Layout.cshtml`

### Styling

Edit `wwwroot/css/site.css` for custom styles. Bootstrap 5 is already included via CDN.

### Connect to Real Data

Replace sample data in `Index.cshtml.cs`:

```csharp
private void LoadSampleData()
{
    // Replace with:
    var blocks = _filter.GetRecentBlocks();
    BlockedToday = blocks.Count(b => b.Time.Date == DateTime.Today);
    // etc...
}
```

## 🔒 Security Notes

### For MVP (Week 1-2)
- ✅ Simple hardcoded login (admin/PocketFence2026!)
- ✅ Session-based authentication
- ✅ HttpOnly cookies
- ⚠️ **NOT production-ready!**

### For Production (Week 3+)
- [ ] Hash passwords with bcrypt/Argon2
- [ ] Use ASP.NET Identity for user management
- [ ] Add CSRF protection
- [ ] Enable HTTPS only
- [ ] Rate limiting on login attempts
- [ ] Audit logging

## 📊 Integration Points

### Current (Sample Data)
Dashboard shows hardcoded sample data for demo purposes.

### Production Integration
Connect dashboard to your filtering service:

```csharp
// In DashboardService.cs
builder.Services.AddSingleton<ContentFilter>();
builder.Services.AddSingleton<SimpleAI>();

// In Index.cshtml.cs
private readonly ContentFilter _filter;
public IndexModel(ContentFilter filter)
{
    _filter = filter;
}
```

## 🎯 Week 1 Checklist

- [x] Basic login page
- [x] Dashboard with stats cards
- [x] Blocked content table
- [x] Settings page
- [x] Bootstrap styling
- [x] Session authentication
- [ ] Test on 3 browsers (Chrome, Edge, Firefox)
- [ ] Add real data integration
- [ ] Fix any UI bugs

**Estimated Time:** 8 hours (as planned in roadmap)

## 🐛 Troubleshooting

### Dashboard won't start
```powershell
# Make sure you're using .NET 8.0
dotnet --version

# Restore packages
dotnet restore

# Try running again
dotnet run dashboard
```

### Port 5000 already in use
Edit `DashboardService.cs` line:
```csharp
builder.WebHost.UseUrls("http://localhost:5001");  // Change port
```

### Can't login
- Default credentials: `admin` / `admin`
- Clear browser cache/cookies
- Check browser console for errors

## 📚 Resources

- [ASP.NET Core Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Bootstrap 5 Docs](https://getbootstrap.com/docs/5.3/)
- [Session State in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state)

---

**Next Steps:** Follow Week 1 roadmap to complete MVP and launch! 🚀
