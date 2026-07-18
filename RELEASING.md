# Releasing

## Continuous integration
`.github/workflows/ci.yml` builds and tests the backend on every push and PR.

## Release builds
`.github/workflows/release.yml` builds signed mobile binaries. It runs on a
`v*` tag push (e.g. `git tag v1.0.0 && git push --tags`) or manually via
**Actions → Release → Run workflow**. Artifacts (the `.aab` / `.ipa`) are
attached to the run.

### Required GitHub secrets (Settings → Secrets and variables → Actions)

Android:
| Secret | What it is |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | Your upload keystore, base64-encoded: `base64 -w0 peepoo.keystore` |
| `ANDROID_KEYSTORE_PASSWORD` | Keystore (store) password |
| `ANDROID_KEY_ALIAS` | Key alias (e.g. `peepoo`) |
| `ANDROID_KEY_PASSWORD` | Key password |
| `MAPS_API_KEY` | Restricted Google Maps Android key |

iOS (optional — set the repo **variable** `ENABLE_IOS` to `true` to enable the job):
| Secret | What it is |
|---|---|
| `APPLE_CERT_P12_BASE64` | Distribution certificate (.p12), base64-encoded |
| `APPLE_CERT_PASSWORD` | Certificate password |
| `APPLE_PROVISIONING_PROFILE_BASE64` | The `.mobileprovision` file, base64-encoded (installed on the runner) |
| `APPLE_SIGNING_IDENTITY` | Codesign identity name |
| `APPLE_PROVISIONING_PROFILE` | Provisioning profile name/UUID |

Before enabling iOS, re-add `net10.0-ios` to `<TargetFrameworks>` in
`src/MauiApp/PeePooFinder/PeePooFinder.csproj` (it is dropped on Linux).

Nothing sensitive lives in the repository — all keys come from secrets.
