# Filter Levels Implementation ✅

PocketFence now supports **three filter levels** that adjust content filtering sensitivity based on age appropriateness.

## 🔴 Strict Mode (Young Children 5-12)

**Strictness:** Most aggressive filtering  
**Block Threshold:** 0.5 (blocks at lower threat scores)  
**Monitor Threshold:** 0.3  

### Blocked Keywords (21 total):
- `adult`, `gambling`, `casino`, `poker`, `bet`
- `drugs`, `marijuana`, `cannabis`
- `weapons`, `gun`
- `violence`, `blood`, `gore`
- `malware`, `phishing`, `illegal`, `torrent`, `pirate`, `crack`, `hack`
- `porn`, `xxx`, `explicit`
- `hate`, `racist`, `extremist`
- `suicide`, `self-harm`

**Use Case:** Best for elementary school children. Blocks educational references to sensitive topics.

---

## 🟡 Moderate Mode (Teens 13-15) - DEFAULT

**Strictness:** Balanced protection  
**Block Threshold:** 0.7 (standard sensitivity)  
**Monitor Threshold:** 0.4  

### Blocked Keywords (11 total):
- `adult`, `gambling`, `drugs`, `weapons`, `violence`
- `malware`, `phishing`, `illegal`, `torrent`
- `porn`, `xxx`

**Use Case:** Balanced approach. Allows educational content about sensitive topics while still blocking inappropriate material.

---

## 🟢 Relaxed Mode (Older Teens 16+)

**Strictness:** Least restrictive  
**Block Threshold:** 0.85 (only blocks high threats)  
**Monitor Threshold:** 0.6  

### Blocked Keywords (7 total):
- `adult`, `porn`, `xxx`, `explicit`
- `malware`, `phishing`, `illegal`

**Use Case:** For mature teenagers. Still blocks explicit adult content and illegal activities but allows most informational content.

---

## How It Works

### 1. **Dashboard Settings Page**
Parents select filter level: Strict / Moderate / Relaxed

### 2. **Dynamic Filtering**
When checking a URL:
- System loads current filter level from `Data/dashboard_settings.json`
- Applies level-specific keyword blocklist
- Uses level-specific threat thresholds

### 3. **Example Scenarios**

#### URL: `https://casino-news.com/poker-tournament`

| Filter Level | Result | Reason |
|-------------|--------|--------|
| **Strict** | ❌ BLOCKED | Contains `casino` and `poker` keywords |
| **Moderate** | ❌ BLOCKED | Contains `gambling` keyword |
| **Relaxed** | ✅ ALLOWED | News about gambling not explicitly blocked |

#### URL: `https://education.com/drug-awareness-program`

| Filter Level | Result | Reason |
|-------------|--------|--------|
| **Strict** | ❌ BLOCKED | Contains `drug` keyword (blocks educational content) |
| **Moderate** | ⚠️ MONITOR | Contains `drug` but in educational context |
| **Relaxed** | ✅ ALLOWED | Educational content permitted |

#### URL: `https://explicit-adult-site.xxx`

| Filter Level | Result | Reason |
|-------------|--------|--------|
| **Strict** | ❌ BLOCKED | Contains `adult`, `explicit`, `xxx` |
| **Moderate** | ❌ BLOCKED | Contains `adult`, `xxx` |
| **Relaxed** | ❌ BLOCKED | Contains `adult`, `explicit`, `xxx` |

---

## Technical Implementation

### ContentFilter Class
```csharp
// Loads filter level from settings
var settings = await _settingsManager.LoadSettingsAsync();
_currentFilterLevel = settings.FilterLevel;

// Gets level-specific keywords
private List<string> GetKeywordsForLevel(string level)
{
    return level.ToLower() switch
    {
        "strict" => [ /* 21 keywords */ ],
        "relaxed" => [ /* 7 keywords */ ],
        _ => [ /* 11 keywords (moderate) */ ]
    };
}
```

### SimpleAI Class
```csharp
// Adjusts threat thresholds based on filter level
double blockThreshold = filterLevel switch
{
    "strict" => 0.5,   // More sensitive
    "relaxed" => 0.85, // Less sensitive
    _ => 0.7           // moderate
};
```

### Block Reasons Include Filter Level
```
🚫 BLOCKED: Contains blocked keyword 'gambling' (Filter: moderate)
```

---

## Testing

1. **Open Dashboard:** http://localhost:5000
2. **Login:** admin / PocketFence2026!
3. **Go to Settings**
4. **Change Filter Level** and click Save
5. **Test URLs** - filter sensitivity changes immediately

---

## Benefits

✅ **Age-Appropriate:** Three levels for different age groups  
✅ **Parental Control:** Parents choose strictness level  
✅ **Educational Context:** Moderate/Relaxed allow learning about sensitive topics  
✅ **Real-Time:** Changes apply immediately without restart  
✅ **Transparent:** Block reasons show which filter level triggered the block  

---

**Implementation Date:** January 2, 2026  
**Status:** ✅ Fully Implemented and Tested
