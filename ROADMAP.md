# PocketFence-Family Roadmap

**Mission:** Help parents discover and use the parental controls already built into their kids' devices.

## 💡 The Real Problem

Parents struggle to protect their kids online, **not because tools don't exist**, but because:
- ❌ They don't know iOS Screen Time / Android Family Link / Windows Family Safety exist
- ❌ The settings are buried and confusing
- ❌ They don't know what's appropriate for their child's age
- ❌ Setup takes too long and feels overwhelming

## ✨ Our Solution

**PocketFence = Your AI Guide to Built-In Parental Controls**

1. **Parent tells us:** Child's age, device type
2. **AI recommends:** Exactly what to enable/block for that age
3. **We show:** Step-by-step visual guide to do it
4. **Result:** Parent sets up controls in 5 minutes, OS enforces perfectly

**Why This Works:**
- ✅ Solves real problem (awareness + guidance)
- ✅ Works immediately (no API integrations needed)
- ✅ Can't be bypassed (OS enforces natively)
- ✅ Fast to build (guides + AI recommendations)
- ✅ Universal (works for any device)

---

## ✅ Phase 0: DONE (Jan 3, 2026)

**What's Built:**
- Web dashboard with authentication
- Local AI engine (SimpleAI)
- Network accessible interface
- Basic UI framework

---

## 🔄 Phase 1: Smart Setup Guide (Jan 5-12, 2026)

**Goal:** Parents can get AI recommendations and step-by-step setup guides

### Week 1: Core Wizard

**Build:**
- [ ] Setup Wizard page (/Setup/Start)
  - Device type picker (iOS, Android, Windows)
  - Child age input
  - Child interests/concerns (optional)
  
- [ ] AI Recommendation Engine
  - Age-based restriction templates
  - Device-specific suggestions
  - Risk analysis (social media, messaging, web browsing)
  
- [ ] Recommendation Display
  - "For your 8-year-old with iPhone, here's what to do:"
  - Checklist format with explanations
  - Copy-paste ready lists (apps to block, etc.)

**Templates to Create:**
```
Age Groups:
- Toddler (0-5): Maximum protection, educational only
- Child (6-12): Balanced, homework-friendly
- Teen (13-17): Privacy-respecting, safety-focused

Per Device:
- iOS: Screen Time configuration guide
- Android: Family Link setup walkthrough  
- Windows: Family Safety instructions
```

---

## 🚀 Phase 2: Visual Guides (Jan 13-19, 2026)

**Goal:** Step-by-step guides with screenshots/videos

### Week 2: Guide Library

**Create:**
- [ ] Guide page (/Guides)
  - Searchable guide database
  - Filter by device/age/topic
  
- [ ] iOS Screen Time Guides
  - "How to Enable Screen Time"
  - "How to Block Apps by Category"
  - "How to Set Daily Time Limits"
  - "How to Restrict Web Content"
  - "How to Enable Communication Safety"
  
- [ ] Android Family Link Guides
  - "How to Set Up Family Link"
  - "How to Block Specific Apps"
  - "How to Set Bedtime Rules"
  - "How to Approve App Downloads"
  
- [ ] Windows Family Safety Guides
  - "How to Enable Family Safety"
  - "How to Block Websites"
  - "How to Set Screen Time Limits"
  - "How to View Activity Reports"

**Format:**
- Screenshots with annotations (red circles/arrows)
- Exact menu paths written out
- Video walkthroughs (screen recordings)
- PDF downloads for offline reference

---

## 📊 Phase 3: Activity Insights (Jan 20-26, 2026)

**Goal:** Help parents understand and improve current settings

### Week 3: Monitoring & Optimization

**Build:**
- [ ] Screenshot Parser
  - Upload iOS Screen Time weekly report
  - AI extracts usage data
  - Provides insights and suggestions
  
- [ ] Progress Tracker
  - Parent logs what they've enabled
  - Dashboard shows "Setup Completion: 80%"
  - Nudges for incomplete steps
  
- [ ] Knowledge Base
  - Common questions ("How do I...?")
  - Troubleshooting ("Child bypassed by...?")
  - Best practices from community

---

## 🎯 Phase 4: Community & Polish (Jan 27 - Feb 9, 2026)

**Goal:** Production-ready with community features

### Week 4-5: Enhancement

**Add:**
- [ ] Parent Community
  - Share templates ("This works for my 10-yr-old")
  - Success stories
  - Q&A forum
  
- [ ] Template Marketplace
  - Pre-built configurations
  - "Teen TikTok Safety Settings"
  - "Elementary School YouTube Setup"
  
- [ ] Mobile PWA Support
  - Make dashboard installable on phone
  - Offline access to guides
  - Better mobile UX

---

## 📱 Phase 5: Optional Companion Apps (Feb 10+)

**Only if needed - Most parents won't need this**

### Minimal Native Apps

**If parents demand easier application:**
- [ ] iOS App: "Apply These Settings" button
  - Uses FamilyControls framework
  - One-tap to apply dashboard recommendations
  
- [ ] Android App: Same concept
  - Uses Digital Wellbeing APIs
  - Quick setup from recommendations

**Note:** Start without apps, add only if user feedback demands it

---

## 📊 Current Status

**Active Phase:** Phase 1 - Smart Setup Guide  
**Current Date:** January 5, 2026  
**Completed:** Phase 0 (Dashboard foundation)  

### What We're Building Now:
1. Setup wizard (device picker, age input)
2. AI recommendation engine
3. Age-appropriate templates

### What We Learned:
- ✅ iOS Screen Time API requires native app (not REST API)
- ✅ Parents need guidance more than automation
- ✅ Built-in OS controls are powerful but unknown
- ✅ Educational approach is faster and more valuable

---

## 🎯 Success Metrics

**Phase 1 Success:**
- [ ] Parent completes setup in < 5 minutes
- [ ] Recommendations feel personalized and smart
- [ ] At least 3 device types supported

**Overall Success:**
- [ ] Parents say "I didn't know this existed!"
- [ ] 90% successfully enable controls
- [ ] Kids are safer online
- [ ] Parents check dashboard monthly for updates

---

## 🗑️ What We're NOT Building

❌ REST API integrations (don't exist for iOS)  
❌ OAuth flows (unnecessary for guides)  
❌ Device linking/pairing  
❌ Real-time device monitoring  
❌ Proxy servers or network filtering  
❌ Custom enforcement mechanisms  

**Reason:** OS already does enforcement perfectly. We just help parents use it.

---

**Last Updated:** January 5, 2026  
**Next Review:** January 12, 2026 (after Phase 1 complete)
