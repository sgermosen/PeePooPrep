# Historial de cambios

Registro de lo que se hizo en el proyecto, por rondas, con cómo se verificó
y qué queda pendiente. El detalle técnico de cada cambio está en los mensajes
de commit; la referencia de la API en [`docs/API.md`](docs/API.md).

## Octubre 2026 — "Hacerlo totalmente funcional" (PR #14)

### Ronda 1 · Diagnóstico y primera corrección
Pregunta inicial: *¿el proyecto es funcional?* Se compiló, se ejecutaron los
tests y se levantó la API para probar cada flujo de punta a punta.

Estado encontrado: compilaba y los 12 tests pasaban, pero:

| Problema | Corrección | Commit |
|---|---|---|
| Subir cualquier foto (lugar, reseña) daba error 500 sin Cloudinary | Almacenamiento local en disco (`wwwroot/uploads`) cuando no hay Cloudinary | `126924f` |
| `[AllowAnonymous]` en toda la clase anulaba `[Authorize]` en `GET/DELETE /api/account` | Solo login y registro son anónimos | `126924f` |
| Paquetes con vulnerabilidades altas (SQLite, Cryptography.Xml) | Microsoft.* actualizado a 10.0.12 | `126924f` |
| Base de datos `reactivities.db` vieja en el repositorio | Eliminada | `126924f` |

### Ronda 2 · Corregir todo, seguridad, funciones nuevas, web, diseño y contenido
Pedido: *corregir absolutamente todo, extender funcionalidades, reforzar la
seguridad, nuevas funciones, simplificar el proceso, un diseño menos "IA",
generar contenido y páginas landing.*

#### Backend — `af26a19`
**Errores corregidos**
- *Mass assignment*: el cliente podía aprobar su propio lugar, fijar la
  verificación, cambiar fotos o mover reseñas a otro lugar. Ahora hay DTOs de
  entrada (`PlaceInput`, `VisitInput`) con solo los campos permitidos.
- Las respuestas exponían entidades de base de datos e IDs de usuarios. Ahora
  solo DTOs de salida.
- La nota de un lugar era la que escribió quien lo añadió → ahora es el
  promedio de las reseñas (con número de reseñas).
- Guardar en favoritos tu propio lugar lo marcaba como cerrado sin avisar →
  endpoint propio para abrir/cerrar, solo para el dueño.
- La foto de un lugar se convertía en el avatar de quien la subía.
- Los errores llegaban como texto plano y la app no podía mostrarlos → siempre
  `{ "message": "…" }`.
- El rol Admin solo se creaba en Desarrollo: la moderación no funcionaba en
  producción.
- Borrar lugares, reseñas o cuentas dejaba las fotos huérfanas → se borran.
- Los reportes no validaban que el contenido existiera.

**Seguridad**
- Cabeceras de seguridad y CSP estricta; HSTS y redirección HTTPS en producción.
- Los tokens incluyen el *security stamp*: cambiar la contraseña, borrar la
  cuenta o ser suspendido cierra todas las sesiones abiertas.
- La app no arranca sin `TokenKey` de 32+ caracteres.
- Límite de intentos por IP (login) y de escrituras por usuario (antes había
  un único límite global para toda la app). Configurables.
- Login sin distinguir mayúsculas en el correo, reglas de nombre de usuario,
  límites de longitud, validación de tipo y tamaño de imágenes.
- Eliminado un endpoint sin uso que permitía subir archivos arbitrarios.

**Funciones nuevas**
- Explorar sin cuenta; la cuenta solo se pide para aportar.
- Campos accesible, gratis, horario y dirección; búsqueda por texto; orden por
  distancia, nota o recientes.
- Favoritos, "mis lugares", "mis reseñas", una reseña por persona y lugar,
  cambiar contraseña, desbloquear usuarios.
- Contenido reportado por 3 personas distintas se oculta hasta revisión;
  los moderadores pueden restaurar, suspender usuarios y ver estadísticas.
- Datos de demostración realistas (lugares ficticios en Santo Domingo).
- Migración SQL Server `AddAmenitiesAndModeration` (normaliza tipos antiguos).

