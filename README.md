# PeePoo Finder

A community guide to nearby restrooms: hours, accessibility, changing tables
and real reviews. Browse without an account; sign up to contribute.

## What's in the repo

| Path | What it is |
|---|---|
| `src/PeePoo` | .NET 10 backend: JSON API **and** the public website (Razor Pages), Clean Architecture (API, Application, Domain, Infrastructure, Persistence) + tests |
| `src/MauiApp/PeePooFinder` | .NET 10 MAUI mobile app (Android, iOS) |
| `docs/API.md` | API reference |
| `CHANGELOG.md` | What was done, how it was verified and what's pending (Spanish) |
| `store/` | Store listings and screenshots |

## Run it locally (one command)

```bash
docker compose up        # → http://localhost:8080
```

or, with the .NET 10 SDK:

```bash
cd src/PeePoo
dotnet run --project API          # → http://localhost:5093
dotnet test Tests/Tests.csproj    # unit + HTTP integration tests
```

Locally the API uses SQLite and fills itself with demo data around Santo
Domingo. If the local database is from an older version it is rebuilt
automatically.

| Demo account | Email | Password |
|---|---|---|
| Regular user | `starling@test.com` | `Pa$$w0rd` |
| Moderator (`/admin`) | `admin@peepoo.local` | `ChangeMe-Admin-2026!` |

## The website

Served by the same app, in Spanish:

- `/` home · `/explorar` map with filters and "near me" · `/lugar/{id}` place pages (with social cards and schema.org data)
- `/negocios` for businesses · `/guias` guides · `/ayuda` FAQ
- `/privacidad`, `/terminos`, `/eliminar-cuenta` (+ English `/privacy`, `/terms`, `/account-deletion` used by the store listings)
- `/admin` moderation console

The design follows wayfinding signage (yellow sign panels, ink outlines,
pictograms). Fonts and the map library are self-hosted so the Content Security
Policy can stay strict (`'self'` only, no inline scripts or styles).

## Configuration

Never commit secrets. Set these through environment variables (use `__` for
nesting, e.g. `ConnectionStrings__DefaultConnection`) or user-secrets.

| Key | Required | Notes |
|---|---|---|
| `TokenKey` | yes | Random string, 32+ characters. The app refuses to start without it. |
| `Database:Provider` | – | `SqlServer` in production (runs migrations on startup), `Sqlite` otherwise |
| `ConnectionStrings:DefaultConnection` | yes | |
| `Seed:AdminEmail`, `Seed:AdminPassword` | – | Creates/promotes the moderator account on startup |
| `Cloudinary:CloudName`, `ApiKey`, `ApiSecret` | recommended | Without it, photos are stored on local disk under `wwwroot/uploads` (fine for one server; lost on redeploy) |
| `Site:BaseUrl` | recommended | Public origin for canonical links, sitemap and share cards |
| `Site:SupportEmail`, `Site:PlayStoreUrl`, `Site:AppStoreUrl` | – | Store buttons appear on the site once the URLs are set |
| `Cors:AllowedOrigins` | – | Only needed for browser apps on other origins |
| `RateLimits:AuthPerMinute`, `RateLimits:WritesPerMinute` | – | Defaults 10 and 60 |

## Security notes

- Passwords: ASP.NET Identity hashing, lockout after 5 failures.
- JWTs carry the user's security stamp: changing the password, deleting the
  account or being banned ends every existing session.
- Clients can only set the fields in the input DTOs (no approving their own
  content, moving reviews, etc.).
- Security headers on every response (CSP, `X-Frame-Options`, `nosniff`,
  referrer and permissions policies), HSTS + HTTPS redirect in production.
- Uploads are limited to images ≤10 MB and always stored with an image
  extension.

## Mobile app

See `src/MauiApp/PeePooFinder/README.md`. Release builds: `RELEASING.md`.
