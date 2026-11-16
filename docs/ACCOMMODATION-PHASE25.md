# Phase 2.5 — Experimental accommodation presentation

Implemented 2026-11-16 on `feature/website-redesign`, starting from a clean tree
at `7d2c858` (committed, owner-approved Phase 2.4 rollback baseline).
The owner approved the complete Phase 2.5 design, including the final simplified
nine-item grid, and authorized commit/push on 2026-11-16. Initial experiment and
refinement verification is recorded below; finalization is recorded in DEVLOG.
No branch switch/reset, PR change, real application startup, database access or
upload modification.

## Presentation and responsive behavior

Home and Rooms share a two-column grid at 768px and wider, with one column below
768px, 32px gaps, equal-height cards within each row and flexible content padding.
The existing 4:3 media ratio now produces substantially larger desktop photos than
the previous three-column grid. Covers remain above a white content surface;
there are no text overlays, new image assets or photo edits. Long titles/prices
wrap; descriptions retain the three-line preview and full text on Details.
Capacity and one 44px-minimum Details action remain at the bottom. Existing
forest-green/ivory tokens, 8px corners, lazy loading, alt text and missing-file
placeholders are retained. Shadows use the existing restrained card token.

Home no longer truncates published rooms with `Take(3)`: all published rooms are
queried in the existing price order. Four rooms therefore form a 2-by-2 desktop
grid without hardcoding a count or IDs. Additional rooms flow into further rows;
an odd last room retains a normal card width. Publication filtering, summary
calculations and per-field English fallback are unchanged. The preview fixture
now supplies four synthetic rooms, including missing covers and long content.

On `(hover: hover) and (pointer: fine)` with no reduced-motion preference, cards
lift 4px, shadows increase slightly, and images scale to 1.02 over 220ms. The
media container clips image overflow. Reduced motion disables card/image
transitions and excludes hover transforms; touch has no hover motion. Keyboard
focus keeps the explicit Details outline and green card border without lift.

## Verified facilities and missing information

The owner explicitly confirmed nine property facilities in the amenities
refinement request on 2026-11-16. This supersedes the initial two-facility brief.
No runtime descriptions were parsed or records inspected. Final titles:

| Bulgarian | English |
| --- | --- |
| Подово отопление | Underfloor heating |
| Климатици | Air conditioning |
| Wi-Fi | Wi-Fi |
| Телевизия | Television |
| Кухня и механа | Kitchen and traditional tavern |
| Паркинг | Guest parking |
| Сезонен басейн | Seasonal swimming pool |
| Барбекю зона с пещ | BBQ area with traditional oven |
| Тераси с гледка | Terraces with views |

Following owner desktop/mobile approval and the final simplification request,
all nine facilities use icons and titles only. The three secondary description
paragraphs, their unused SiteText entries and paragraph styling were removed,
not visually hidden. The pool heading retains its seasonal qualifier in BG/EN.

The compact ivory section stays immediately before the unchanged Your Stay.
Its nine icon-and-text items use three columns from 992px (three rows), two
from 576px to below 992px, and one below 576px. Icons remain 28px forest green,
with 32px grid gaps (24px on narrow mobile), no tile shadows or extra borders.
Small inline decorative SVGs follow the existing contact-panel technique; no
library was added. Icons are vertically centered beside titles; headings have
zero margin and no space is reserved for descriptions. Grid gaps are unchanged.

The owner additionally confirmed terraces with beautiful views for Apartment 1
and Apartment 2, not automatically for triple rooms. `Room` and RoomCardViewModel
still have no structured amenities or apartment-type field. Existing room cards
retain only their approved heating/individual-thermostat highlight and capacity;
none of the seven newly confirmed facilities is added to cards. Terrace badges
would require a structured terrace amenity flag or association maintained through
Admin and projected to cards. This is future separately scoped data work; no
names/slugs or hardcoded IDs are used to infer it.
No data fields/schema/migrations were added. Bathroom types and pet policy remain
unconfirmed. Pool months/dates/heating, facility specifications, usage conditions,
capacities and fees are unspecified and no promises about them are displayed.

## Room links and content audit

