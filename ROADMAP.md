# PocketFence-Family Roadmap

**Simple Concept:** One dashboard to control your kids' built-in device restrictions (iOS Screen Time, Android Family Link, Windows Family Safety). Add AI smarts to what already works.

---

## 🎯 How It Works

**Parent's Device** (Your PC or Phone)
- Opens PocketFence dashboard in browser
- Signs in with Apple/Google/Microsoft account
- Sees all kids' devices from Family Sharing/Family Link
- Sets restrictions (PocketFence → OS API → Device)
- Gets activity reports from all devices

**Child's Device** (iPhone, Android, Windows)
- Already has Screen Time/Family Link/Family Safety built-in
- Parent links it through PocketFence
- **OS enforces restrictions natively** (can't be bypassed)
- Reports activity back through OS API
- Works at home, school, everywhere

**Why This Works:**
✅ Uses what devices already have (iOS Screen Time, Family Link, Family Safety)  
✅ OS enforces rules → Can't be uninstalled or bypassed  
✅ No network setup → No proxy, no router config, no VPN  
✅ Works everywhere → Home, school, friends' houses  
✅ Parents trust it → Apple/Google/Microsoft tools they already know  
✅ AI adds intelligence → Smart suggestions, simplified management  

---

## ✅ Phase 0: DONE (Jan 3, 2026)

**What's Built:**
- Web dashboard (runs on PC)
- Login system (Admin/Parent/Child roles)
- Network access (http://192.168.1.114:5000)
- Local AI (SimpleAI for threat detection)
- Email notifications

---

## 🔄 Phase 1: Connect to OS APIs (Jan 6-27)

**Week 1: iOS Screen Time**
- [🔄] Apple OAuth (started)
- [ ] List devices from Family Sharing
- [ ] Read Screen Time settings
- [ ] Update Screen Time restrictions
- [ ] Show iOS devices in dashboard

**Week 2: Android Family Link**
- [ ] Google OAuth
- [ ] List devices from Family Link
- [ ] Read Digital Wellbeing settings
- [ ] Update app limits, bedtime
- [ ] Show Android devices in dashboard

**Week 3: Microsoft Family Safety**
- [ ] Microsoft OAuth
- [ ] List devices from Family Safety
- [ ] Read web filtering settings
- [ ] Update screen time limits
- [ ] Show Windows/Xbox devices

**Goal:** Manage all platforms from one dashboard

---

## 🚀 Phase 2: AI Enhancements (Jan 27 - Feb 16)

**Smart Features:**
- [ ] Age-based templates (toddler, child, teen)
- [ ] AI suggests optimal settings
- [ ] Activity reports with charts
- [ ] Weekly email summaries
- [ ] "Extend bedtime 30 min" quick actions

**Goal:** Make parental controls smarter and easier

---

## 🎯 Phase 3: Polish & Deploy (Feb 17 - Mar 16)

**Production Ready:**
- [ ] Multi-family support
- [ ] Mobile-friendly UI
- [ ] Auto-updates
- [ ] Complete documentation
- [ ] 30-day stable operation

**Goal:** Ready for real families to use

---

## 📊 Current Status

**Active:** Phase 1, Week 1 (iOS Screen Time)  
**Blocker:** Need to research Apple Screen Time API availability  
**Next:** Determine if REST API exists or if we need iOS companion app  

---

## 🔍 What We Won't Build

❌ Custom proxy server  
❌ Network traffic monitoring  
❌ Router-level filtering  
❌ VPN or DNS override  
❌ Custom device agents  

**Reason:** OS already does this better. We just make it easier to manage.

---

## 📝 Tech Stack

**Dashboard:**
- .NET 8.0 (ASP.NET Core Razor Pages)
- Bootstrap 5 UI
- Chart.js for graphs
- Runs on Windows/Mac/Linux

**OS Integration:**
- Apple Screen Time API (OAuth)
- Google Family Link API (OAuth)
- Microsoft Family Safety API (OAuth)
- No device-side code needed

**AI:**
- SimpleAI (local, keyword-based)
- Generates age-appropriate rules
- Analyzes activity patterns
- No external API calls

---

**Last Updated:** January 5, 2026  
**Next Review:** After iOS API research complete
