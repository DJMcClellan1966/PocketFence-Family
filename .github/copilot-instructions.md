# PocketFence-Family

**One dashboard to manage kids' device restrictions across iOS, Android, and Windows.**

## Core Concept

**Simple:**  
Devices already have parental controls (iOS Screen Time, Android Family Link, Windows Family Safety). PocketFence makes them smarter and easier to manage from one dashboard.

**How It Works:**
1. Parent opens PocketFence dashboard (web browser)
2. Signs in with Apple/Google/Microsoft (OAuth)
3. Sees all kids' devices from Family Sharing/Family Link
4. Sets restrictions → PocketFence → OS API → Device enforces
5. Gets activity reports from all devices in one place

**Why This Approach:**
- ✅ OS enforces natively (can't be bypassed)
- ✅ No network setup (no proxy/router/VPN)
- ✅ Works everywhere (home, school, anywhere)
- ✅ Parents already trust these tools
- ✅ AI adds intelligence on top

## Development Guidelines

**Tech Stack:**
- .NET 8.0 (ASP.NET Core Razor Pages)
- Bootstrap 5 UI
- OAuth 2.0 (Apple, Google, Microsoft)
- SimpleAI (local threat detection)
- JSON storage (migrate to SQLite later)

**Key Principles:**
- Keep it simple - leverage what devices already have
- Local AI only - no external API dependencies
- Privacy first - all processing local
- Mobile-friendly UI - parent uses phone
- Fast & lightweight - runs on any PC

## Current Focus

**Phase 1:** Connecting to OS APIs
- iOS Screen Time (in progress)
- Android Family Link (week 2)  
- Microsoft Family Safety (week 3)

**Do NOT Build:**
- Proxy servers
- Network traffic monitors
- Custom device agents
- Router-level filtering

**Reason:** OS already does this better.