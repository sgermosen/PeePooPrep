# Store listing kit — PeePoo Finder

Everything to publish the app, ready to paste. Fill the `YOUR-API.com` /
`YOUR-WEB.com` placeholders with your real domain.

## Sheets
- [`google-play.md`](google-play.md) — Google Play listing, content rating, data safety
- [`app-store.md`](app-store.md) — App Store info, keywords, privacy, review notes

## Assets (`assets/`)
| File | Use |
|---|---|
| `appicon-512.png` | Google Play app icon (512×512) |
| `appicon-1024.png` | App Store app icon (1024×1024) |
| `feature-1024x500.png` | Google Play feature graphic |
| `shot1-explore.png` … `shot5-login.png` | Phone screenshots (1080×1920) — Google Play |
| `appstore/shot1-explore.png` … `shot5-login.png` | Phone screenshots (1290×2796) — App Store 6.7" |
| `icon-master.svg` | Vector master of the brand mark |

The in-app icon and splash live in `src/MauiApp/PeePooFinder/Resources/`
(`AppIcon/appicon.svg` + `AppIcon/appiconfg.svg`, `Splash/splash.svg`) and MAUI
generates every density from them at build time.

## Before you submit
- Point the privacy / terms / account-deletion URLs at your API domain (the
  backend serves `/privacy`, `/terms` and `/account-deletion`).
- Create a real demo account in production for App Review.
- Rotate and restrict the Google Maps key; build the app with `-p:MapsApiKey=…`.