#### Web pública — `cc68e31`
Servida por la misma API (Razor Pages), en español:
- Inicio con cifras reales de la base de datos, mapa `/explorar` con
  OpenStreetMap y "cerca de mí", ficha pública de cada lugar `/lugar/{id}`
  (datos schema.org y tarjeta para redes sociales), `/negocios`, `/ayuda`.
- Contenido: 4 guías prácticas en `/guias`.
- Legales en español y en inglés (se conservan las URLs en inglés que usan las
  fichas de las tiendas).
- Nuevo panel de moderación `/admin`, página 404, `sitemap.xml`, `robots.txt`.

#### App móvil — `47d2757`
- Abre directamente en el mapa; el inicio de sesión solo aparece al aportar.
- Si la sesión caduca o se revoca, vuelve al inicio de sesión.
- No se pueden guardar lugares sin ubicación (antes se guardaban en 0,0).
- Todas las funciones nuevas: favoritos, mis lugares, mis reseñas,
  editar/borrar tu reseña, abrir/cerrar tu lugar, compartir, cambiar
  contraseña, personas bloqueadas, filtros.
- Android: permisos para abrir mapas, cámara y compartir; colores de marca.

#### Diseño
Un mismo lenguaje visual en web y app inspirado en la señalética de los baños:
carteles amarillos, bordes en tinta, pictogramas y números grandes. Sin
degradados ni tarjetas genéricas. Tipografías Bricolage Grotesque y Atkinson
Hyperlegible (licencia OFL), alojadas en el propio servidor.

#### Proceso simplificado — `d9b58fb`
- `docker compose up` levanta API y web con datos de prueba.
- La base de datos local se reconstruye sola si está desactualizada.
- El CI compila la app Android, revisa paquetes vulnerables y construye y
  arranca la imagen Docker.
- README reescrito y referencia de la API en `docs/API.md`.

### Cómo se verificó
- **74 tests automáticos** (antes 12): unitarios y de integración HTTP que
  cubren autenticación, sesiones, mass assignment, cabeceras de seguridad,
  páginas web, 404, sitemap y contenido oculto.
- Pruebas de punta a punta contra el servidor en ejecución (crear lugar con
  foto, reseña, favoritos, cambio de contraseña, panel de admin).
- Capturas de todas las páginas en claro, oscuro y móvil, sin errores de
  consola ni de CSP ni desbordes horizontales.
- **CI en GitHub**: compilación y tests del backend, compilación completa de la
  app Android con el SDK real y construcción de la imagen Docker.

### Pendiente
Cosas que no se hicieron o no se pudieron comprobar:

- **Probar la app en un teléfono o emulador.** Está compilada, pero nadie la
  ha usado en un dispositivo real.
- **iOS** no se compila en el CI (hace falta un runner macOS y certificados;
  ver `RELEASING.md`).
- **Idioma de la app:** la app está en inglés y la web en español. Traducirla
  es el siguiente paso natural.
- **Editar un lugar desde la app:** la API lo permite (`PUT /api/places/{id}`)
  pero la app solo deja abrir/cerrar o borrar. Hoy se edita por la API.
- **Avatares:** se quitó la foto de perfil porque estaba rota (usaba fotos de
  lugares). La app muestra la inicial del nombre.
- **Configuración de producción** (ver abajo).

### Lista para el deploy
1. `TokenKey`: valor aleatorio de 32+ caracteres (obligatorio).
2. `Database:Provider=SqlServer` y `ConnectionStrings:DefaultConnection`.
   La migración se aplica sola al arrancar.
3. Cloudinary (`Cloudinary:CloudName`, `ApiKey`, `ApiSecret`); sin él las
   fotos se pierden en cada despliegue.
4. `Site:BaseUrl` con el dominio público.
5. `Seed:AdminEmail` y `Seed:AdminPassword` para la cuenta de moderación.
6. Cuando las apps estén publicadas: `Site:PlayStoreUrl` y `Site:AppStoreUrl`
   (aparecen los botones de descarga en la web).
