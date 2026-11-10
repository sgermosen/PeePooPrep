# Ideas y pendientes

Lista viva de lo que falta, mejoras recomendadas e ideas para PeePoo Finder.
Cuando hagas algo de aquí, bórralo de esta lista y anótalo en
[`CHANGELOG.md`](../CHANGELOG.md) con cómo lo verificaste.

**Prioridad:** 🔴 antes de lanzar · 🟠 pronto · 🟢 cuando haya tiempo
**Esfuerzo:** S (horas) · M (1–3 días) · L (una semana o más)

Cada punto dice por qué importa y dónde tocar el código.

---

## 1. Pendientes antes de lanzar

| | Pendiente | Esfuerzo | Detalle |
|---|---|---|---|
| 🔴 | **Configurar y desplegar producción** | S | `TokenKey` (32+ caracteres, obligatorio), `Database:Provider=SqlServer` + cadena de conexión, Cloudinary, `Site:BaseUrl`, `Seed:AdminEmail`/`Seed:AdminPassword`. Lista paso a paso en `CHANGELOG.md` → "Lista para el deploy". La migración se aplica sola al arrancar. |
| 🔴 | **Probar la app en un teléfono o emulador real** | S | Compila en CI con el Android SDK real, pero nadie la ha usado en un dispositivo. Recorrer: explorar sin cuenta, registrarse, añadir lugar con foto, reseñar, guardar, cambiar contraseña, borrar cuenta. |
| 🔴 | **Recuperar contraseña ("olvidé mi contraseña")** | M | No existe. Quien olvida su contraseña pierde la cuenta. Hace falta un servicio de correo (SMTP o SendGrid), `GeneratePasswordResetTokenAsync` / `ResetPasswordAsync` de Identity en `AccountController`, una página en la web para el enlace del correo y la pantalla en la app. |
| 🔴 | **Quitar metadatos (EXIF) de las fotos** | S | Las fotos se guardan tal cual, y una foto de móvil suele llevar la ubicación GPS y el modelo del teléfono de quien la tomó. Es un riesgo de privacidad. Limpiarlas al subirlas (por ejemplo con ImageSharp) en `API/Services/LocalPhotoAccessor.cs` e `Infrastructure/Photos/PhotoAccessor.cs`, y de paso reducirlas a un tamaño razonable. |
| 🔴 | **Actualizar las fichas de las tiendas** | S | Las capturas de `store/assets` son de julio y muestran el diseño anterior; los textos de `store/*.md` no mencionan que se puede explorar sin cuenta ni las funciones nuevas. Revisar también el formulario de "Seguridad de los datos" de Google Play (ubicación, fotos, cuenta). |
| 🟠 | **Compilar iOS en CI** | M | El proyecto ya incluye `net10.0-ios` cuando se compila en macOS, pero el CI solo compila Android. Hace falta un runner macOS y los certificados de Apple (ver `RELEASING.md`). |
| 🟠 | **Botones de descarga en la web** | S | Cuando las apps estén publicadas, rellenar `Site:PlayStoreUrl` y `Site:AppStoreUrl`; hasta entonces la web muestra "Muy pronto". |

## 2. Mejoras recomendadas (técnicas)

### Seguridad y abuso
| | Mejora | Esfuerzo | Detalle |
|---|---|---|---|
| 🟠 | Confirmación de correo | M | Hoy cualquiera se registra con un correo que no es suyo. Usa el mismo servicio de correo que la recuperación de contraseña. |
| 🟠 | Límite a "¿Sigue igual?" | S | `Application/Places/Verify.cs` no limita cuántas veces una persona confirma un lugar, así que se puede inflar la frescura. Guardar quién confirmó y permitir una vez por persona y día. |
| 🟠 | Fotos reportables | S | Se pueden reportar lugares y reseñas, pero no una foto suelta. Añadir `TargetType = "Photo"` en `Moderation/CreateReport.cs` y en el panel `/admin`. |
| 🟢 | Tokens más cortos con renovación | M | Hoy el token dura 14 días (`Jwt:LifetimeDays`). Con *refresh tokens* se puede bajar a minutos sin obligar a iniciar sesión otra vez. |
| 🟢 | Registro de acciones de moderación | S | Guardar quién borró, restauró o suspendió qué y cuándo, para poder revisarlo después. |
| 🟢 | Umbral de ocultado configurable | S | Hoy son 3 reportes fijos (`CreateReport.AutoHideThreshold`); pasarlo a configuración. |
| 🟢 | Quitar el ajuste de `Pkcs` | S | `Persistence.csproj` fija `System.Security.Cryptography.Pkcs` 10.0.14 porque `Microsoft.Data.SqlClient` todavía trae la 9.0.11, que es vulnerable. Cuando una versión nueva de EF Core / SqlClient ya traiga la corregida, borrar esa línea (el CI avisará si vuelve a hacer falta). |
| 🟢 | Escaneo de seguridad en CI | S | Dependabot para NuGet y GitHub Actions; un escaneo OWASP ZAP contra la imagen Docker del CI; `report-uri` en la CSP para enterarse de bloqueos. |

