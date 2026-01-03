# PocketFence-Family Development Roadmap

**Project Vision:** Local content filtering system for families with parent monitoring dashboard and child protection features, optimized for local AI inference without external dependencies.

**Current Version:** v0.1.0 (Phase 0 Complete)  
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

## 🎯 PHASE 1: ACTIVE FILTERING - **STARTING NOW**

**Goal:** Transform from monitoring dashboard to active content filter  
**Timeline:** January 6-12, 2026 (Week 1)  
**Priority:** **CRITICAL** - This makes the app actually work

### Implementation Plan

#### Day 1-2: HTTP Proxy Server Foundation
- [ ] Create ProxyServer.cs class
- [ ] Add HTTP proxy listener (port 8080)
- [ ] Request/response interception pipeline
- [ ] Basic pass-through (no filtering yet)
- [ ] Request logging to console
- [ ] Test with HTTP sites (curl, browser proxy settings)

**Technical Details:**
- Use HttpListener or Titanium.Web.Proxy
- Parse HTTP requests (method, URL, headers, body)
- Forward to destination server
- Capture response
- Log to BlockedContentStore

#### Day 3-4: Content Filtering Integration
- [ ] Integrate SimpleAI into proxy request handler
- [ ] Check URL against ContentFilter blocklist
- [ ] Analyze response content for threats
- [ ] Return blocked page HTML for bad requests
- [ ] Dashboard shows live filtering activity
- [ ] Per-user filtering (track by session/device)

**Filtering Logic:**
```
Request → Extract URL
       ↓
    ContentFilter.IsBlocked(url)?
       ↓ YES
    Return BlockPage.html
       ↓ NO
    Forward to destination
       ↓
    Response ← Get content
       ↓
    SimpleAI.AnalyzeContent()?
       ↓ THREAT
    Return BlockPage.html
       ↓ SAFE
    Return response to client
```

#### Day 5: HTTPS Support (SSL/TLS Interception)
- [ ] Generate self-signed root certificate
- [ ] Store certificate in Data folder
- [ ] Certificate installation script/guide for:
  - Windows (certmgr.msc)
  - macOS (Keychain Access)
  - iOS (Settings → Profile)
  - Android (Settings → Security)
- [ ] SSL/TLS termination in proxy
- [ ] Re-encrypt connection to destination
- [ ] Handle certificate validation
- [ ] Test HTTPS sites (facebook.com, youtube.com, etc.)

**Cert Setup:**
```powershell
# Generate root CA
New-SelfSignedCertificate -Type Custom -Subject "CN=PocketFence Root CA" ...

# Export for import on child devices
Export-Certificate -Cert $cert -FilePath PocketFenceCA.cer
```

#### Day 6-7: Testing, Polish & Documentation
- [ ] Test with child device (phone, tablet)
- [ ] Verify blocked content logs to dashboard
- [ ] Performance testing (latency impact)
- [ ] Memory usage optimization
- [ ] Error handling (connection failures, timeouts)
- [ ] Blocked page design (user-friendly message)
- [ ] Proxy setup guide:
  - Windows proxy settings
  - iOS WiFi proxy config
  - Android WiFi proxy config
  - Browser-specific proxy (Firefox, Chrome)
- [ ] Troubleshooting common issues
- [ ] Update README with proxy instructions

**Setup Example:**
```
Windows: Settings → Network → Proxy → Manual
  Address: 192.168.1.114
  Port: 8080

iOS: Settings → WiFi → (i) → HTTP Proxy → Manual
  Server: 192.168.1.114
  Port: 8080
```

### Success Criteria
- ✅ Child device routes traffic through proxy
- ✅ Blocked domains return block page
- ✅ Threat content triggers AI blocking
- ✅ Dashboard shows real-time filtering
- ✅ HTTPS sites work with installed cert
- ✅ < 50ms latency overhead
- ✅ Handles 100+ req/min without issues

### Deliverables
1. ProxyServer.cs - Core proxy implementation
2. BlockPage.cshtml - User-facing block page
3. Certificate generation script
4. PROXY_SETUP.md - Setup guide
5. Updated dashboard showing live proxy activity

---

## 🚀 PHASE 2: DEVICE MANAGEMENT (Weeks 2-4)

**Goal:** Make it easy for parents to manage multiple children and devices  
**Timeline:** January 13 - February 2, 2026

### Week 2: Child Profiles & QR Pairing

#### Child Device Model
- [ ] ChildDevice.cs (Id, Name, Age, DeviceType, PairingToken, CreatedAt)
- [ ] Device storage in JSON (devices.json)
- [ ] DeviceManager.cs (CRUD operations)
- [ ] Parent page: /Devices (list all devices)
- [ ] Parent page: /Devices/Add (create new device profile)

#### QR Code Generation
- [ ] Add QRCoder NuGet package
- [ ] Generate pairing token (GUID)
- [ ] QR code contains: `http://192.168.1.114:5000/pair?token={guid}`
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

**Deliverables:**
- Full device management system
- QR code pairing working
- Age-based rules active
- Educational content available
- Enhanced parent dashboard

---

## 📈 PHASE 3: PRODUCTION READY (Months 2-3)

**Goal:** Robust system for 24/7 household use  
**Timeline:** February-March 2026

### Multi-Device Support
- [ ] Handle 10+ child devices simultaneously
- [ ] Per-device bandwidth management
- [ ] Performance optimization for multiple streams
- [ ] Connection pooling
- [ ] Request queuing and prioritization

### Offline & Synchronization
- [ ] Offline activity logging (child device caches)
- [ ] Sync queue when connection restored
- [ ] Local rule caching on child devices
- [ ] Graceful degradation when parent offline
- [ ] Conflict resolution for setting changes

### Advanced Features
- [ ] Multi-parent accounts (mom + dad both manage)
- [ ] YouTube video title/channel filtering
- [ ] Social media monitoring (TikTok, Instagram)
- [ ] Image content analysis (basic NSFW detection)
- [ ] App usage tracking (if visible in proxy)
- [ ] Device location tracking (optional)

### System Reliability
- [ ] Auto-restart on crash (Windows Service)
- [ ] Database backup/restore
- [ ] Configuration export/import
- [ ] Health monitoring endpoint
- [ ] Performance metrics logging
- [ ] Memory leak detection and prevention

### Production Deployment
- [ ] Windows Service installation
- [ ] Startup on boot
- [ ] Logging to files (rolling logs)
- [ ] Update mechanism (auto-update)
- [ ] Uninstaller script

**Deliverables:**
- Rock-solid system running 30+ days
- Handles 10,000+ requests/day
- 99.9% uptime
- Family uses daily with confidence

---

## 🌟 PHASE 4: DISTRIBUTION (Month 4+) - **TBD**

**Goal:** Share with other families or publish commercially  
**Timeline:** April 2026 onwards  
**Decision:** Will be made after Phase 3 testing

### Option A: Open Source Project
- Polish documentation
- One-click installer
- Docker container
- Setup wizard
- Video tutorials
- GitHub releases
- Community support forum

### Option B: PWA Hosted Service (Progressive Web App)
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
