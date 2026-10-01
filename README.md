# PeePoo Finder

A community app to find nearby restrooms, rate them and share reviews.

## Structure

- **`src/PeePoo`** — .NET 10 backend (Clean Architecture: API, Application,
  Domain, Infrastructure, Persistence) with ASP.NET Core Identity + JWT,
  MediatR, EF Core and an xUnit test suite. Runs on SQLite locally and
  SQL Server in production (`Database:Provider`).
- **`src/MauiApp/PeePooFinder`** — .NET 10 MAUI mobile client (replaces the
  retired Xamarin app).

## Running the API

```bash
cd src/PeePoo
dotnet run --project API          # http://localhost:5093, Swagger in Development
dotnet test Tests/Tests.csproj    # run the test suite
```

In Development the API uses SQLite and seeds demo data.

Test user: `starling@test.com` / `Pa$$w0rd`

### Photo storage

Photos (places, reviews) are uploaded to Cloudinary when the `Cloudinary`
section is configured. Without it, the API stores them on local disk under
`API/wwwroot/uploads` and serves them from `/uploads/...`, so photo uploads
work locally with no external accounts. Use Cloudinary in production, since
local files don't survive redeploys or scale across instances.

## Running the app

See `src/MauiApp/PeePooFinder/README.md` for build prerequisites and
configuration (API base URL and Google Maps key).

## Security note

Never commit real secrets. Configuration keys (connection strings, `TokenKey`,
Cloudinary/Blob credentials, Google Maps key) should come from user-secrets,
environment variables or a secrets vault — not from source control.