### Rendimiento y escala
| | Mejora | Esfuerzo | Detalle |
|---|---|---|---|
| 🟠 | Proveedor de mapas para producción | S | La web usa los servidores públicos de teselas de OpenStreetMap, cuya política no permite tráfico alto de una app. Con usuarios reales, cambiar a un proveedor con cuota (MapTiler, Stadia, Thunderforest); es una línea en `wwwroot/js/explore.js` y `place.js`. |
| 🟢 | Paginación | M | Las reseñas se cortan en 200 (`Visits/List.cs`) y los lugares en 500 por consulta. Añadir paginación por cursor a la API y "cargar más" en web y app. |
| 🟢 | Búsqueda geográfica nativa | M | La búsqueda por cercanía filtra por rectángulo y calcula la distancia en memoria. Con muchos lugares, usar `geography` de SQL Server con índice espacial (NetTopologySuite en EF Core). |
| 🟢 | Fotos en miniatura | S | Servir miniaturas en las listas (Cloudinary las genera por URL) en vez de la imagen completa. |
| 🟢 | Caché y compresión | S | Compresión de respuestas y caché corta para `GET /api/places` y las páginas públicas. |

### Calidad y operación
| | Mejora | Esfuerzo | Detalle |
|---|---|---|---|
| 🟠 | Pruebas de la web en el navegador en CI | M | Hoy se verificaron a mano con Playwright (capturas, errores de consola y de CSP). Automatizarlo en CI con la imagen Docker que ya se construye allí. |
| 🟢 | Tests de la app | M | Proyecto de tests para los ViewModels de la app (hoy solo se prueba el backend). |
| 🟢 | Monitorización | S | Application Insights o logs estructurados, y alertas sobre `/health`. |
| 🟢 | Limpieza del repositorio | S | `SplashScreen.json`, `SplashScreen2.json`, `map.png` y `LogoTransparent.png` en la raíz no se usan en ningún sitio; moverlos a `store/` o borrarlos. |

## 3. Ideas de producto

### App
| | Idea | Esfuerzo | Por qué |
|---|---|---|---|
| 🟠 | **App en español** | M | La web está en español y la app en inglés. Usar archivos `.resx` para tener los dos idiomas según el del teléfono. |
| 🟠 | **Editar un lugar desde la app** | S | La API ya lo permite (`PUT /api/places/{id}`), pero la app solo deja abrir/cerrar o borrar. Reutilizar la pantalla de "Añadir lugar". |
| 🟠 | **Foto de perfil** | M | Se quitó porque estaba rota (usaba fotos de lugares). Añadir un campo propio de avatar al usuario; hoy la app muestra la inicial. |
| 🟢 | Enlaces que abren la app | M | Que `/lugar/{id}` abra la app si está instalada (Android App Links / iOS Universal Links). Hoy compartir un lugar lleva a la web. |
| 🟢 | Modo sin conexión | M | Guardar en el teléfono los lugares guardados y los últimos vistos, para consultarlos sin señal. |
| 🟢 | Avisar de duplicados | S | Al añadir un lugar, avisar si ya hay uno con nombre parecido a menos de 30 m. |

### Datos de cada lugar
| | Idea | Esfuerzo | Por qué |
|---|---|---|---|
| 🟠 | **Horario estructurado** | M | Hoy el horario es texto libre y "abierto/cerrado" lo marca el dueño. Con horario por día se podría mostrar "abierto ahora" automáticamente y filtrar por eso. |
| 🟢 | Notas por aspecto | M | Además de la nota general: limpieza, papel y jabón, espera, accesibilidad. Responde mejor a "¿cómo está?". |
| 🟢 | Más detalles | S | Baño sin género, papel disponible, agua caliente, iluminación, barras de apoyo. |
| 🟢 | "Estaba cerrado" de la comunidad | S | Que cualquiera pueda avisar de que lo encontró cerrado (con hora), sin depender del dueño. |

### Negocios
| | Idea | Esfuerzo | Por qué |
|---|---|---|---|
| 🟢 | Reclamar una ficha | M | Hoy un negocio que quiere administrar su ficha tiene que escribir por correo (página `/negocios`). Un flujo de verificación dentro de la app le daría control directo. |
| 🟢 | Distintivo para negocios | S | Un cartel o pegatina con código QR que lleve a su ficha. Se apoya en la página `/negocios`. |

### Web y contenido
| | Idea | Esfuerzo | Por qué |
|---|---|---|---|
| 🟠 | Más guías | S | Hay 4 en `/guias`. Temas posibles: viajar en guagua o carretera, baños en playas, eventos y conciertos, baños para personas con enfermedades intestinales (por ejemplo, Crohn). Cada guía es un archivo en `Pages/Guias/Articulos` y una línea en `Site/Guides.cs`. |
| 🟢 | Páginas por ciudad o zona | M | Por ejemplo `/ciudad/santo-domingo` y `/zona/zona-colonial`, con los lugares de esa zona. Ayudan a que la web aparezca en buscadores. |
| 🟢 | Web en inglés | M | Para turistas; las páginas legales ya existen en inglés. |
| 🟢 | Web instalable (PWA) | S | Que el mapa web se pueda instalar en el teléfono y abrir sin conexión los lugares ya vistos. |
| 🟢 | Revisión de accesibilidad | S | Probar web y app con lector de pantalla (TalkBack, VoiceOver) y con la guía WCAG. Encaja con la misión del producto. |
| 🟢 | Estadísticas respetuosas con la privacidad | S | Algo como Plausible, sin cookies, para saber qué páginas y guías se usan. |
