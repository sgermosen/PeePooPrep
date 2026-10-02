# PeePoo API reference

Base URL: your deployment (e.g. `https://peepoo.azurewebsites.net`). All JSON uses camelCase.
Send `Authorization: Bearer <token>` for anything marked **auth**. Errors always look like
`{ "message": "…" }` (validation errors also include `errors` per field).

## Account
| Method | Path | Auth | Notes |
|---|---|---|---|
| POST | `/api/account/register` | – | `{ email, username, displayName, password }` → `{ token, username, displayName, isAdmin }` |
| POST | `/api/account/login` | – | `{ email, password }` → same as register. Email is case-insensitive. |
| GET | `/api/account` | auth | Current user + fresh token |
| POST | `/api/account/password` | auth | `{ currentPassword, newPassword }` → new token. Ends every other session. |
| DELETE | `/api/account` | auth | Deletes the account, reviews, photos, favorites, reports. |

Usernames: 3–24 letters, numbers, `.` or `_`. Passwords: 8+ chars with upper, lower and a digit.
Sign-in/sign-up are limited per IP (`RateLimits:AuthPerMinute`, default 10).

## Places
| Method | Path | Auth | Notes |
|---|---|---|---|
| GET | `/api/places` | – | Query: `lat`, `long`, `radiusKm`, `q`, `type`, `babyChanger`, `roomy`, `accessible`, `free`, `availableOnly`, `sort` (`distance`\|`rating`\|`recent`), `limit` (≤500) |
| GET | `/api/places/{id}` | – | Includes `averageRating`, `reviewCount`, `photos`, `isFavorite`, `isOwner` |
| GET | `/api/places/{id}/reviews` | – | Same as `/api/visits/visitsFromPlace/{id}` |
| GET | `/api/places/types` | – | `Unisex, Men, Women, Family, Accessible` |
| GET | `/api/places/saved` | auth | Places you saved |
| GET | `/api/places/mine` | auth | Places you added |
| POST | `/api/places` | auth | multipart form: `Name`, `Type`, `Lat`, `Long` required; optional `Id`, `Description`, `Observations`, `Address`, `OpeningHours`, `Toilets`, `Urinals`, `Rating` (0–5), `IsAvailable`, `HaveBabyChanger`, `IsRoomy`, `IsAccessible`, `IsFree`, `File` (image ≤10 MB) |
| PUT | `/api/places/{id}` | owner | JSON with the same fields (no file) |
| DELETE | `/api/places/{id}` | owner | Also removes photos and reviews |
| POST | `/api/places/{id}/favorite` | auth | Toggles; → `{ isFavorite }` |
| POST | `/api/places/{id}/availability` | owner | `{ isAvailable }` |
| POST | `/api/places/{id}/verify` | auth | "Still like this" — stamps `lastVerifiedAt` |
| POST | `/api/places/{id}/report` | auth | `{ reason }` |

`rating` is the rounded average of reviews (the submitter's own rating only while there are none).

## Reviews (visits)
| Method | Path | Auth | Notes |
|---|---|---|---|
| GET | `/api/visits/visitsFromPlace/{placeId}` | – | Newest first; hides hidden reviews and people you blocked |
| GET | `/api/visits/{id}` | – | |
| GET | `/api/visits/mine` | auth | |
| POST | `/api/visits` | auth | multipart: `PlaceId`, `Title`, `Description`, `Rating` (1–5), optional `Id`, `File`. One review per person per place. |
| PUT | `/api/visits/{id}` | author | `{ title, description, rating }` |
| DELETE | `/api/visits/{id}` | author | |
| POST | `/api/visits/{id}/report` | auth | `{ reason }` |

## Profiles, photos, blocking
| Method | Path | Auth | Notes |
|---|---|---|---|
| GET | `/api/profiles/{username}` | – | `{ displayName, bio, placesCount, reviewsCount, joinedAt }` |
| PUT | `/api/profiles` | auth | `{ displayName, bio }` |
| GET | `/api/profiles/blocked` | auth | Usernames you blocked |
| POST / DELETE | `/api/profiles/{username}/block` | auth | Block / unblock |
| POST | `/api/photos` | auth | multipart `PlaceId`, `File` — extra photo for a place (max 20) |
| DELETE | `/api/photos/{id}` | uploader, place owner or admin | |

## Moderation (Admin role)
| Method | Path | Notes |
|---|---|---|
| GET | `/api/admin/stats` | Counts of places, reviews, users, open reports |
| GET | `/api/admin/reports` | Open reports with a preview of the reported content |
| POST | `/api/admin/reports/{id}/resolve?restore=true` | Closes all open reports on that item; `restore` makes hidden content visible |
| DELETE | `/api/admin/places/{id}` · `/api/admin/visits/{id}` | Take content down |
| POST | `/api/admin/users/{username}/ban` | Locks the account, ends its sessions, hides its reviews |

Content reported by 3 different people is hidden automatically until reviewed.
The web console at `/admin` uses these endpoints.

## Other
`GET /health` (database check), `GET /sitemap.xml`, `GET /robots.txt`.
Writes are rate-limited per user (`RateLimits:WritesPerMinute`, default 60).