Home and Rooms projections copy `r.Slug` and `r.Id` directly from published rooms.
The shared card uses MVC `Rooms/Details` with `asp-route-slug="@Model.Slug"`.
The attribute route `/Rooms/{slug}` resolves only the matching published record;
unknown/unpublished slugs return 404. No fixed old room slugs were found in the
relevant public application source. Cover queries remain scoped to each room's
`GalleryImages`, preferring its cover then its saved order. Details keep gallery
order independent of cover selection; photo existence checks remain room-scoped.

The SQLite regression checks five published rooms plus one unpublished room,
a renamed slug and a cover that differs from the first gallery image, in BG and
EN. The refinement explicitly asserts count equality and resolves every card's
detail URL. Every published card remains present, links resolve to the same room ID,
cover associations survive, and the old/hidden URLs return 404. These are
isolated controller tests, not live-record or browser click-through validation.
Owner-edited names/slugs/photos were not overwritten or queried in SQL Server.

Old published URLs have no redirect history in the current implementation. If
previous URLs were shared/indexed, collect the exact previous-to-current mapping
and separately implement reviewed permanent redirects. No speculative redirects
were added. Existing query/cookie language selection and translation fallback
remain unchanged; the previously documented query/cookie switch conflict still
needs its own fix. Query-only language continuity on navigation is not newly
guaranteed by this presentation change.

## Verification and remaining visual review

The owner approved the Phase 2.5 room cards, typography, desktop/mobile layout
and expanded nine-item amenities grid, then approved the final simplified grid
and authorized finalization on 2026-11-16.
That approval is distinct from the detailed QA checklist below. Room-card markup,
styles and hover behavior were not changed during the refinement.

The refinement changes only homepage amenities markup, its CSS and `SiteText`,
`tests/GalleryUpload.Tests/Program.cs`, this note and `DEVLOG.md`. The files listed
at the end include the whole Phase 2.5 implementation. The room-card
individual-thermostat label is retained; all nine amenity titles remain localized.

- Solution and both separate harness builds passed with zero warnings/errors:
  `--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase25FinalVerification/`.
  Isolated output avoids interfering with the owner's already-running web app.
- Fresh DLL execution: Availability **14/14**, GalleryUpload **30/30**, including
  the check for all nine BG/EN titles, including the seasonal pool heading.
- All Node suites (availability, gallery, header): **20/20**. **64 checks total**.
- Final source diff, local documentation links and `git diff --check` passed.
- During the preceding expansion, the safe preview reported listening on
  `127.0.0.1:5192`, with a Data Protection
  key-ring access warning. The in-app browser timed out after startup during this
  refinement, as it did during the initial implementation. No screenshots or rendered
  assertions were obtained. The preview was stopped. Production startup and EF
  commands were never invoked.
- Browser checks were not repeated for the final description removal. Owner
  approval covers the preceding expanded desktop/mobile design; final title-only
  alignment/wrapping and the previously documented detailed QA remain unverified
  by the agent. No application or preview host was started in this final step.

Manual review is still required in **BG and EN at 320, 375, 390, 480, 768, 1024,
1200 and 1440 CSS pixels**: authentic cover crops, photo/content balance, wrapping,
overflow, one/two/four/five-room layouts, empty rooms, missing photos, long names
and descriptions, large test prices, keyboard focus/order and Details navigation,
hover/touch/reduced motion, 200% zoom/400% reflow, language switching, booking and
contact navigation, and offline calendar recovery. No browser, live SQL Server
concurrency, production price or deployment verification is claimed.

## Files changed

- `KeinishkataKushta/Controllers/HomeController.cs`: remove three-room limit.
- `KeinishkataKushta/Views/Shared/_RoomCard.cshtml`: verified heating highlight.
- `KeinishkataKushta/Views/Home/Index.cshtml`: nine-facility section before Your Stay.
- `KeinishkataKushta/Infrastructure/SiteText.cs`: eleven BG/EN translation entries.
- `KeinishkataKushta/wwwroot/css/site.css`: grid, surface, motion and amenities.
- `tests/Availability.Tests/PreviewHost.cs`: four-room synthetic fixture.
- `tests/GalleryUpload.Tests/Program.cs`: inventory/current slug and localized amenities regressions.
- `docs/ROOM-CARDS.md`, this note and `DEVLOG.md`: experiment and evidence.

Approved commit message:
`feat: enhance accommodation presentation and amenities`
