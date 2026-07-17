# App Store — App information (ready to paste)

**Bundle ID:** `com.peepoo.finder`
**Primary category:** Navigation _(secondary: Travel)_
**Price:** Free

---

## App information

### Name _(max 30)_
```
PeePoo Finder
```

### Subtitle _(max 30)_
```
Clean restrooms, right nearby
```

### Promotional text _(max 170)_
```
The fastest way to find a clean restroom near you — map, reviews, directions. Free and ad-free.
```

### Description _(max 4000)_
```
Gotta go? PeePoo Finder helps you find the nearest, cleanest restroom in seconds.

Browse spots near you on a live map, sorted by distance, and see exactly what to expect before you go: rating, type, number of toilets, baby changer, accessibility and whether it's open right now.

WHY YOU'LL LOVE IT
• Nearby first — restrooms sorted by real distance from you
• Live map with pins for every spot
• Honest reviews and photos from the community
• Amenity filters — baby changer, open now, and more
• "Still good?" freshness checks so ratings stay current
• One tap for walking directions
• Add a spot in seconds using your location and a photo

Built by a community that believes finding a decent restroom shouldn't be a gamble. Every spot you add helps someone else.

Free. No ads.
```

### Keywords _(max 100, comma-separated, no spaces)_
```
restroom,toilet,bathroom,near me,public toilet,wc,find toilet,restroom finder,map,reviews
```

### Support URL
```
https://YOUR-API.com/privacy
```
### Marketing URL _(optional)_
```
https://YOUR-WEB.com
```
### Privacy Policy URL
```
https://YOUR-API.com/privacy
```
### Terms of Use URL _(served by the backend)_
```
https://YOUR-API.com/terms
```

---

## Screenshots

Upload for at least the 6.7" size; the 6.5"/5.5" are reused/scaled.

| Size | Pixels | Source |
|---|---|---|
| **6.7" (required)** | **1290×2796** | `assets/appstore/shot1-explore … shot5-login.png` — **ready to upload** |
| 6.5" | 1242×2688 | optional; App Store Connect scales the 6.7" set |
| 5.5" | 1242×2208 | optional |
| App icon | 1024×1024 (no alpha) | `assets/appicon-1024.png` |

The 6.7" set is provided at the exact pixel size Apple requires. The
1080×1920 `assets/shot*.png` are the same frames for Google Play.

---

## Age rating (questionnaire)

- Cartoon/violence/sexual/etc.: **None**
- **User-generated content:** Yes → with filtering, reporting and blocking (see below)
- Unrestricted web access: No

**Result:** With moderation (report + block + admin queue) you can justify **12+**. Without it, Apple typically requires **17+**.

---

## App privacy (nutrition labels)

Same as Google Data safety:

| Data | Linked to user | Purpose |
|---|---|---|
| Email | Yes | App functionality (account) |
| Name (display/username) | Yes | App functionality |
| Coarse location | Yes | App functionality (nearby search) |
| Photos | Yes | App functionality (user content) |
| User content | Yes | App functionality |

Not used for tracking. Not used for ads.

---

## Guideline 1.2 — User-generated content (all required, all implemented)

- ✅ Filter/report objectionable content — in-app **Report** on places and reviews
- ✅ **Block** abusive users — in-app, hides their content
- ✅ Moderation to act on reports — **admin queue** (`/api/admin/reports`, takedowns)
- ✅ Account & data deletion — in-app (Profile → Delete account) + `/account-deletion`
- ✅ EULA / terms with a zero-tolerance policy — served at `https://YOUR-API.com/terms`

---

## App Review notes

```
Demo account (create one in your production database before submitting):
  email: (your demo email)
  password: (your demo password)

The Map tab needs a Google Maps API key set at build time
(-p:MapsApiKey=...). All other features work without it.
Location is used only to sort restrooms by distance and to place a new spot.
```
