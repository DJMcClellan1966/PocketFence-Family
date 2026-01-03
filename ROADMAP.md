# PocketFence-Family Development Roadmap

**Project Vision:** AI-powered parental control hub that leverages native OS restrictions (iOS Screen Time, Google Family Link, Microsoft Family Safety) with centralized parent monitoring, optimized for local AI inference.

**Architecture Pivot:** Changed from proxy server approach to native OS API integration for simpler, more reliable device control.

**Current Version:** v0.1.0 (Phase 0 Complete)  
**Current Phase:** Phase 1 - OS API Integration (Starting)  
**Last Updated:** January 3, 2026

---

## ✅ PHASE 0: FOUNDATION - **COMPLETED** (January 3, 2026)

### Core Infrastructure
- [x] .NET 8.0 ASP.NET Core Razor Pages application
- [x] Cross-platform support (Windows, Mac, Linux)
- [x] JSON-based data storage (no database required)
- [x] Session-based authentication system
- [x] Network accessibility (0.0.0.0:5000 - all interfaces)
- [x] Shows both local and network URLs on startup

### Authentication & Security
- [x] User registration with email verification
- [x] Gmail SMTP integration (your-email@gmail.com)
- [x] Password reset (forgot password) flow
- [x] Password reuse prevention
- [x] Rate limiting (5 attempts, 15-min lockout)
- [x] Security audit logging (OptimizedSecurityAuditLogger)
- [x] 30-minute session timeout with warnings
- [x] Security headers (CSP, XSS protection, frame-options, etc.)
- [x] Email verification required for login

### User Management
- [x] Admin account (username: admin, password: PocketFence2026!)
- [x] Parent role (full dashboard access)
- [x] Child role (limited dashboard with simplified view)
- [x] Role-based access control (AuthenticatedPageModel)
- [x] Account management page (email/password updates)
- [x] Role selection during registration

### AI & Content Filtering (Local)
- [x] SimpleAI - Lightweight threat detection
- [x] Keyword-based content analysis (no external APIs)
- [x] Pattern matching for safe/threat keywords
- [x] Local inference only (privacy-first)
- [x] ContentFilter with domain blocklist
- [x] BlockedContentStore for activity tracking
- [x] Real-time filter service (RealTimeFilterService)

### Dashboard Features
- [x] Parent dashboard with statistics (today, week, month, all-time)
- [x] Blocked content display with categories
- [x] Recent activity timeline
- [x] Child dashboard (simplified, age-appropriate)
- [x] Educational safety tips for children
- [x] Settings page (filters, thresholds)
- [x] Responsive Bootstrap 5 UI
- [x] Session monitoring and auto-logout

### SMS/Notifications Infrastructure
- [x] EmailService with configurable SMTP
- [x] Configurable base URL for email links (env var support)
- [x] SMS provider architecture (ISmsProvider interface)
- [x] ConsoleSmsProvider (development mode)
- [x] EmailToSmsProvider (carrier gateways)
- [x] WebhookSmsProvider (for Gammu/Android gateway)
- [x] TwilioSmsProvider (production SMS)
- [x] AiSmsService with AI features:
  - Message optimization (160 char limit)
  - Smart routing with retry logic
  - Rate limiting (10 msgs/min per number)
  - Multiple provider support
- [x] Phone verification system (infrastructure complete, UI hidden)

### Documentation
- [x] README.md with project overview
- [x] copilot-instructions.md for development
- [x] NETWORK_SETUP.md (how to access from other devices)
- [x] SELF_HOSTED_SMS_GUIDE.md (Gammu, Android gateway)
- [x] AI_SMS_SERVICE_GUIDE.md (SMS provider setup)

### Git Commit
- [x] **Committed:** January 3, 2026 - "Phase 0 Complete"

**Current State:** Dashboard operational at http://192.168.1.114:5000, authentication working, role system enforced, but **NOT actively filtering traffic yet** - monitoring interface ready, needs proxy server.

---

## 🎯 PHASE 1: OS API INTEGRATION - **STARTING NOW**

**Goal:** Control child devices via native OS parental control APIs instead of proxy  
**Timeline:** January 6-27, 2026 (3 weeks)  
**Priority:** **CRITICAL** - Simpler, more reliable than proxy approach

