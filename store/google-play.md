# Google Play — Store listing (ready to paste)

**Package name:** `com.peepoo.finder`
**Category:** Maps & Navigation _(alternative: Travel & Local)_
**Type:** App · Free · No ads
**Default language:** English (US)

---

## Main store listing

### App name _(max 30)_
```
PeePoo Finder
```

### Short description _(max 80)_
```
Find, rate and share the nearest clean restrooms — with map, photos & reviews.
```

### Full description _(max 4000)_
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

### Full description — Spanish (es-419) _(optional locale)_
```
¿Apuro? PeePoo Finder te ayuda a encontrar el baño limpio más cercano en segundos.

Explora los lugares cerca de ti en un mapa en vivo, ordenados por distancia, y mira qué esperar antes de ir: calificación, tipo, número de inodoros, cambiador de bebé, accesibilidad y si está abierto ahora.

POR QUÉ TE VA A ENCANTAR
• Lo más cercano primero — ordenado por tu distancia real
• Mapa en vivo con pines de cada lugar
• Reseñas y fotos honestas de la comunidad
• Filtros — cambiador de bebé, abierto ahora y más
• Verificación "¿sigue limpio?" para que las notas estén al día
• Cómo llegar con un toque
• Añade un lugar en segundos con tu ubicación y una foto

Gratis. Sin anuncios.
```

---

## Graphics

| Asset | Requirement | File |
|---|---|---|
| App icon | 512×512 PNG, 32-bit | `assets/appicon-512.png` |
| Feature graphic | 1024×500 PNG/JPG | `assets/feature-1024x500.png` |
| Phone screenshots (min 2) | 1080×1920+ | `assets/shot1-explore.png` … `shot5-login.png` |

Recommended screenshot order: Explore → Map → Detail → Add → Login.

---

## Content rating (IARC questionnaire)

| Question | Answer |
|---|---|
| Violence / sexual / language / drugs? | No |
| Users interact or share content? | **Yes** (reviews, photos, places) |
| Shares user's location with others? | No — only the location of a place the user chooses to publish |
| Digital purchases? | No |

**Expected result:** Everyone / PEGI 3.

---

## Data safety

Collected, encrypted in transit, and the user can request deletion.

| Data | Collected | Purpose | Shared |
|---|---|---|---|
| Email | Yes | Account / auth | No |
| Display name / username | Yes | Profile, review attribution | Visible in app |
| Approximate location | Yes (in use) | Nearby search, adding a place | No |
| Photos | Yes (optional) | Place & review photos | Visible in app |
| User content | Yes | Places & reviews | Visible in app |

---

## App content

- **Privacy policy URL:** `https://YOUR-API.com/privacy` _(served by the backend)_
- **Account/data deletion URL:** `https://YOUR-API.com/account-deletion` _(served by the backend)_
- **Terms of Use URL:** `https://YOUR-API.com/terms` _(served by the backend)_
- **Target audience:** 13+
- **Ads:** No
- **User-generated content:** Yes → in-app **report** (places/reviews), **block** users, and an **admin moderation** queue are implemented.

### What's new _(release notes)_
```
First release 🎉 Find nearby restrooms on a map, filter by amenities, read and write reviews, and get directions.
```

---

## Release checklist
- [ ] Upload signed `.aab` to Internal testing, verify on devices
- [ ] Promote to Production
- [ ] Privacy policy + account-deletion URLs point to your API domain
- [ ] Data safety form submitted
- [ ] Content rating completed
