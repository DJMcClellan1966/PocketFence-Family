# Screenshot Organization Guide

## 📁 Directory Structure

```
wwwroot/images/guides/
├── ios/
│   ├── 01-settings-app.png
│   ├── 02-screentime-location.png
│   ├── 03-turn-on-screentime.png
│   ├── 04-this-is-my-childs-iphone.png
│   ├── 05-app-limits-menu.png
│   ├── 06-add-limit-button.png
│   ├── 07-select-social-networking.png
│   ├── 08-set-time-1-minute.png
│   ├── 09-add-limit-confirmation.png
│   ├── 10-content-restrictions-toggle.png
│   ├── 11-web-content-menu.png
│   └── 12-limit-adult-websites.png
├── android/
│   ├── 01-family-link-app-store.png
│   ├── 02-open-family-link.png
│   ├── 03-add-child-button.png
│   ├── 04-enter-child-email.png
│   ├── 05-set-restrictions.png
│   └── 06-confirm-setup.png
└── windows/
    ├── 01-family-safety-website.png
    ├── 02-sign-in-microsoft.png
    ├── 03-add-family-member.png
    ├── 04-enter-child-email.png
    ├── 05-content-filters.png
    └── 06-screen-time-limits.png
```

## 📝 Naming Convention

**Format:** `[step-number]-[action-description].png`

**Examples:**
- ✅ `01-settings-app.png`
- ✅ `05-app-limits-menu.png`
- ✅ `12-limit-adult-websites.png`
- ❌ `Screenshot_20260106_143522.png` (bad - no context)
- ❌ `IMG_1234.png` (bad - meaningless)

**Rules:**
- Prefix with 2-digit step number (01, 02, 03...)
- Use hyphens between words (not spaces or underscores)
- Keep names short but descriptive
- All lowercase
- Always use `.png` (better quality than .jpg for screenshots)

## 📸 Screenshot Checklist

### Before Taking Screenshots:

**iPhone/iPad:**
- [ ] Clean up: Close notifications, clear icons from status bar
- [ ] Full charge: Better if battery shows 100%
- [ ] Brightness: 75-100% for clear visibility
- [ ] Portrait mode (most natural for phones)
- [ ] Time: Set to 9:41 AM (Apple's default marketing time)

**Android:**
- [ ] Similar cleanup
- [ ] Use demo mode if available: Developer options → Demo Mode

**Windows:**
- [ ] 1920x1080 resolution minimum
- [ ] Hide personal info in screenshots
- [ ] Close unnecessary windows

### Annotation Style:

**Use Consistently:**
- 🟢 **Green arrows** → "Tap here" / "Click this"
- 🔴 **Red circles** → "Find this text" / "Important"
- 🟡 **Yellow highlights** → Background emphasis
- 📝 **Text boxes** → Step explanations (Arial, 16-18pt)
- 🔍 **Magnifier** (iOS only) → Zoom small buttons

**Text Guidelines:**
- Keep text short: "Tap Screen Time" not "Now you need to tap on the Screen Time option"
- Number steps: "Step 1 of 5"
- Use emojis sparingly: ⚠️ for warnings, ✅ for success

## 🎨 Annotation Examples

### Good Annotation:
```
[Screenshot with green arrow pointing to button]
Text box: "Step 3: Tap 'Add Limit'"
```

### Bad Annotation:
```
[Screenshot with messy circles everywhere]
Text: "Here you can see the button that you need to click which is the add limit button and it's located in the middle of the screen..."
```

## 📐 Image Specifications

**File Format:** PNG (lossless, best for UI screenshots)
**iPhone:** 1170x2532 (native resolution) → Resize to 585x1266 for web
**Android:** Varies → Resize to 540x1080 common
**Windows:** 1920x1080 → Crop to relevant areas

**Optimization:**
- Use TinyPNG or similar to compress (50-70% smaller)
- Keep file size under 300KB per screenshot
- Batch resize: Use ImageMagick or Photoshop actions

## 🚀 Quick Workflow

### For iOS (Recommended):

1. **Take screenshots on iPhone:**
   - Side button + Volume Up → Screenshot
   - Tap thumbnail in corner → Markup editor opens

2. **Annotate on iPhone:**
   - Tap **+** button → Select arrow/text/magnifier
   - Add green arrows for "tap here"
   - Add text boxes for explanations
   - Tap "Done"

3. **Transfer to Windows:**
   - AirDrop to Mac then copy, OR
   - Email to yourself, OR
   - iCloud Photos sync

4. **Organize on Windows:**
   - Save to correct folder (ios/)
   - Rename: `01-settings-app.png`
   - Compress with TinyPNG if > 300KB

### For Android/Windows:

1. **Capture with Greenshot:**
   - Press `PrtScn` → Select area
   - Editor opens automatically

2. **Annotate:**
   - Add arrows (green)
   - Add text boxes
   - Add circles/highlights

3. **Save:**
   - Save to guides/android/ or guides/windows/
   - Use naming convention: `01-description.png`

## 📊 Progress Tracking

**iOS Screenshots Needed:** ~12-15
- [ ] Settings & Screen Time (3 shots)
- [ ] Blocking Apps Example (4 shots)
- [ ] Time Limits Example (3 shots)
- [ ] Content Restrictions (2-3 shots)

**Android Screenshots Needed:** ~8-10
- [ ] Family Link Installation (2 shots)
- [ ] Adding Child (3 shots)
- [ ] Setting Restrictions (3 shots)

**Windows Screenshots Needed:** ~6-8
- [ ] Family Safety Website (2 shots)
- [ ] Adding Child (2 shots)
- [ ] Content Filters (2-3 shots)

## 💡 Pro Tips

1. **Take extras:** Capture 2-3 shots of same screen from different angles
2. **Crop tight:** Remove unnecessary UI (just show relevant area)
3. **Consistency:** Use same annotation colors across all screenshots
4. **Test on phone:** View screenshots on mobile - are arrows/text readable?
5. **Version control:** Keep originals in separate folder before annotation

## 🔗 Using Screenshots in HTML

**In Guides/Index.cshtml:**

```html
@if (Model.DeviceType == "iOS")
{
    <h2>Step 1: Open Settings</h2>
    <img src="~/images/guides/ios/01-settings-app.png" 
         class="img-fluid rounded shadow mb-3" 
         alt="iPhone Settings app icon">
    <p>Find and tap the Settings app on your iPhone...</p>
}
```

**Bootstrap Classes:**
- `img-fluid` → Responsive (scales with screen)
- `rounded` → Rounded corners
- `shadow` → Subtle drop shadow
- `mb-3` → Margin bottom spacing

## ✅ Quality Checklist

Before publishing, verify each screenshot:
- [ ] Clear and readable text (not blurry)
- [ ] Arrows point to correct elements
- [ ] No personal information visible
- [ ] Consistent annotation style
- [ ] File size optimized (< 300KB)
- [ ] Properly named with step number
- [ ] Tested on mobile device view

---

**Need Help?**
- iPhone Markup Tutorial: Settings → Photos → Select photo → Edit → Markup
- Greenshot Docs: https://getgreenshot.org/help/
- TinyPNG: https://tinypng.com/