**Architecture Decision:** Leverage iOS Screen Time, Google Family Link, and Microsoft Family Safety APIs. Our AI decides restrictions, OS enforces them.

### Why This Approach is Better
- ✅ **OS enforces** - child can't bypass without parent password
- ✅ **System-level control** - blocks apps, games, browsers, everything
- ✅ **No proxy setup** - just link devices via existing OS features
- ✅ **Works offline** - rules cached on device by OS
- ✅ **More reliable** - Apple/Google/Microsoft maintain the APIs
- ✅ **Simpler code** - REST API calls vs network proxying

### Week 1: Foundation & iOS Screen Time API

**Status:** ⬜ Not Started

#### Day 1-2: Research & Architecture
- [ ] Research iOS Screen Time API (Apple Family Sharing)
- [ ] Research Google Family Link API
- [ ] Research Microsoft Family Safety API
- [ ] Design unified abstraction layer (IDeviceControlProvider)
- [ ] Create ChildDevice model with OS-specific fields
- [ ] Plan authentication flows for each platform

#### Day 3-5: iOS Screen Time Integration
- [ ] Create iOSScreenTimeProvider.cs
- [ ] Implement Apple ID authentication (OAuth)
- [ ] Device linking flow (parent authorizes via Apple Family)
- [ ] API client for Screen Time restrictions:
  - Website blocking
  - App limits by category
  - Downtime schedules
  - Communication limits
- [ ] Test with real iPhone/iPad
- [ ] Error handling (auth failures, network issues)

**iOS API Capabilities:**
- Block websites by domain or category
- Restrict apps by age rating or specific apps
- Set downtime (device unusable during hours)
- Limit screen time per day/app
- Always allow certain apps (educational)
- Communication limits (who can contact)

#### Day 6-7: Dashboard Integration
- [ ] /Devices/LinkiOS page (authorization flow)
- [ ] Display linked iOS devices in dashboard
- [ ] Show current restrictions per device
- [ ] Update restriction settings UI
- [ ] Push changes to iOS devices via API
- [ ] Pull activity reports from Screen Time API
- [ ] Display in parent dashboard

### Week 2: Android & Windows Integration

**Status:** ⬜ Not Started

#### Day 8-10: Google Family Link Integration
- [ ] Create AndroidFamilyLinkProvider.cs
- [ ] Implement Google OAuth authentication
- [ ] Device linking via Family Link
- [ ] API client for Digital Wellbeing:
  - App blocking by package name
  - Screen time limits
  - Bedtime schedules
  - Location tracking
  - Content ratings
- [ ] Test with Android phone/tablet
- [ ] Dashboard pages for Android devices

**Android API Capabilities:**
- Block specific apps by package name
- Set daily screen time limits
- Bedtime mode (device locks)
- Location tracking and history
- App activity monitoring
- Content filtering by rating
- YouTube supervised experience

#### Day 11-14: Microsoft Family Safety Integration
- [ ] Create WindowsFamilySafetyProvider.cs
- [ ] Implement Microsoft Account OAuth
- [ ] Device linking via Family Safety
- [ ] API client for Windows restrictions:
  - Web filtering
  - App/game blocking by rating
  - Screen time limits
  - Activity reports
- [ ] Test with Windows PC
- [ ] Dashboard pages for Windows devices

**Windows API Capabilities:**
- Web content filtering (Safe Search, blocked sites)
- App and game restrictions by rating (ESRB, PEGI)
- Screen time limits per day
- Activity reports (apps used, sites visited)
- Location tracking
- Purchase approvals for games/apps

### Week 3: AI Rule Generation & Unified Dashboard

**Status:** ⬜ Not Started

#### Day 15-17: AI-Driven Rule Sets
- [ ] Age-based restriction templates:
  - Toddler (0-5): Very restrictive
  - Child (6-12): Educational focus
  - Teen (13-17): Balanced restrictions
- [ ] SimpleAI analyzes age and generates rules
- [ ] Per-device customization (override AI defaults)
- [ ] Rule preview before applying
- [ ] Bulk rule updates across devices
- [ ] Schedule-ENHANCED FEATURES & POLISH (Week 4-6)

**Goal:** Advanced monitoring, educational content, and user experience polish  
**Timeline:** January 27 - February 16, 2026  
**Status:** ⬜ Not Started

### Week 4: Advanced Monitoring & Reports

