# 🛡️ PocketFence-Family

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**One dashboard to manage all your kids' device restrictions. Works with iOS Screen Time, Android Family Link, and Microsoft Family Safety.**

---

## 🎯 What It Does

**Simple Concept:**  
Your kids' devices already have parental controls built-in. PocketFence makes them easier to manage with AI-powered suggestions and a unified dashboard.

**How It Works:**
1. **Open PocketFence** on your PC or phone browser
2. **Sign in** with your Apple/Google/Microsoft account
3. **See all devices** from Family Sharing/Family Link in one place
4. **Set restrictions** → PocketFence pushes to OS → **Device enforces**
5. **Get reports** → Activity from all devices, all platforms

**Why This Works:**
- ✅ Uses built-in OS controls (Screen Time, Family Link, Family Safety)
- ✅ OS enforces rules → Can't be bypassed or uninstalled
- ✅ No network setup needed → No proxy, router config, or VPN
- ✅ Works everywhere → Home, school, friends' houses
- ✅ AI makes it smarter → Age-appropriate suggestions automatically

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0](https://dotnet.microsoft.com/download) or later
- Windows 10+, macOS 11+, or Linux

### Run the Dashboard
```powershell
# Start PocketFence
dotnet run dashboard
   Confidence: 0.45
   Recommendation: ALLOW

pocketfence> stats
📊 Filtering Statistics:
   Total Requests: 25
   Blocked: 5 (20.0%)
   Allowed: 20 (80.0%)
   AI Processed: 25
# Opens in browser automatically
# Visit: http://localhost:5000 or http://192.168.1.114:5000

# Default login:
# Username: admin
# Password: PocketFence2026!
```

---

## 📱 Platform Support

### iOS (Screen Time)
- Block websites by domain
- App limits by bundle ID
- Downtime scheduling
- Communication controls
- **Status:** OAuth integration in progress

### Android (Family Link)
- Block apps by package name
- Daily screen time limits
- Bedtime mode
- Location tracking
- **Status:** Coming Week 2

### Windows (Family Safety)
- Web content filtering
- App/game restrictions by rating
- Screen time limits
- Activity reports
- **Status:** Coming Week 3

---

## 🤖 AI Features

**SimpleAI Engine:**
- Analyzes threat levels locally (no cloud)
- Suggests age-appropriate restrictions
- Learns from parent preferences
- Generates smart block lists

**Age-Based Templates:**
- Toddler (0-5): Maximum protection
- Child (6-12): Balanced approach
- Teen (13-17): Privacy-respecting safety

---

## 📊 Current Status

**Phase 0:** ✅ Complete (Dashboard, auth, AI engine)  
**Phase 1:** 🔄 In Progress (iOS Screen Time integration)  
**Phase 2:** ⏳ Planned (Android + Windows)  
**Phase 3:** ⏳ Planned (AI enhancements)

See [ROADMAP.md](ROADMAP.md) for full timeline.

---

## 📚 Documentation

- [ROADMAP.md](ROADMAP.md) - Development plan
- [NETWORK_SETUP.md](NETWORK_SETUP.md) - Access from other devices
- [APPLE_OAUTH_SETUP.md](APPLE_OAUTH_SETUP.md) - iOS integration guide
- [DEVICE_API_RESEARCH.md](DEVICE_API_RESEARCH.md) - Platform API details
- [DASHBOARD_QUICKSTART.md](DASHBOARD_QUICKSTART.md) - Dashboard guide

---

## 🛡️ Privacy & Security

- All processing happens locally
- No data sent to external servers
- OS account credentials handled by OAuth (never stored)
- Activity logs stored locally only
- Open source - audit the code yourself

---

## 📄 License

MIT License - See [LICENSE](LICENSE) file

---

**PocketFence-Family** - Smart parental controls that work with what devices already have. 🛡️✨
