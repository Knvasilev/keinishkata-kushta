# Existing architecture

Audit date: 2026-11-12. Source baseline: `feature/website-redesign` at `b95618a`. This describes repository code, not a verified deployed environment or live database. No runtime configuration values or private data were inspected for this audit.

## Solution and dependencies

`KeinishkataKushta.sln` contains two .NET 8 projects with nullable reference types and implicit usings enabled:

| Component | Responsibility and dependencies |
| --- | --- |
| `KeinishkataKushta/KeinishkataKushta.Web.csproj` | ASP.NET Core MVC/Razor web host; references the Data project; EF Core, SQL Server provider, and EF Tools packages pinned to 8.0.11. |
| `KeinishkataKushta.Data/KeinishkataKushta.Data.csproj` | Entities, `AppDbContext`, migrations, shared date overlap rules; EF Core/SQL Server/Tools 8.0.11. |
| `tests/Availability.Tests/` | Separate executable .NET 8 Web SDK harness and isolated calendar preview; references Web; not in the solution. |
| `tests/GalleryUpload.Tests/` | Separate executable harness referencing Web and EF SQLite 8.0.11; not in the solution. |
| `tests/*.test.cjs` | Two dependency-free Node built-in test-runner files for calendar and gallery JavaScript. |

Bundled frontend file headers identify Bootstrap 5.1.0, jQuery 3.6.0, jQuery Validation 1.19.5, and Unobtrusive Validation 4.0.0. These are vendored under `wwwroot/lib`, not managed by an npm build pipeline. Public layout requests Playfair Display and Plus Jakarta Sans from Google Fonts with local fallback font stacks. Versions describe checked-in files, not a current vulnerability or support assessment.

## Request and startup flow

`Program.cs` registers MVC, scoped `AppDbContext` using SQL Server, cookie authentication, the role-based `AdminOnly` policy, `IPasswordHasher<AdminUser>`, and an IP-partitioned login limiter. Controllers directly query EF and build view models; there is no separate application service/repository layer or SPA/API project.

Before accepting requests, startup calls `Database.MigrateAsync()` then `AdminAccountSeeder.SeedAsync()`. The latter reads the optional `AdminSeed:Username` and `AdminSeed:Password` keys and creates the configured normalized username if absent. It is not limited to Development or to an empty admin table. No default account is embedded in this helper. Treat startup as a potentially database-writing operation.

Production middleware uses `/Home/Error` and HSTS, followed by HTTPS redirection, static files, routing, rate limiting, authentication, and authorization. Routes are `{area:exists}/{controller=Home}/{action=Index}/{id?}` then `{controller=Home}/{action=Index}/{id?}`. Room details additionally use the attribute route `/Rooms/{slug}`; setup uses `/Admin/Setup`.

Typical request: route -> controller -> EF query/projection -> view model -> Razor view/public or admin layout -> HTML. Calendar fetch requests return the `_Calendar` partial through the same public controller. Static uploads are served by the static-file middleware.

## Complete source structure map