#### Enhanced Dashboard
- [ ] Activity timeline with filtering (date, device, child)
- [ ] Charts with Chart.js:
  - Screen time trends over weeks
  - Most used apps by device
  - Blocked content by category
  - Time-of-day usage heatmap
- [ ] Weekly parent reports via email
- [ ] Export functionality (CSV, PDF)
- [ ] Device comparison view
- [ ] Real-time notifications (child requests override)

#### Smart Alerts
- [ ] Email alerts for concerning activity
- [ ] SMS alerts (optional, via Twilio)
- [ ] Parent mobile app notifications (future)
- [ ] Configurable alert thresholds
- [ ] Daily digest email option

### Week 5: Educational Content Portal

#### Safe Learning Hub
- [ ] Age-appropriate website directory (curated)
- [ ] Educational games and apps database
- [ ] STEM learning resources by grade level
- [ ] Coding tutorials (Khan Academy, Code.org)
- [ ] Safe video content (YouTube Kids curated)
- [ ] Online library resources
- [ ] Homework help sites whitelist

#### Content Integration
- [ ] Auto-whitelist educational content
- [ ] "Always Allow" for specific learning apps
- [ ] Educational app recommendations by age
- [ ] Integration with Khan Academy API
- [ ] YouTube Kids safe channel lists
- [ ] Safe search enforcement helpers

### Week 6: User Experience & Polish

#### Parent UX Improvements
- [ ] Onboarding wizard for first-time setup
- [ ] Tooltips and help text throughout
- [ ] Quick actions menu (common tasks)
- [ ] Mobile-responsive improvements
- [ ] Dark mode option
- [ ] Keyboard shortcuts
- [ ] Accessibility improvements (ARIA labels)

#### Device Management Features
- [ ] Device nicknames (rename devices)
- [ ] Device groups (all kids, just teens, etc.)
- [ ] Bulk rule changes across groups
- [ ] Template rule sets (save/load)
- [ ] Rule history (see past changes)
- [ ] Rollback changes feature

#### Testing & Documentation
- [ ] User testing with real families
- [ ] Fix bugs from testing feedback
- [ ] Complete setup guides per platform
- [ ] Video tutorials (screen recordings)
- [ ] FAQ document
- [ ] Troubleshooting guide
- [ ] Privacy policy and terms

**Deliverables:**
- Advanced analytics dashboard
- Educational content hub
- Polished UX with onboarding
- Complete documentation
- Ready for family beta testingtp://192.168.1.114:5000/pair?token={guid}`
- [ ] Display QR on /Devices/Add page
- [ ] Child scans → redirects to device setup instructions
- [ ] Device pairing endpoint validates token

#### Age-Based Content Rules
- [ ] Age brackets: Toddler (0-5), Child (6-12), Teen (13-17)
- [ ] Age-appropriate blocklists
- [ ] Different AI threat thresholds per age
- [ ] Educational content filtered by age
- [ ] Parent can override rules per child

### Week 3: Enhanced Monitoring & Controls

#### Dashboard Improvements
- [ ] Per-device activity view
- [ ] Charts with Chart.js (blocked over time)
- [ ] Activity timeline (who, what, when)
- [ ] Category breakdown by device
- [ ] Export reports (CSV download)
- [ ] Search/filter activity logs

#### Time-Based Restrictions
- [ ] Bedtime enforcement (9pm-7am → block all)
- [ ] School hours (8am-3pm → education only)
- [ ] Weekend vs weekday rule sets
- [ ] Screen time tracking per device
- [ ] Daily/weekly limits with warnings

### Week 4: Educational Content Portal

#### Safe Browsing Features
- [ ] Age-appropriate website directory
- [ ] Educational games database
- [ ] STEM learning resources
- [ ] Safe search enforcement (Google SafeSearch API)
- [ ] YouTube Restricted Mode enforcement
- [ ] Curated learning links by subject

#### Parent Control Panel
- [ ] Whitelist management (always allow sites)
- [ ] Blacklist management (always block sites)
- [ ] Temporary bypass codes (1-hour access)
- [ ] Emergency override (parent password)
- [ ] Weekly email reports (activity summary)
- [ ] Mobile notifications via email
Weeks 7-10)

**Goal:** Robust system for 24/7 household use with multiple families  
**Timeline:** February 17 - March 16, 2026  
**Status:** ⬜ Not Started

