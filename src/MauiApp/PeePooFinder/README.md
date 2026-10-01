# PeePoo Finder (.NET MAUI)

.NET 10 MAUI client for the PeePoo API. Anyone can browse nearby restrooms,
the map and reviews without an account; signing in unlocks adding places (with
GPS location and a photo), reviews, saved places, confirming that a place is
still good, reporting and blocking. Profile includes saved places, places you
added, your reviews, change password and blocked people.

Design: the same wayfinding-signage language as the website (yellow sign
panels, ink outlines) with Bricolage Grotesque and Atkinson Hyperlegible
(both SIL Open Font License, see `Resources/Fonts`).

## Architecture

- **MVVM** with `CommunityToolkit.Mvvm` (`[ObservableProperty]` / `[RelayCommand]`).
- **Shell** navigation (`AppShell`): the app opens on the `Nearby` / `Map` / `You`
  tabs; the sign-in page is only shown when a guest tries to contribute.
  Detail, submit, review and profile sub-pages are pushed routes.
- **Typed HTTP client** (`PeePooApiClient`) registered with `AddHttpClient`, with
  an `AuthMessageHandler` that attaches the bearer token and, when the server
  rejects it (password changed elsewhere, account deleted, ban), clears the
  session and sends the user back to sign in.
- **Secure session**: the JWT is stored in `SecureStorage`; only non-sensitive
  display data lives in `Preferences`.

## Configuration

`appsettings.json` (embedded) holds non-secret settings:

```json
{
  "Api": { "BaseUrl": "https://peepoo.azurewebsites.net" },
  "GoogleMaps": { "ApiKey": "" }
}
```

Point `Api:BaseUrl` at your API (e.g. `http://10.0.2.2:5099` for the Android
emulator talking to a local API).

The **Map** tab uses the native map (Google Maps on Android). Supply your own
restricted key at build time — it is injected into the Android manifest via a
placeholder, never committed:

```bash
dotnet build -f net10.0-android -p:MapsApiKey=YOUR_ANDROID_MAPS_KEY
```

The rest of the app (nearby search, distance, directions) works without a key.

## Building

The Android SDK, a JDK and the MAUI Android workload are required.

```bash
dotnet workload install maui-android
dotnet build -f net10.0-android -c Debug \
  -p:AndroidSdkDirectory=<android-sdk> -p:JavaSdkDirectory=<jdk>
```

### iOS

iOS shares all of this project's code. To build it, add `net10.0-ios` back to
`<TargetFrameworks>` (it is dropped on Linux), install the `maui-ios` workload
and build on macOS.
