# Phase 3.2 — Unified gallery and lightbox

Implemented 2026-11-16 on `feature/website-redesign`, HEAD `0456290`, preserving
the existing [Phase 3.1](ROOM-DETAILS-PHASE31.md) work. On 2026-11-17 the owner
approved the unified desktop/mobile layout, controls and swipe interactions and
authorized final verification, commit and push. The implementation includes the
preceding Room Details work on which the shared lightbox is integrated.

## Audit and changes

The two pages already shared `site.js`, but duplicated their lightbox markup.
Rooms used an interactive image button plus text controls; the property Gallery
used circular side arrows, including on phones. The room image click advanced
the collection, and the generic modal did not make the background inert.

Both pages now render `_GalleryLightbox.cshtml` with a title/count view model.
Native `dialog.showModal()` prevents background interaction. Close receives
initial focus; Tab/Shift+Tab wrap through visible controls and optional caption.
Escape, native cancel, Close and clicks on the surrounding dialog close it.
Closing clears the image source/gesture state and restores the actual opener.

One shared JS implementation keeps cyclic previous/next and keyboard arrows.
The image is no longer a button: image clicks/taps never advance or close it.
Horizontal swipes retain the existing distance/direction thresholds. Short,
vertical, cancelled, lost-capture and multi-touch gestures cannot complete a
navigation. Image animation was removed; the lightbox introduces no motion.

Each page marks its existing photo section as `data-gallery-collection`. Only
its triggers/openers are registered; hidden room photos remain in saved order,
and the collection opener does not duplicate an item. Each current page has one
collection/dialog. No multi-gallery page abstraction or new library was added.

Desktop uses 52px circular arrows beside the image, with counter/Close above.
Below 768px, the image spans the available width and 44px arrows sit below it.
Viewport height uses `dvh` with `vh` fallback and phone safe-area padding.
`object-fit: contain` preserves the entire image. Optional captions wrap in a
scrollable, keyboard-focusable area bounded to 20% of viewport height. Shared
controls retain BG/EN labels, gold focus outlines, image descriptions and a live
counter. Originals still load on demand; no image derivatives were introduced.

The Room Details layout/preview strip/description/CTAs and property Gallery grid
are retained. Homepage and Rooms listing files, controllers, queries, slugs,
prices, translation fallback, associations, database records and uploads were
not changed. The Phase 3.1 room-specific modal styles are superseded by this
shared component; the rest of that phase remains intact.

## Verification

Solution and both standalone harness builds passed with zero warnings/errors:
`--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase32Verification/`.
Executed the freshly built harness DLLs and all three Node interaction suites:

| Suite | Passed |
| --- | --- |
| Availability | 14/14 |
| Gallery/upload | 30/30 |
| Node gallery, availability and header | 27/27 |
| Total | 71/71 |

Gallery JS regressions cover collection isolation, hidden items, first/last wrap,
zero/one/many images, click/tap non-navigation, swipe thresholds/cancellation/
multi-touch, keyboard, focus, captions and closing/reopening cleanup. The .NET
gallery harness retains isolated SQLite checks for ordering, ownership, slugs,
published filtering, cover independence and BG/EN business-field fallback.

The isolated preview now supplies `/Gallery` and `/Rooms/preview-room` with
synthetic in-memory models and generated 1200x800, 600x800 and 1200x400 SVGs.
Query parameters: `lang=bg|en`, `photos=0..30`, `long=1`. Resource filters bypass
real controller execution; no database connection or production startup occurs.
After building, run from the solution folder:

```powershell
dotnet .\tests\Availability.Tests\bin\Phase32Verification\Debug\net8.0\Availability.Tests.dll --preview --contentRoot "$PWD\KeinishkataKushta"
```

In-app browser evidence:

- Both pages in BG/EN at 320, 375, 390, 480, 768, 1024, 1200 and 1440 CSS pixels,
  with 900px height: 32 modal layout cases. No dialog scrolling overflow; controls
  remained within the viewport with at least 44px targets, image fit was `contain`,
  and native `:modal` was active. A 375px viewport reported a 0.33px right-edge
  difference; follow-up confirmed no scrolling overflow and containment within
  a one-pixel rounding tolerance.
- Both pages/languages with 0, 1, 2 and 9 images: no dialog for zero photos,
  hidden arrows for one, correct first/last wrap for 2/9.
- Actual horizontal browser pointer drags advanced the property collection and
  advanced/reversed the room collection. Clicking the image retained its index
  and open state. These were pointer drags, not physical phone touch tests.
- EN room Tab wrapping, Escape, image-source cleanup and restoration to View all
  photos passed. BG property keyboard arrows/Escape restored thumbnail focus.
  The accessibility snapshot exposed the modal without the background page.
- Inspected desktop landscape, mobile portrait/room and 667x375 BG landscape
  screenshots with a long, visibly scrollable caption and panoramic image.
  Fixture border markers remained visible around complete images. Screenshot
  review was selective, not performed at every breakpoint.

Preview needed an approved run outside the sandbox after a data-protection access
failure. It was stopped afterwards; the temporary tab and viewport override were
cleaned up. An early preview command ran before its DLL existed and was retried
after the successful build. No real application/database startup occurred.
`git diff --check` and local documentation links passed.

## Remaining review

Authentic uploads, slow/broken images, physical touch/pinch on mobile Safari and
Android, screen-reader announcements, actual 200% zoom/400% reflow and very short
viewports remain manual QA. Native dialog and dynamic viewport behavior were
verified only in the available in-app browser; no compatibility polyfill was
introduced. No-JavaScript full-image access and custom broken-image recovery
remain outside this increment. Full homepage/listing and offline-calendar browser
regressions were not repeated; existing automated header/calendar checks passed.
No guest form was submitted.

## Files touched by Phase 3.2

- `KeinishkataKushta/Models/GalleryLightboxViewModel.cs`
- `KeinishkataKushta/Views/Shared/_GalleryLightbox.cshtml`
- `KeinishkataKushta/Views/Rooms/Details.cshtml`
- `KeinishkataKushta/Views/Gallery/Index.cshtml`
- `KeinishkataKushta/wwwroot/css/site.css`
- `KeinishkataKushta/wwwroot/js/site.js`
- `tests/gallery-interaction.test.cjs`
- `tests/Availability.Tests/PreviewHost.cs`
- This note and [DEVLOG](../DEVLOG.md).

The implementation phase made no commits, pushes, merges, branch resets/switches
or PR #1 changes. Final commit/push results are reported after execution; PR #1
must remain open as a draft. Owner approval does not establish completion of
every remaining manual QA item listed above.