### Multi-Family Support
- [ ] Multiple parent accounts
- [ ] Shared parenting (mom + dad co-manage)
- [ ] Permission levels (admin parent, view-only parent)
- [ ] Grandparent access (read-only)
- [ ] Caregiver temporary access codes

### System Reliability
- [ ] Auto-restart on crash (Windows Service)
- [ ] Health monitoring endpoint (/health)
- [ ] Performance metrics logging
- [ ] Memory leak detection
- [ ] API rate limit handling (OS APIs)
- [ ] Retry logic for failed API calls
- [ ] Graceful degradation if APIs down

### Data Management
- [ ] Database backup/restore (JSON → SQLite migration)
- [ ] Configuration export/import
- [ ] Archive old activity logs (> 90 days)
- [ ] Data retention policies
- [ ] GDPR compliance (data export, deletion)
- [ ] Secure credential storage (encrypted)

### Deployment & Updates
- [ ] Windows Service installation script
- [ ] Startup on boot configuration
- [ ] Logging to files (rolling logs, max 100MB)
- [ ] Auto-update mechanism (check GitHub releases)
- [ ] Uninstaller script
- [ ] Configuration wizard (first-run)

### Performance Optimization
- [ ] API call caching (reduce OS API calls)
- [ ] Database query optimization
- [ ] Lazy loading for dashboard
- [ ] Pagination for large activity logs
- [ ] Background job queue (push updates)
- [ ] Handle 20+ devices without slowdown

**Deliverables:**
- Windows Service package
- Auto-update system
- 30+ days stable operation
- Multi-family ready3+) - **TBD**

**Goal:** Share with other families or publish commercially  
**Timeline:** March 2026 onwards  
**Decision:** Will be made after Phase 3 beta testing  
**Status:** ⬜ Not Started

### Distribution Options Under Consideration

#### Option A: Open Source GitHub Project
**Pros:** Free, community-driven, maximum reach  
**Effort:** 2-3 weeks polish

- [ ] Complete documentation (README, guides, FAQs)
- [ ] One-click installer (PowerShell script)
- [ ] Docker container option
- [ ] Setup wizard (first-run experience)
- [ ] Video tutorials (YouTube)
- [ ] GitHub releases with changelogs
- [ ] Community support (Discord or Discussions)
- [ ] MIT or GPL license

#### Option B: Hosted SaaS Service
**Pros:** Easiest for users, recurring revenue  
**Effort:** 2-3 months (cloud infrastructure)

- [ ] Convert to cloud-hosted service
- [ ] Multi-tenant database architecture
- [ ] User account system (signup/billing)
- [ ] Subscription tiers ($5-15/month)
- [ ] Stripe payment integration
- [ ] Azure/AWS deployment
- [ ] Customer support system
- [ ] Privacy policy / GDPR compliance
ROJECT STATUS TRACKER

**Current Phase:** Phase 1 - OS API Integration (Week 1, Day 1)  
**Next Milestone:** iOS Screen Time API working (January 13, 2026)  
**Team Size:** 1 (solo developer)  
**Total Dev Time:** ~3 weeks  
**Architecture:** Native OS API approach (no proxy)

### ✅ Completed Milestones

**Phase 0: Foundation (January 3, 2026)** ✅ COMPLETE
- ✅ Authentication system (email verification, password reset)
- ✅ Role-based access control (Parent/Child/Admin)
- ✅ Dashboard UI (parent monitoring + child simplified view)
- ✅ Local AI filtering engine (SimpleAI - keyword matching)
- ✅ Network accessibility (0.0.0.0:5000)
- ✅ Email/SMS infrastructure (Gmail SMTP, multiple SMS providers)
- ✅ User management (create, update, delete accounts)
- ✅ Security features (rate limiting, audit logging, session timeout)
- ✅ Git repository with commits

### 🔄 Current Sprint (Week 1: Jan 6-12, 2026)

**Focus:** iOS Screen Time API Integration

**Today (Day 1 - January 6):**
- ⬜ Research Apple Screen Time API documentation
- ⬜ Research authentication requirements (OAuth, Family Sharing)
- ⬜ Design IDeviceControlProvider interface
- ⬜ Create ChildDevice model
- ⬜ Plan folder structure for device providers

