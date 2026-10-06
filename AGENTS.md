# Guide for coding agents

PeePoo Finder: a community map of restrooms. One .NET 10 backend serves both the
JSON API and the public website; a .NET MAUI app is the mobile client.
Human docs: `README.md` (run/config), `CHANGELOG.md` (history, what was verified,
**open pending items** — read its "Pendiente" section before starting work),
`docs/API.md` (endpoints), `RELEASING.md` (store builds).

## Where things are

| Path | What |
|---|---|
| `src/PeePoo/API` | ASP.NET Core host: `Program.cs` (pipeline, rate limits, CSP), `Controllers/` (JSON API), `Pages/` (Razor Pages website, Spanish), `wwwroot/` (CSS, JS, self-hosted fonts and Leaflet), `Services/` (tokens, local photo storage, dev DB), `Site/` (site options, Spanish formatting, guides list, sitemap/robots) |
| `src/PeePoo/Application` | MediatR handlers by feature: `Places/`, `Visits/` (= reviews), `Profiles/`, `Photos/`, `Moderation/`; shared rules in `Core/` |
| `src/PeePoo/Domain` | EF entities (`Place`, `Visit`, `Photo`, `ApplicationUser`, `Report`, `UserBlock`…) |
| `src/PeePoo/Persistence` | `DataContext`, SQL Server migrations, `Seed.cs` (roles/admin always; demo data in Development) |
| `src/PeePoo/Infrastructure` | Cloudinary photos, current-user accessor, owner authorization policies |
| `src/PeePoo/Tests` | xUnit: handler tests + `Integration/` (real HTTP pipeline via `WebApplicationFactory`, incl. website pages) |
| `src/MauiApp/PeePooFinder` | MAUI app: `ViewModels/` (CommunityToolkit.Mvvm), `Views/` (XAML), `Services/PeePooApiClient.cs`, styles in `Resources/Styles` |
| `store/` | Store listings and screenshots |

## Commands

```bash
cd src/PeePoo
dotnet build PeePoo.sln -c Release
dotnet test Tests/Tests.csproj -c Release          # must stay green
dotnet run --project API                            # http://localhost:5093, SQLite + demo data
docker compose up                                   # from repo root, http://localhost:8080
```

Demo logins (Development only): `starling@test.com` / `Pa$$w0rd`; admin
`admin@peepoo.local` / `ChangeMe-Admin-2026!`.

New SQL Server migration (production uses migrations; local SQLite is created from the model):

```bash
dotnet tool install --global dotnet-ef
Database__Provider=SqlServer ConnectionStrings__DefaultConnection="Server=.;Database=x;Trusted_Connection=True;TrustServerCertificate=True" \
ASPNETCORE_ENVIRONMENT=Production TokenKey=design-time-key-0123456789-abcdefghijklmnopqrstuvwxyz \
dotnet ef migrations add <Name> -p Persistence -s API
```
When you add columns, also update `API/Services/DevDatabase.cs` (`IsCurrentAsync`) so
old local SQLite files are rebuilt.

Mobile app: `dotnet build -f net10.0-android` needs the MAUI workload and an Android SDK.
CI (`.github/workflows/ci.yml`) builds it on every push, together with backend tests,
a vulnerable-package check and a Docker image smoke test. Without an Android SDK you can
still type-check C#/XAML by pointing `-p:AndroidSdkDirectory` at a stub SDK; the
Java packaging step will fail, but `obj/Debug/net10.0-android/PeePooFinder.dll` is
produced only if C# and XAML compile.

## Conventions that matter

- **Never bind entities from requests.** Clients send input DTOs (`PlaceInput`,
  `VisitInput`, `VisitEditInput`) with only the fields they may set; handlers copy
  fields explicitly (`PlaceQueries.Apply`). Responses are DTOs projected with
  `ProjectTo(..., new { currentUsername })` — never return entities.
- Handlers return `Result<T>`: `null` → 404, failure → 400 `{ message }`, `Unit` → 204
  (`BaseApiController.HandleResult`). Validation errors also return `{ message, errors }`.
  The app shows `message`, so write it for end users.
- Validation lives in FluentValidation validators next to the input DTOs.
- Controllers require auth by default (global filter); mark public reads `[AllowAnonymous]`.
  Ownership uses the `IsPlaceOwner` / `IsVisitOwner` policies (admins pass).
- Visibility: `Place.IsAproved` (sic) = visible, `Visit.IsHidden` = hidden. Public queries
  must filter them (see `PlaceQueries.VisibleTo`).
- Anything that changes a user's security stamp (password change, ban, deletion) must
  also call `IMemoryCache.InvalidateSessionCache(userId)`, or old tokens keep working
  for up to 30 s.
- Deleting content must delete stored photos too (`PhotoCleanup.DeleteAsync`).
- **Website / CSP:** the policy is `'self'` only. No inline `<script>`, no `style="…"`
  attributes, no CDN links — add CSS classes to `wwwroot/css/site.css` and JS files to
  `wwwroot/js`. JSON for scripts goes in `<script type="application/json">`.
  User text from JS must be set with `textContent`.
- Site copy is Spanish (`lang="es"`); the English legal URLs (`/privacy`, `/terms`,
  `/account-deletion`) are linked from the store listings — keep them.
- Design language (web and app): wayfinding signage — honey yellow `#FCD24F` sign
  panels, ink `#1E1410` 2px outlines, small radii, hard offset shadows, Bricolage
  Grotesque (headings) + Atkinson Hyperlegible (body). Avoid gradients, emoji and
  generic rounded card grids. Tokens: top of `site.css`; app: `Resources/Styles/Colors.xaml`
  and `AppStyles.xaml`.
- App: guests can browse; gate contributions with `EnsureSignedInAsync`; wrap API calls
  in `RunAsync` (shows server messages, handles offline). App UI text is English.
- `UseRouting()` is called explicitly after the status-code re-execute middleware in
  `Program.cs` so the friendly 404 page works — keep that order.
- Rate limits are configurable (`RateLimits:*`); integration tests raise them.

## Before you finish

1. `dotnet test` passes (add tests for new handlers; HTTP-level ones in `Tests/Integration`).
2. For website changes, load the page and check the browser console for CSP errors.
3. Update `docs/API.md` for endpoint changes and add an entry to `CHANGELOG.md`
   (what, why, how verified; move items out of "Pendiente" when done).
4. Never commit secrets (`TokenKey`, connection strings, Cloudinary keys); use
   environment variables or user-secrets.