| Directory/file group | Inventory |
| --- | --- |
| Public controllers | `Home`, `Rooms`, `Gallery`, `Places`, `Availability`, `Language` (each suffixed `Controller.cs`). |
| `Views/Home` | `Index`, `Contact`, `Privacy`. |
| `Views/Rooms`, `Gallery`, `Places` | Rooms `Index`/`Details`; Gallery `Index`; Places `Index`. |
| `Views/Availability` | `Index`, `_Calendar`. |
| `Views/Shared` | `_Layout`, `_RoomCard`, `_ValidationScriptsPartial`, `Error`, plus `_Layout.cshtml.css`. |
| Razor setup | Public and Admin each have `_ViewImports.cshtml` and `_ViewStart.cshtml`. |
| Public `Models` | `HomeIndexViewModel`, `RoomCardViewModel`, `RoomDetailsViewModel`/`RoomImageViewModel`, `PropertyGalleryViewModel`/`PropertyGalleryImageViewModel`, `NearbyPlaceViewModel`, `ContactFormViewModel`, `ErrorViewModel`, `AvailabilityViewModel` and `BlockedNights`/`CalendarDay`/`CalendarMonth` records. |
| Admin controllers | `Account`, `Home`, `Room` (singular), `Gallery`, `ContactMessages`, `Availability`. |
| Admin views | Account `Login`/`Setup`/`AccessDenied`; Home `Index`; Room `Index`/`Create`/`Edit`/`Delete`; Gallery `Index`/`Property`/`_ImageCaptions`/`_ImagePosition`; ContactMessages `Index`/`Details`; Availability `Index`/`Form`/`Delete`; Shared `_Layout`. |
| Admin `Models` | `AdminLoginViewModel`, `AdminSetupViewModel`, `RoomFormViewModel`, `RoomListItemViewModel`, `AvailabilityFormViewModel`, `GalleryUploadViewModel`, `GalleryIndexViewModel` and image list/position/caption models; `PropertyGalleryViewModels.cs` holds property index/upload models. |
| `Infrastructure` | `SiteText` (BG/EN dictionary and language selection), `SiteContact` (public contact/social/map constants), `CurrencyText` (display formatting), `AvailabilityCalendar` (Sofia-local date and calendar construction), `AdminAccountSeeder`. |
| Static CSS/JS | `site.css` and `site.js`; `availability.css` and `availability.js`; `admin.css`. |
| Assets | `images/branding/keinishkata-logo.png`, `hero-clean.png`, `hero-image.png`; `images/social/facebook.svg`, `instagram.svg`, `LICENSE.txt`; `favicon.ico`; licensed libraries under `wwwroot/lib`. |
| Runtime storage | Generated files under `wwwroot/uploads/rooms/{roomId}/{fileName}` and `wwwroot/uploads/property/{fileName}`; ignored by Git. No upload content or live database inventory was audited. |
| Configuration/tooling | `Program.cs`, safe `appsettings.example.json`, `Properties/launchSettings.json`, solution/project files, `.gitignore`, `.gitattributes`; local `.vs`/`.codex-build` are tooling/build artifacts. No checked-in CI workflow was found under `.github`. |
| Existing documentation | `README.md`, `AVAILABILITY.md`, `SOCIAL-ICONS.md`, `docs/GALLERY-ORDERING.md`, `docs/ENGLISH-CONTENT.md`; preserved unchanged by this audit. |

## Implemented public functionality

| Controller / principal URL | Current behavior |
| --- | --- |
| Home `/` | Branded hero, quick contact links, published-room count, maximum individual room capacity, minimum nightly room price; up to three published rooms ordered by price; introduction, stay highlights, nearby guide, enquiry links. Summary capacity is not total house capacity. |
| Rooms `/Rooms`, `/Rooms/{slug}` | Published rooms ordered by price, euro prices, capacity, descriptions, cover fallback; details return 404 for absent/unpublished rooms and show ordered photos plus availability/contact links. |
| Gallery `/Gallery` | House/grounds images (`RoomId == null`) in saved order with captions and lightbox. |
| Places `/Places` | Six hardcoded bilingual attractions: Covered Bridge, Varosha, Devetashka Cave, Krushuna Waterfalls, Kakrina Inn, Troyan Monastery; external information and map links. No admin place editor. |
| Home `/Home/Contact` | Phone/email/map links; required name, email and message, optional phone; DataAnnotations validation and antiforgery POST; saves `ContactMessage`, redirects with success feedback. No email dispatch. |
| Availability `/Availability?month=yyyy-MM` | Current month plus following 11 months allowed, two months rendered except final boundary; CSS shows one on phones; available/occupied/maintenance/past states; only dates and kind projected publicly. AJAX fragment navigation, no-store, native-link fallback. No date selection or booking submission. |
| Language `/Language/Set` | BG/EN one-year `kk_lang` cookie and local-only return redirect. `SiteText` gives a nonempty `lang` query priority over the cookie, then defaults to BG. Optional English room/caption fields fall back independently to originals. |
| Home `/Home/Privacy`, `/Home/Error` | Privacy is the scaffold placeholder; error view carries request ID and disables caching. Privacy is not linked in the public footer. |

`site.js` supports opening/closing gallery dialogs, cyclic arrow-key navigation, swipe gestures, single-photo handling, focus trapping/restoration, Escape, and reduced-motion-aware room-photo animation. Room details advance on photo click; property gallery has visible previous/next buttons. `availability.js` delegates navigation, guards concurrent clicks, uses fetch with a 15-second abort, restores focus and retains the old month on failure. No frontend framework is required.

## Implemented administrative functionality

All content controllers require `AdminOnly`; mutation endpoints use POST and antiforgery validation.