**This Week:**
- ⬜ Day 1-2: Research & architecture design
- ⬜ Day 3-5: Build iOS Screen Time provider
- ⬜ Day 6-7: Dashboard integration for iOS devices
JSON file storage (migrate to SQLite in Phase 3)
- REST API clients for OS platforms

**Frontend:**
- Bootstrap 5
- JavaScript (vanilla)
- Chart.js (analytics graphs)
- QRCoder (device linking - maybe not needed now)

**OS API Integrations:**
- iOS: Screen Time API via Apple Family Sharing
- Android: Google Family Link / Digital Wellbeing API
- Windows: Microsoft Family Safety API
- OAuth 2.0 for authentication

**AI/Filtering:**
- SimpleAI (keyword + pattern matching)
- Local inference only
- No ML models (lightweight)
- Generates age-appropriate rule sets

**Communications:**
- Gmail SMTP (parent notifications)
- Twilio SMS (optional alerts)

**NuGet Packages:**
- Microsoft.AspNetCore.App
- System.Text.Json
- QRCoder (if needed for linking)
- Chart.js (CDN)
- Bootstrap 5 (CDN)

**Future Considerations:**
- SQLite for better performance (Phase 3)
- SignalR for real-time dashboard updates
- Background job queue (Hangfire or custom

### 🎯 Phase Completion Status

| Phase | Status | Start Date | End Date | Progress |
|-------|--------|------------|----------|----------|
| Phase 0: Foundation | ✅ Complete | Dec 2025 | Jan 3, 2026 | 100% |
| Phase 1: OS APIs | 🔄 In Progress | Jan 6, 2026 | Jan 27, 2026 | 0% |
| Phase 2: Features | ⏳ Planned | Jan 27, 2026 | Feb 16, 2026 | 0% |
| Phase 3: Production | ⏳ Planned | Feb 17, 2026 | Mar 16, 2026 | 0% |
| Phase 4: Distribution | ⏳ TBD | Mar 2026+ | TBD | 0% |

### 📈 Weekly Progress Tracking

**Week of January 6-12, 2026:**
- [⬜] Day 1: API research complete
- [⬜] Day 2: Architecture designed
- [⬜] Day 3: iOS provider started
- [⬜] Day 4: OAuth authentication working
- [⬜] Day 5: First restriction successfully pushed
- [⬜] Day 6: Dashboard shows linked iOS device
- [⬜] Day 7: Week 1 demo ready

**Update this section daily with completed tasks.**

### 🚧 Known Issues & Blockers

**Current Issues:**
- None (fresh start on Phase 1)

**Technical Debt from Phase 0:**
- Settings.cshtml.cs null reference warning (low priority)
- Phone verification code still in repo but commented out
- JSON storage may need SQLite migration eventually

**Risks:**
- iOS/Android/Windows API availability (need to verify access)
- OAuth complexity for each platform
- API rate limits from OS providers
- Testing requires real devices (iOS, Android, Windows)

### 🎯 Success Metrics

**Phase 1 Success Criteria:**
- [ ] Successfully link at least one device per platform (iOS, Android, Windows)
- [ ] Push restrictions from dashboard to device
- [ ] Retrieve activity reports from OS APIs
- [ ] Dashboard displays all devices in one view
- [ ] Setup time < 5 minutes per device

**Overall Project Success:**
- [ ] Works with own family (3+ children, 5+ devices)
- [ ] 30+ days continuous operation
- [ ] Positive feedback from beta users
- [ ] Less than 1 hour/week maintenance
- [ ] Stable enough for public releas
**Current Lean:** Option A (Open Source) for maximum reach, then add Option C (Store) for easy installs
- Convert dashboard to PWA
- Cloud parent account
- Child PWA installs on any device
- Works offline with ServiceWorker
- Subscription model ($5-10/month)
- Multi-tenant architecture

### Option C: Native Mobile/Desktop Apps
- .NET MAUI parent app (Windows/Mac)
- .NET MAUI child app (Android)
- iOS child app (Swift)
- System-level device controls
- App Store submission

**Note:** PWA approach saved for later - see notes in conversation history.

---

## 📊 Project Status

**Current Phase:** Phase 1 - Active Filtering  
**Next Milestone:** Working proxy server (January 12, 2026)  
**Team Size:** 1 (solo developer)  
**Development Time:** ~3 weeks so far

### Completed (Phase 0)
✅ Authentication system  
✅ Role-based access control  
✅ Dashboard UI (parent + child)  
✅ Local AI filtering engine  
✅ Network accessibility  
✅ Email/SMS infrastructure  

### In Progress (Phase 1)
🔄 HTTP/HTTPS proxy server  
⬜ Content filtering in proxy pipeline  
⬜ Certificate management for HTTPS  

### Upcoming (Phase 2)
⏳ Child device profiles  
⏳ QR code pairing  
⏳ Age-based rules  
⏳ Educational content  

### Blocked/Waiting
None

---

## 📦 Technical Stack

**Backend:**
- .NET 8.0 (C#)
- ASP.NET Core Razor Pages
- HTTP Proxy (Titanium.Web.Proxy or custom)
- JSON file storage

**Frontend:**
- Bootstrap 5
- JavaScript (vanilla)
- Chart.js (for graphs)
- QRCoder (for QR generation)

**AI/Filtering:**
- SimpleAI (keyword + pattern matching)
- Local inference only
- No ML models (lightweight)

**Optional:**
- Gmail SMTP (emails)
- Twilio (SMS)

---

## 🎯 Success Metrics

### Phase 1 Success
- [ ] Proxy filters 90%+ of test threats
- [ ] Dashboard shows real-time activity
- [ ] < 50ms latency overhead
- [ ] Child device browses safely through proxy
- [ ] Setup takes < 10 minutes

### Phase 2 Success
- [ ] Manage 3+ child devices easily
- [ ] QR code setup < 2 minutes
- [ ] Age rules block inappropriate content
- [ ] Parents check dashboard weekly

### Phase 3 Success
- [ ] System runs 30+ days without restart
- [ ] Handles 10,000+ requests/day
- [ ] 99.9% uptime
- [ ] Family uses daily for 3+ months

---

## 🐛 Known Issues & Technical Debt

### Issues
- [ ] Settings.cshtml.cs has null reference warning (line 62)
- [ ] Phone verification UI commented out but code remains
- [ ] JSON file may slow with 10,000+ entries (consider SQLite)
- [ ] Email service lacks retry logic
- [ ] No database indexes

### Future Improvements
- [ ] Add caching for frequently checked domains
- [ ] Optimize BlockedContentStore queries
- [ ] Lazy load dashboard statistics
- [ ] SQLite migration for better performance
- [ ] Add CSRF token validation everywhere
- [ ] Implement 2FA for parent accounts
- [ ] Encrypted storage for sensitive settings

---

## 📚 Dependencies

**Runtime Requirements:**
- .NET 8.0 Runtime
- Windows 10+ / macOS 11+ / Linux (modern)
- 512MB RAM minimum
- 100MB disk space
- Network access

**Development Requirements:**
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git
- PowerShell (Windows) or Bash (Mac/Linux)

**Optional:**
- Gmail account (for emails)
- Twilio account (for SMS)

---

## 🔗 Related Documents

- [README.md](README.md) - Project overview
- [NETWORK_SETUP.md](NETWORK_SETUP.md) - Network access guide
- [SELF_HOSTED_SMS_GUIDE.md](SELF_HOSTED_SMS_GUIDE.md) - SMS setup
- [AI_SMS_SERVICE_GUIDE.md](AI_SMS_SERVICE_GUIDE.md) - SMS providers
- [copilot-instructions.md](.github/copilot-instructions.md) - Dev guidelines

---

## 📝 Version History

### v0.1.0 - Foundation (January 3, 2026)
- Authentication with email verification
- Parent/Child role system
- Dashboard UI (monitoring interface)
- Network accessibility (0.0.0.0:5000)
- Local AI filtering engine (SimpleAI)
- Email service with Gmail SMTP
- SMS service architecture
- Security features (rate limiting, audit logs)

### v0.2.0 - Active Filtering (Target: January 12, 2026)
- HTTP/HTTPS proxy server
- Real-time content filtering
- SSL certificate support
- Blocked page UI
- Live dashboard monitoring

### v1.0.0 - Production Ready (Target: March 2026)
- Full device management
- QR code pairing
- Educational content portal
- Multi-device support
- 30+ days stable operation

---

**Last Updated:** January 3, 2026  
**Next Review:** January 12, 2026 (after Phase 1 complete)
