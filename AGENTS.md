# Project development instructions

## Scope and safety

- This repository is an ASP.NET Core MVC guest-house website. Work in the existing solution; do not create duplicate project directories or archives.
- Verify the working directory, current branch, and `git status --short` before editing. The audit baseline on 2026-11-12 was `feature/website-redesign`, commit `b95618a`, with a clean working tree. Do not assume future sessions have the same state.
- Do not automatically commit, push, switch branches, or discard user changes.
- Preserve existing documentation and asset licenses. Do not modify existing photographs or branding images without an explicit request.
- Documentation and visual work must not change database entities, schema, or migrations. Any future data change needs its own explicit scope and verification.
- Never print, copy into documentation, or commit credentials, connection strings, local settings, password hashes, or guest enquiry data. Use configuration key names only. Local settings and uploads are intentionally ignored by Git.
- Do not launch the real web application or run EF database commands during a read-only audit: `Program.cs` applies migrations and invokes admin seeding before serving requests. Use only an intentionally selected development database for future runtime work.

## Read first

Read [README.md](README.md), [ARCHITECTURE.md](ARCHITECTURE.md), [DESIGN.md](DESIGN.md), [ROADMAP.md](ROADMAP.md), and [DEVLOG.md](DEVLOG.md). Feature-specific sources remain authoritative for usage details:

- [AVAILABILITY.md](AVAILABILITY.md): whole-property policy, calendar behavior, manual checks, isolated preview.
- [docs/GALLERY-ORDERING.md](docs/GALLERY-ORDERING.md): photo order versus room-card cover.
- [docs/ENGLISH-CONTENT.md](docs/ENGLISH-CONTENT.md): editable translations and fallback behavior.
- [SOCIAL-ICONS.md](SOCIAL-ICONS.md): icon attribution and license retention.

## Code ownership and conventions

- `KeinishkataKushta/`: web startup, public controllers, view models, Razor views, infrastructure helpers, and static assets.
- `KeinishkataKushta/Areas/Admin/`: authenticated content management. Keep the `AdminOnly` policy, antiforgery validation, and POST-only mutations.
- `KeinishkataKushta.Data/`: entities, EF mappings, availability overlap expression, and migration history.
- Use existing MVC/Razor patterns and view models. Keep public queries restricted to published rooms and public availability projections free of private notes and room-use fields.
- Preserve published room URLs (`/Rooms/{slug}`); do not regenerate slugs as a side effect of translating or restyling content.
- Preserve BG/EN behavior: `SiteText` UI text, query/cookie language selection, and independent fallback for optional English business fields.
- Preserve whole-property exclusivity, arrival-inclusive/departure-exclusive date rules, Sofia-local calendar dates, overlap transactions, and row-version concurrency.
- Gallery order and room-card cover are independent. Uploads live under `wwwroot/uploads`, outside Git; keep database and file handling consistent.
- Public styling lives in `wwwroot/css/site.css`; calendar styling in `availability.css`; admin styling in `admin.css`. `_Layout.cshtml.css` currently contains only a maintenance comment.
- Keep calendar native-link fallback, loading/error recovery, gallery keyboard/focus handling, non-colour status labels, reduced-motion support, and existing licensed assets.

## Verification

Run commands from the solution directory. A build does not start `Program.Main`; running the real web project does.

```powershell
dotnet build .\KeinishkataKushta.sln --no-restore
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj --no-restore
dotnet run --project .\tests\GalleryUpload.Tests\GalleryUpload.Tests.csproj --no-restore
node --test .\tests\availability-navigation.test.cjs .\tests\gallery-interaction.test.cjs
```

`--no-restore` requires existing restored packages. Follow README setup if dependencies are absent; do not silently update dependency versions. The two .NET harnesses are outside the solution and must be invoked separately. Availability checks inspect SQL translation/model metadata without connecting to SQL Server; gallery checks use isolated SQLite and temporary sample files. Neither proves real SQL Server concurrency or deployment behavior.

Audit environment note: the default-worker solution build returned an unexplained failure; `dotnet build .\KeinishkataKushta.sln --no-restore -m:1 -v:minimal` succeeded with zero warnings/errors. Both separately built harnesses and all checks passed. Use this recorded fallback for diagnosis without assuming the underlying cause is known.

For future visual changes, verify both languages at 320, 375, 768, 1024, and 1440 CSS pixels; keyboard-only interaction; zoom/reflow; reduced motion; long content; empty/missing-photo states; and calendar requests failing offline. Record actual evidence and limitations. Do not claim browser or database checks that were not performed.

For documentation-only work, validate links, factual source references, and the final diff; do not add tests that merely restate prose. Update DEVLOG with the actual scope and verification, separating completed work from recommendations.
