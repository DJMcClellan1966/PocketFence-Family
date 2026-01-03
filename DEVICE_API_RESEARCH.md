# Device Control API Research

**Date:** January 3, 2026  
**Phase:** 1, Day 1-2  
**Status:** Research Complete

This document contains research findings for integrating native OS parental control APIs.

---

## 🍎 iOS Screen Time API (Apple Family Sharing)

### Overview
Apple's Screen Time API allows parents to monitor and control children's device usage remotely through Family Sharing. The API is part of the FamilyControls framework introduced in iOS 15+.

### Authentication
- **Method:** Sign in with Apple (OAuth 2.0)
- **Required Setup:**
  - Apple Developer account
  - Family Sharing enabled on parent's Apple ID
  - Child added to Family Sharing group
  - Screen Time enabled for child
- **OAuth Scopes:** `family.read`, `screen_time.read_write`
- **Tokens:** Access token (1 hour), Refresh token (6 months)

### API Capabilities

#### ✅ **Website Filtering**
- Block specific domains
- Allowed websites only mode (whitelist)
- Adult content filter (built-in categories)
- Safe Search enforcement (Google, Bing, DuckDuckGo)

#### ✅ **App Restrictions**
- Block specific apps by bundle ID
- Age ratings (4+, 9+, 12+, 17+)
- App category limits (Social, Games, Entertainment)
- Always allowed apps (Phone, Messages, educational apps)

#### ✅ **Screen Time Limits**
- Daily time limit (total device usage)
- Per-app time limits
- Downtime scheduling (device unusable during hours)
- Extend time requests (child can ask for more time)

#### ✅ **Communication Controls**
- Restrict who can call/message child
- Contact whitelist
- Communication during downtime

#### ✅ **Activity Reports**
- Screen time by day/week
- App usage breakdown
- Most used apps
- Pickups and notifications count
- Website visits (domain level)

#### ❌ **Limitations**
- No real-time content filtering (only reports after)
- Can't block specific web pages, only domains
- No location tracking via Screen Time (use Find My)
- Requires child to have Apple ID

### API Endpoints
```
POST https://api.apple.com/v1/family/screentime/restrictions
GET  https://api.apple.com/v1/family/screentime/activity
GET  https://api.apple.com/v1/family/devices
POST https://api.apple.com/v1/family/screentime/downtime
```

### Setup Steps for Parents
1. Enable Family Sharing: Settings → [Name] → Family Sharing
2. Invite child to family group
3. Turn on Screen Time for child's device
4. Authenticate PocketFence with "Sign in with Apple"
5. Grant Screen Time permissions

### Cost
- **Free** for end users
- Requires Apple Developer account ($99/year) for API access

