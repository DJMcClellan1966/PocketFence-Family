# PocketFence-Family - Current Status

**Date:** January 5, 2026  
**Phase:** 1 - Smart Setup Guide  
**Focus:** Building AI recommendation engine for parental control setup

---

## ✅ What's Working

### Dashboard Foundation
- ✅ Web server running on http://192.168.1.114:5000
- ✅ User authentication (Admin/Parent/Child roles)
- ✅ Session management
- ✅ Network accessible from any device
- ✅ Bootstrap 5 responsive UI
- ✅ SimpleAI engine (local threat detection)

### Pages
- ✅ `/` - Home/Login
- ✅ `/Dashboard` - Parent dashboard (stats, activity)
- ✅ `/Blocked` - Blocked content log
- ✅ `/Settings` - Configuration
- ✅ `/Account` - User profile management

### Security
- ✅ Rate limiting (5 attempts, 15-min lockout)
- ✅ Security audit logging
- ✅ 30-minute session timeout
- ✅ Password hashing (PBKDF2)

---

## 📦 What We Archived (Not Needed)

Moved to `/Archive` folder - may use later if we build companion apps:

- ❌ OAuth integration code (Apple, Google, Microsoft)
- ❌ DeviceControl provider interfaces
- ❌ iOS/Android/Windows API client stubs
- ❌ Device linking pages
- ❌ Apple OAuth documentation

**Why archived:** These APIs don't exist or require native apps. Our new approach (guides + AI recommendations) doesn't need them.

---

## 🎯 Current Sprint: Setup Wizard (Jan 5-12)

### What We're Building This Week:

**1. Setup Wizard Page** `/Setup/Start`
```
[ ] Device type picker (iOS / Android / Windows)
[ ] Child age input (0-17)
[ ] Optional: Concerns (social media, gaming, etc.)
[ ] "Get Recommendations" button
```

**2. AI Recommendation Engine**
```
[ ] Age-based templates in SimpleAI
[ ] Device-specific recommendation logic
[ ] Generate checklist of settings to enable
[ ] Provide explanations for each recommendation
```

**3. Recommendation Display Page** `/Setup/Recommendations`
```
[ ] Show personalized checklist
[ ] "Why this matters" explanations
[ ] Copy-paste ready lists (apps to block)
[ ] Link to step-by-step guides
```

**4. Guide Templates (Start with iOS)**
```
[ ] Create guide format (screenshots + text)
[ ] "How to Enable Screen Time" guide
[ ] "How to Block Apps by Category" guide
[ ] Store guides in Guides/ folder
```

---

## 📂 Project Structure

```
PocketFence-Family/
├── Dashboard/               # Web application
│   ├── Pages/              # Razor pages
│   │   ├── Index.cshtml    # Home/Login
│   │   ├── Dashboard.cshtml # Parent dashboard
│   │   ├── Blocked.cshtml  # Activity log
│   │   ├── Settings.cshtml # Configuration
│   │   ├── Account.cshtml  # User profile
│   │   └── Setup/          # NEW - Setup wizard (to create)
│   ├── wwwroot/            # Static files (CSS, JS, images)
│   ├── Security/           # Auth & security classes
│   ├── DashboardService.cs # Service configuration
│   ├── UserManager.cs      # User CRUD operations
│   └── EmailService.cs     # Email notifications
├── SimpleAI.cs             # Local AI engine (to enhance)
├── ContentFilter.cs        # Content filtering logic
├── Data/                   # JSON storage
│   ├── users.json         # User database
│   └── dashboard_settings.json
├── Guides/                 # NEW - Guide content (to create)
├── Archive/                # Archived OAuth/DeviceControl code
├── ROADMAP.md             # Updated project plan
├── STATUS.md              # This file
└── README.md              # Project overview
```

---

## 🔧 Tech Stack

**Backend:**
- .NET 8.0 (C# ASP.NET Core Razor Pages)
- JSON file storage (users, settings)
- Local AI (SimpleAI - keyword matching)

**Frontend:**
- Bootstrap 5 (responsive UI)
- Vanilla JavaScript (no frameworks)
- Chart.js (for future analytics)

**No External Dependencies:**
- ✅ No cloud APIs
- ✅ No database (JSON files)
- ✅ No OAuth providers
- ✅ Runs entirely offline

---

## 📋 Next Steps (In Order)

### Today/Tomorrow (Jan 5-6)
1. **Create `/Setup/Start.cshtml` page**
   - Device type buttons (iOS, Android, Windows)
   - Age input slider/dropdown
   - Basic form validation

2. **Create `SetupModel.cs` page model**
   - Handle form submission
   - Pass data to recommendation engine

3. **Enhance SimpleAI with templates**
   - Add age-based restriction templates
   - Create recommendation generation logic

### This Week (Jan 7-9)
4. **Create `/Setup/Recommendations.cshtml`**
   - Display AI-generated checklist
   - Show "why" explanations
   - Provide copy-paste lists

5. **Create first guide**
   - Take screenshots of iOS Screen Time setup
   - Write step-by-step instructions
   - Format as web page

### End of Week (Jan 10-12)
6. **Create `/Guides` section**
   - Guide listing page
   - Search/filter by device
   - Store guides as Markdown or HTML

7. **Test with real parents**
   - Get feedback on clarity
   - Iterate on recommendations
   - Improve guide visuals

---

## 🐛 Known Issues

**Minor:**
- Settings.cshtml.cs has null reference warning (line 62) - low priority
- Email service disabled (no SMTP configured) - not needed yet
- SMS service disabled - not needed for guide approach

**None blocking development**

---

## 💡 Key Decisions Made

**January 5, 2026:**
- ✅ Pivoted from API integration to educational guide approach
- ✅ Archived OAuth and DeviceControl code (may use later)
- ✅ Focusing on AI recommendations + step-by-step guides
- ✅ Parents manually apply settings (OS enforces)

**Why This is Better:**
- Faster to build (weeks vs months)
- Works immediately (no API dependencies)
- More valuable (solves awareness problem)
- Can't be bypassed (OS enforces natively)
- Universal (works for any device/OS version)

---

## 🎯 Success Criteria

**This Week:**
- [ ] Parent can select device + age
- [ ] AI generates personalized recommendations
- [ ] At least 1 complete guide created (iOS Screen Time)
- [ ] Setup takes < 5 minutes

**This Month:**
- [ ] 10+ guides created (iOS, Android, Windows)
- [ ] AI recommendations feel intelligent
- [ ] Real parents successfully set up controls
- [ ] Dashboard provides ongoing value

---

## 📞 How to Run

```powershell
# Start dashboard
dotnet run dashboard

# Access from:
# Local: http://localhost:5000
# Network: http://192.168.1.114:5000

# Login:
# Username: admin
# Password: PocketFence2026!
```

---

**Last Updated:** January 5, 2026, 10:30 PM  
**Next Update:** January 8, 2026 (mid-week check-in)