| Controller / URL prefix | Inventory |
| --- | --- |
| Account `/Admin/Account` | Login, logout POST, access-denied page; password hash verification/rehash; inactive-account check; five failed attempts trigger 15-minute lockout. Login/setup POST limit: ten requests per remote IP per minute. |
| Account `/Admin/Setup` | Initial account creation only in Development, from loopback, when no admin exists; username rules and password length/complexity/confirmation validation. |
| Home `/Admin` | Static dashboard confirming area routing; no operational metrics. |
| Room `/Admin/Room` | List, create, edit, publish/unpublish, delete with confirmation; bilingual name/description, capacity, euro price, editable unique slug. Slug normalization trims, lowercases and joins spaces; no general transliteration. |
| Gallery `/Admin/Gallery?roomId=...` and `/Admin/Gallery/Property` | Separate room/property lists; batch uploads (1-20 JPG/JPEG/PNG/WebP files, each 1 byte-5 MB, request/form limits 105 MiB); GUID filenames; shared bilingual batch captions; existing-caption editing; numeric positions; room cover selection; deletion. Failed batch copy/database save attempts clean up newly written files. |
| ContactMessages `/Admin/ContactMessages` | Newest-first list, details, mailto link, deletion with browser confirmation. `IsProcessed` badges exist, but there is no action to mark a message processed or reply within the app. |
| Availability `/Admin/Availability` | Upcoming/current entries or `history=true` showing all entries; create/edit/save/delete reservations or maintenance, private rooms-used/notes; exclusive departure validation, serializable overlap check, SQL deadlock/constraint feedback, row-version checks for stale edits/deletes. |

Authentication uses a custom `AdminUser` entity and ASP.NET password hasher, not the full Identity account-management UI. Cookie is HttpOnly, Secure Always, SameSite Strict, with 30-minute sliding expiry. No password-reset/change UI, account management UI, MFA, payment, guest account, booking engine, or platform synchronization was found.

## Data model and migration history

`AppDbContext` exposes five sets:

| Entity | Persistence and constraints |
| --- | --- |
| `Room` | Default/English name and description, unique required slug, capacity, `decimal(10,2)` euro price, publication flag, collection of images. |
| `GalleryImage` | Filename, optional bilingual captions, cover flag, integer `SortOrder`, optional room FK; null FK denotes property gallery. Filtered unique index allows at most one cover per non-null room. Ordering uses `SortOrder`, then descending ID. |
| `ContactMessage` | Required name/email/message, optional phone, UTC creation time, processed flag. |
| `AdminUser` | Unique normalized username, password hash, active flag, failed count, UTC lockout/creation/login times. |
| `AvailabilityBlock` | Arrival/departure `DateOnly`, Reservation/Maintenance enum, private notes and rooms-used text, SQL rowversion; date/kind check constraints and arrival/departure index. No FK to Room: every entry blocks the entire property. |

The optional GalleryImage -> Room FK has no cascade-delete action in the initial migration; room deletion does not explicitly delete photos or their files. Treat deletion with attached images as an integration-test requirement, not guaranteed cleanup.

Existing migrations, in order (timestamps are identifiers, not proof of implementation dates):

1. `20260120200347_Initial`: rooms, gallery images, contact messages and optional room FK.
2. `20260129180425_AddUniqueRoomSlug`: unique slug index.
3. `20260430194241_AddUniqueGalleryCoverPerRoom`: filtered unique cover index.
4. `20260803180000_AddAdminAuthenticationAndEuroPrices`: admin users and SQL conversion of existing prices to euros.
5. `20260918161952_AddPropertyAvailability`: blocks, checks, index and rowversion.
6. `20261010120000_AddGalleryImageOrdering`: sort column and per-gallery backfill preserving prior order.
7. `20261010130000_AddEnglishContent`: optional English room fields and image captions.

Designer metadata exists alongside migrations except the admin/euro migration, which declares context/migration attributes directly. `AppDbContextModelSnapshot.cs` records the latest model. Applied migration state was not inspected. Do not replay the price conversion manually.

## Operational boundaries and verification

Configuration uses `ConnectionStrings:DefaultConnection` and optional admin seed keys through ASP.NET configuration providers. See README for local setup; keep values in local secrets/environment configuration. Upload files require persistent storage and coordinated database/file backups; they are not contained in database rows or source control.

Source review and the checks recorded in [DEVLOG.md](DEVLOG.md) do not establish production readiness. Real SQL concurrency, login through middleware, storage permissions, deployment/proxy behavior, accessibility with assistive technology, and full-page visual results remain unverified. Risks and proposed work are in [DESIGN.md](DESIGN.md) and [ROADMAP.md](ROADMAP.md).
