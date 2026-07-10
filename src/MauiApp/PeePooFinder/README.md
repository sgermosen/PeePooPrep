# PeePoo Finder (.NET MAUI)

Modern .NET 10 MAUI client for the PeePoo API, replacing the retired
Xamarin.Forms app. It lets people sign in, browse nearby restrooms, read and
write reviews, add new spots (with GPS location and a photo) and manage their
profile.

## Architecture

- **MVVM** with `CommunityToolkit.Mvvm` (`[ObservableProperty]` / `[RelayCommand]`).
- **Shell** navigation (`AppShell`): a login page plus an `Explore` / `Profile`
  tab bar; detail, submit and review pages are pushed routes.
- **Typed HTTP client** (`PeePooApiClient`) registered with `AddHttpClient`, with
  an `AuthMessageHandler` that attaches the bearer token to every request.
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
emulator talking to a local API). The Google Maps key is intentionally empty —
supply your own, restricted key; never commit a real one.

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