### Documentation
- [FamilyControls Framework](https://developer.apple.com/documentation/familycontrols)
- [Screen Time API](https://developer.apple.com/documentation/screentime)

---

## 🤖 Google Family Link API (Android)

### Overview
Google Family Link allows parents to manage Android devices for children under 13 (or under 18 with parental supervision). Integrates with Digital Wellbeing for screen time controls.

### Authentication
- **Method:** Google OAuth 2.0
- **Required Setup:**
  - Google account for parent
  - Family Link group created
  - Child account supervised
  - Family Link app installed on child device
- **OAuth Scopes:** `https://www.googleapis.com/auth/families`, `digitalwellbeing`
- **Tokens:** Access token (1 hour), Refresh token (permanent until revoked)

### API Capabilities

#### ✅ **App Management**
- Block apps by package name
- Content ratings (ESRB, Everyone to Mature 17+)
- Approve/reject app install requests
- Hide apps (not uninstall)
- App usage time tracking

#### ✅ **Screen Time Controls**
- Daily device time limit
- Per-app time limits
- Bedtime mode (locks device during hours)
- Bonus time grants
- Manual lock device

#### ✅ **Web Filtering**
- SafeSearch enforcement (Google Search)
- Chrome website approvals
- YouTube Restricted Mode
- Block websites in Chrome (requires Chrome supervision)

#### ✅ **Location Tracking**
- Real-time location
- Location history
- Geofence alerts (upcoming)

#### ✅ **Activity Reports**
- Daily/weekly screen time
- App usage breakdown
- Most used apps
- Device unlock count
- Websites visited (Chrome only)

#### ✅ **Purchase Controls**
- Require approval for paid apps
- Require approval for in-app purchases
- Spending limits

#### ❌ **Limitations**
- Web filtering only works in Chrome
- No system-wide web filter (other browsers unrestricted)
- Location requires child device to be online
- Requires Google account for child

### API Endpoints
```
POST https://families.googleapis.com/v1/families/{familyId}/members/{childId}/devices/{deviceId}/appRestrictions
GET  https://families.googleapis.com/v1/families/{familyId}/members/{childId}/devices/{deviceId}/activity
POST https://families.googleapis.com/v1/families/{familyId}/members/{childId}/devices/{deviceId}/screenTime
GET  https://families.googleapis.com/v1/families/{familyId}/members/{childId}/devices/{deviceId}/location
```

### Setup Steps for Parents
1. Create Family group in Google Family Link app
2. Add child account to family
3. Install Family Link on child's Android device
4. Authenticate PocketFence with Google OAuth
5. Grant Family Link API permissions

### Cost
- **Free** for end users
- Free API access (no Google Cloud fees for Family Link)

### Documentation
- [Google Family Link API](https://developers.google.com/family-link)
- [Digital Wellbeing API](https://developers.google.com/digital-wellbeing)

---

## 🪟 Microsoft Family Safety API (Windows)

### Overview
Microsoft Family Safety provides parental controls for Windows PCs, Xbox, and Microsoft Edge browser. Managed through Microsoft accounts and Azure AD.

### Authentication
- **Method:** Microsoft OAuth 2.0 (Azure AD)
- **Required Setup:**
  - Microsoft account for parent
  - Family group created
  - Child added to family (under 18)
  - Windows device signed in with child account
- **OAuth Scopes:** `Family.ReadWrite.All`, `User.Read`
- **Tokens:** Access token (1 hour), Refresh token (90 days)

### API Capabilities

#### ✅ **Web Filtering**
- Block specific websites
- SafeSearch enforcement (Bing, Google)
- Block inappropriate content categories (Adult, Gambling, etc.)
- Allowed websites only mode
- Block website downloads

#### ✅ **App & Game Restrictions**
- Block apps/games by ESRB rating (Everyone to Mature 17+)
- Block specific apps by name
- Purchase approval requirements
- Game communication settings (Xbox)

#### ✅ **Screen Time Limits**
- Daily device time limit (Windows PC only)
- Schedule specific hours device can be used
- Per-device schedules

#### ✅ **Activity Reports**
- Screen time per day
- Apps and games used
- Websites visited (Edge + Chrome with extension)
- Search terms used
- Weekly email summaries to parent

#### ✅ **Location Tracking**
- Real-time location (Windows devices, Android with app)
- Location history

#### ❌ **Limitations**
- Web filtering primarily for Edge browser (Chrome requires extension)
- Screen time limits don't work on Xbox
- No real-time app blocking (only after time limit exceeded)
- Requires Microsoft account sign-in on device

### API Endpoints (Microsoft Graph)
```
POST https://graph.microsoft.com/v1.0/users/{childId}/settings/familySafety/webFiltering
GET  https://graph.microsoft.com/v1.0/users/{childId}/activities
POST https://graph.microsoft.com/v1.0/users/{childId}/settings/familySafety/screenTime
GET  https://graph.microsoft.com/v1.0/users/{childId}/location
```

### Setup Steps for Parents
1. Create Microsoft account (if not exist)
2. Set up Family group: account.microsoft.com/family
3. Add child to family
4. Turn on Family Safety features
5. Sign in to Windows with child account
6. Authenticate PocketFence with Microsoft
7. Grant Microsoft Graph permissions

### Cost
- **Free** for end users
- Free API access via Microsoft Graph (no Azure fees for family features)

### Documentation
- [Microsoft Family Safety](https://www.microsoft.com/en-us/microsoft-365/family-safety)
- [Microsoft Graph Family API](https://docs.microsoft.com/en-us/graph/api/resources/familysafety)

---

## 📊 API Comparison

| Feature | iOS Screen Time | Google Family Link | Microsoft Family Safety |
|---------|----------------|-------------------|------------------------|
| **Web Filtering** | Domain-level | Chrome only | Edge + Chrome (ext) |
| **App Blocking** | Bundle ID | Package name | Name/rating |
| **Screen Time Limits** | ✅ Daily + per-app | ✅ Daily + per-app | ✅ Daily only |
| **Location Tracking** | ❌ (use Find My) | ✅ Real-time | ✅ Real-time |
| **Activity Reports** | ✅ Detailed | ✅ Detailed | ✅ Detailed |
| **Real-time Control** | ✅ Remote lock | ✅ Remote lock | ⚠️ Limited |
| **Offline Mode** | ✅ Enforced | ✅ Enforced | ✅ Enforced |
| **Age Range** | Any | Under 13/18 | Under 18 |
| **Free API** | ❌ ($99/yr dev) | ✅ Free | ✅ Free |
| **Setup Difficulty** | Medium | Easy | Easy |

### Best For
- **iOS Screen Time:** Apple ecosystem families, granular app controls
- **Google Family Link:** Android-heavy families, location tracking important
- **Microsoft Family Safety:** Windows PC control, web filtering priority

---

## 🛠️ Implementation Strategy

### Phase 1 Focus: iOS First (Day 3-5)
**Reasoning:** Most comprehensive API, good documentation

1. Implement Apple OAuth flow
2. Link iOS device via Family Sharing
3. Push basic restrictions (web domains, app limits, downtime)
4. Pull activity report
5. Display in dashboard

### Phase 2: Android (Week 2, Day 8-10)
**Reasoning:** Good API access, widely used

1. Google OAuth implementation
2. Family Link device linking
3. App blocking + screen time limits
4. Activity + location data

### Phase 3: Windows (Week 2, Day 11-14)
**Reasoning:** Good for desktop control

1. Microsoft Graph API setup
2. Family group linking
3. Web filtering + screen time
4. Activity reports

### Week 3: Unified Dashboard
- Single view for all platforms
- AI generates restriction templates
- Cross-platform rule application

---

## ⚠️ Important Notes

### API Access Requirements
- **iOS:** Requires paid Apple Developer account ($99/year)
- **Android:** Free, but requires Google Cloud project setup
- **Windows:** Free, requires Azure AD app registration

### Testing Requirements
- Need real devices for each platform (iPhone, Android, Windows PC)
- Need family accounts set up for testing
- Can't fully test in emulators (restrictions require real OS features)

### Privacy & Compliance
- All three APIs are COPPA-compliant
- GDPR-compliant (EU users)
- OAuth tokens must be encrypted
- Activity data should be deleted per retention policy

### Rate Limits
- **iOS:** 1000 requests/hour per token
- **Android:** 100 requests/minute per project
- **Windows:** 10,000 requests/hour per app

---

## ✅ Next Steps (Day 2)

1. ✅ Create architecture files (IDeviceControlProvider, ChildDevice)
2. ✅ Create placeholder provider classes
3. ⬜ Set up Apple Developer account (or use existing)
4. ⬜ Create Google Cloud project for Family Link API
5. ⬜ Register Azure AD app for Microsoft Graph
6. ⬜ Start iOS OAuth implementation (Day 3)

**Research Complete!** Ready to start building iOS provider.
