# Apple Sign In with Apple - OAuth Setup Guide

**Goal:** Configure your Apple Developer account to enable parents to authenticate PocketFence with their Apple IDs and grant access to Screen Time data.

**Prerequisites:**
- Active Apple Developer account ($99/year)
- Access to https://developer.apple.com

**Time Required:** 15-20 minutes

---

## Step 1: Create App ID

1. Go to https://developer.apple.com/account
2. Click **Certificates, Identifiers & Profiles**
3. Select **Identifiers** from the sidebar
4. Click the **+** button to create a new identifier
5. Select **App IDs** and click **Continue**

### Configure App ID:
- **Description:** `PocketFence Family`
- **Bundle ID:** `com.pocketfence.family` (or your preference)
- **Explicit App ID** (not wildcard)

### Enable Capabilities:
- ✅ Check **Sign in with Apple**
- Click **Configure** next to Sign in with Apple
  - Select **Enable as a primary App ID**
  - Click **Save**

6. Click **Continue** → **Register**

---

## Step 2: Create Services ID (OAuth Client)

This is what identifies your web app to Apple's OAuth system.

1. Go back to **Identifiers**
2. Click **+** button again
3. Select **Services IDs** and click **Continue**

### Configure Services ID:
- **Description:** `PocketFence Family Web`
- **Identifier:** `com.pocketfence.family.web`
  - This becomes your OAuth Client ID
- ✅ Check **Sign in with Apple**
- Click **Configure** next to Sign in with Apple

### Configure Web Authentication:
- **Primary App ID:** Select `com.pocketfence.family` (from Step 1)
- **Website URLs:**
  - **Domains and Subdomains:**
    - Enter: `pocketfence.net` (your actual domain)
  - **Return URLs:**
    - Click **+** to add: `https://pocketfence.net/OAuth/AppleCallback`
    
⚠️ **IMPORTANT - Apple Does NOT Allow Localhost:** 
- Apple requires REAL domains for both Domains and Return URLs
- localhost, 127.0.0.1, and local IP addresses are NOT allowed
- For local development, use one of these options:
  1. **ngrok** (recommended): Creates a tunnel to your local server
     - `ngrok http 5000` gives you a public URL like `https://abc123.ngrok.io`
     - Add ngrok URL to Return URLs: `https://abc123.ngrok.io/OAuth/AppleCallback`
  2. **Production domain only**: Test OAuth in production environment
  3. **Skip Apple OAuth in dev**: Use password authentication locally

4. Click **Save** → **Continue** → **Register**

**Important:** Save your Services ID (`com.pocketfence.family.web`) - this is your **Client ID**.

---

## Step 3: Create Private Key for Client Secret

Apple requires you to generate a JWT client secret using a private key.

1. Go to **Keys** in the sidebar
2. Click **+** button to create a new key
3. **Key Name:** `PocketFence Sign in with Apple Key`
4. ✅ Check **Sign in with Apple**
5. Click **Configure** next to Sign in with Apple
   - **Primary App ID:** Select `com.pocketfence.family`
   - Click **Save**
6. Click **Continue** → **Register**

### Download the Key:
⚠️ **CRITICAL:** You can only download this once!

7. Click **Download** to get the `.p8` file
   - File name format: `AuthKey_XXXXXXXXXX.p8`
   - The `XXXXXXXXXX` is your **Key ID** - save this!

8. **Store securely:**
   - Copy the `.p8` file to: `PocketFence-Family/Data/AppleAuthKey.p8`
   - Add to `.gitignore` (DO NOT commit to git)
   - Back up somewhere safe

9. Note your **Key ID** (shown on the key details page, looks like `AB12CD34EF`)

---

## Step 4: Get Your Team ID

1. Go to **Membership** in the sidebar (or top-right account menu)
2. Find **Team ID** (looks like `TEAM123456`)
3. Copy this - you'll need it for the client secret

---

## Step 5: Configure PocketFence

You now have all the credentials needed. Add them to your `appsettings.json`:

```json
{
  "Apple": {
    "ClientId": "com.pocketfence.family.web",
    "TeamId": "TEAM123456",
    "KeyId": "AB12CD34EF",
    "PrivateKeyPath": "Data/AppleAuthKey.p8",
    "RedirectUri": "http://localhost:5000/OAuth/AppleCallback"
  }
}
```

**For Production:** Update `RedirectUri` to your public domain.

---

## Step 6: Request Additional Scopes (Optional - For Screen Time)

Sign in with Apple has limited scopes by default. To access Family Sharing and Screen Time data, you may need to:

1. Contact Apple Developer Support
2. Request access to **FamilyControls framework** APIs
3. Explain your use case (parental control app)
4. Wait for approval (can take 1-2 weeks)

**OR** use the publicly available APIs:
- Family Sharing member list (public)
- Device management endpoints (public)
- Screen Time reports (may be limited)

**Note:** Full Screen Time API access might require App Review and specific entitlements. We'll implement what's publicly available first.

---

## Step 7: Test the Configuration

Once your app code is ready, test locally:

1. Start your PocketFence dashboard: `dotnet run dashboard`
2. Navigate to Device Linking page
3. Click "Link iOS Device"
4. Should redirect to Apple's consent screen
5. Sign in with your Apple ID
6. Grant permissions
7. Redirected back to `http://localhost:5000/OAuth/AppleCallback`

**Common Issues:**
- "Invalid redirect URI" → Check Services ID configuration matches exactly
- "Invalid client" → Check Client ID in appsettings.json
- "Invalid client secret" → Check private key file path and Key ID
- "Localhost invalid in Domains" → Don't put localhost in Domains field, use pocketfence.net
- "Localhost invalid in Return URLs" → Make sure you include `http://` protocol

---

## Security Best Practices

### DO:
- ✅ Store `.p8` private key securely (never commit to git)
- ✅ Use environment variables for production credentials
- ✅ Validate the `state` parameter to prevent CSRF
- ✅ Store access tokens encrypted in database
- ✅ Use refresh tokens to get new access tokens

### DON'T:
- ❌ Commit private key to GitHub
- ❌ Share your Key ID publicly
- ❌ Hardcode credentials in source code
- ❌ Store tokens in plain text
- ❌ Skip state parameter validation

---

## What We Have Access To

With standard Sign in with Apple + Family Sharing APIs:

✅ **Available:**
- User's Apple ID (email)
- Family Sharing member list
- Child devices in family
- Basic device info (model, OS version)
- Some Screen Time data (via reports API)

❌ **Requires Special Access:**
- Real-time Screen Time enforcement
- Programmatic downtime changes
- Deep app restriction controls
- May need App Review or enterprise agreement

**Strategy:** Start with what's publicly available. Request extended access as app matures.

---

## Next Steps

1. ✅ Complete this setup (you're doing it now)
2. Add credentials to `appsettings.json`
3. Run the OAuth flow test
4. Implement device linking UI
5. Test with real Apple ID and Family Sharing

**Setup complete? Let's build the OAuth service code!**
