# Phase 3.1 — Room details experience

Implemented 2026-11-16 on `feature/website-redesign`, starting from clean HEAD
`0456290` (approved Phase 2.5). The initial implementation was left uncommitted.
Its lightbox is superseded by [Phase 3.2](GALLERY-LIGHTBOX-PHASE32.md), approved
for finalization with the integrated Room Details work on 2026-11-17.

## Audit and preserved contracts

Before this change, the detail page rendered name/full description, capacity and
price, first ordered photograph, every remaining photograph in an auto-fit grid,
then the availability/contact CTA band. At phone widths that grid could become
a long single column. Neither action appeared before the photographs.

`RoomsController.Details` filters published rooms by the existing slug and loads
only their associated photos, ordered by `SortOrder`, then descending ID. The
first ordered photo is the detail lead image. `IsCover` controls listing cards
independently and does not override this order. This contract is preserved;
controllers, IDs, slugs, model projections, translations/fallback, price values,
currency formatting, associations and files are unchanged.

Both room details and the property Gallery already use `site.js`. It provides
cyclic navigation, swipe, Escape, focus trapping/restoration, and reduced-motion
handling. Rooms previously advanced by clicking the image with no visible arrow
buttons; the property Gallery already has previous/next buttons.

The Contact destination is generic: room/date context is not passed or prefilled.
This limitation remains. No booking, contact submission, calendar, API, database,
entity or migration behavior was added or modified.

## Implemented presentation

- Name and both availability/enquiry links appear before photography. Capacity
  and existing price remain in a separate compact panel; the existing closing
  CTA remains after the description.
- Desktop uses a lead image capped at 520px, with 16:9 cropping and up to four
  additional previews in one four-column grid. At 320–767px the lead is 4:3 and
  the 112px-wide previews form a locally scrolling horizontal strip.
- A localized **View all photos (count)** opens the existing lightbox at the
  first photo. Every photo remains in its saved position, including those beyond
  the four visible previews. Hidden items have metadata but no image element,
  avoiding initial image requests and hidden keyboard stops. The opener is
  separate from gallery items, so it does not duplicate an item or inflate counts.
- The lead image uses high fetch priority and async decoding. The four previews
  use lazy loading/async decoding; the lightbox requests the selected original.
  There are no generated thumbnails, responsive derivatives or new dependencies;
  visible previews still request original uploads.
- The complete description appears once below the photography, with preserved
  line breaks and a readable line length. The owner-confirmed individually
  controlled underfloor heating is stated for every room. No room identity is
  inferred from a name/slug; apartment terrace information remains in existing
  owner-managed descriptions because there is no structured apartment identity.
- New labels use `SiteText` in BG/EN. Buttons wrap and have at least 44px targets.
  Existing alt/caption fallback, focus styling, keyboard and motion handling
  remain. Room-only previous/next buttons use existing JS handlers. Room-only
  lightbox overflow and caption wrapping accommodate controls and long content;
  property Gallery markup/styles remain unchanged.
- No-photo rooms show a localized message without empty gallery controls.
  Existing broken-file behavior is retained: an unavailable referenced upload
  can still display the browser's broken-image/alt presentation.

## Verification evidence

Solution and both standalone harnesses built successfully with zero warnings
and errors, using `--no-restore -m:1 -v:minimal` and the ignored output path
`-p:BaseOutputPath=bin/Phase31Verification/`. The freshly built DLLs passed:

| Checks | Result |
| --- | --- |
| Availability harness | 14/14 |
| Gallery/upload harness | 30/30 |
| Node availability, gallery and header interaction suites | 22/22 |
| Total automated checks | 66/66 |

Two new Node regressions cover the collection opener, complete nine-image
sequence including hidden previews, wrapping, opening a specific preview,
focus restoration to the collection opener, and single-image controls. Existing
property-gallery navigation and reduced-motion checks continue to pass. The
gallery harness verifies published filtering, ownership/order, unchanged slugs,
independent translation fallback and cover isolation using synthetic SQLite data.
These are not real SQL Server or deployment checks.

The existing isolated Razor preview host now supports `/Rooms/preview-room`
with `lang=bg|en`, `photos=0..30`, `long=1` and `missing=1`. It supplies an in-memory
model and generated SVG fixture graphics under room ID 0, intercepts the action
before controller execution, and neither reads uploads nor connects to a database.
The fixture price is synthetic and does not represent an owner price check.
Run from the solution folder after building the harness:

```powershell
dotnet .\tests\Availability.Tests\bin\Phase31Verification\Debug\net8.0\Availability.Tests.dll --preview --contentRoot "$PWD\KeinishkataKushta"
```

In-app browser evidence from this fixture:

- BG and EN at 320, 375, 390, 480, 768, 1024, 1200 and 1440 CSS pixels (900px
  viewport height): DOM layout measurements showed no horizontal page overflow,
  five visible photo triggers for nine photos, local strip overflow on phones,
  and lead height capped at 520px on desktop. This is a measured layout check at
  every width, not a screenshot review at every width.
- Inspected screenshots at 1440px EN, 320px BG room/lightbox and 320px EN strip,
  description and restored focus. Synthetic graphics validate geometry rather
  than authentic photographic crops.
- Both languages at 320/1440px with zero photos, one photo, nine photos plus long
  name/description, and missing files: no horizontal page overflow; correct zero,
  one or five rendered photo elements. Missing-file checks establish containment,
  not successful image rendering or a custom error fallback.
- BG collection opening and previous navigation wrapped to image 9; Escape
  closed the dialog and restored focus to the opener. EN single-photo opening
  disabled advance and hid previous/next. At 320px EN, Shift+Tab from Close and
  Tab from Next wrapped focus inside the dialog.

The first preview launch encountered sandbox data-protection access errors and
an incorrect relative content root; it was stopped. The approved retry used an
absolute content root and served the isolated fixture successfully. Preview was
stopped and the temporary browser tab/viewport override cleaned up afterwards.
The real application startup/migration/seeding path was never run.

## Remaining manual review and Phase 3.2

Owner approval should use current room slugs and authentic uploads, including
portrait/landscape crops and large-file transfer behavior. Physical touch/swipe,
full keyboard-only journey, assistive technology, actual 200% browser zoom and
400% reflow, reduced-motion rendering, phone landscape, and offline calendar
browser checks remain unverified. A 320px layout check does not establish actual
browser zoom behavior. Current runtime slugs were not enumerated from storage.

Homepage, Rooms listing and property Gallery source views are unchanged; their
full browser regression review was not performed in this increment. Existing
automated header/gallery/calendar tests passed. No form was submitted.

For Phase 3.2, assess the property lightbox's mobile three-column layout (side
controls reduce photo space), long-caption/short-viewport composition, missing
image feedback, background inertness and no-JavaScript full-image access.
These shared-component concerns are source-based review items, not newly
confirmed property-Gallery visual defects. The room lightbox itself still uses
a tall image stage and can scroll with long captions/stacked controls; review
that composition with real photos and short landscape screens before extending
changes to the property Gallery.

## Modified files

- `KeinishkataKushta/Views/Rooms/Details.cshtml`
- `KeinishkataKushta/wwwroot/css/site.css`
- `KeinishkataKushta/Infrastructure/SiteText.cs`
- `KeinishkataKushta/wwwroot/js/site.js`
- `tests/gallery-interaction.test.cjs`
- `tests/Availability.Tests/PreviewHost.cs`
- This note and [DEVLOG](../DEVLOG.md).

See [gallery ordering](GALLERY-ORDERING.md) and
[English content](ENGLISH-CONTENT.md) for the retained data contracts.
