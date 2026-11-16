# Phase 2.4 — Room cards and homepage review

The approved [Phase 2.5 implementation](ACCOMMODATION-PHASE25.md) supersedes the
card grid, motion and homepage inventory limit described below. This document
retains the approved Phase 2.4 baseline and its historical verification.

Implemented 2026-11-16 on `feature/website-redesign`, baseline `06695f5` with a
clean working tree. This is presentation work and a source/isolated-data audit.
The live SQL Server records, upload inventory and actual business prices were not
inspected. No stored content, files, prices, capacities, schema or migrations changed.

## Presentation

The shared homepage/list card uses three columns at 1200px and above, two from
768px, and one below 768px. Cards stretch within each grid row. Images have a
stable 4:3 container and cover cropping without zoom or dark overlays. Existing
serif titles, green/ivory tokens, subtle borders and modest hover/focus shadow
remain. Prices sit below titles, avoiding competition with long names. Long
unbroken text can wrap; footer items wrap and the CTA stays at the bottom with
a 44px minimum height and visible keyboard outline.

There is one bottom Details link per card; its accessible name includes the room
name. The decorative image is no longer a duplicate link. The existing three-line
description preview is retained; Details provides the full description at the
unchanged `/Rooms/{slug}` route. No stored description is shortened.

About now checks file availability and uses the selected room's name as alt text.
Reviewed statistics, About, benefits, local guide, final CTA and footer in source;
no further spacing/style change was justified without rendered evidence. Approved
Hero, header, contact panel, text, links and section order are preserved.

## Localization audit

`Room.Name`/`Description` are the original Bulgarian fields. `NameEn` and
`DescriptionEn` are optional. Home and Rooms controllers select English fields
only in EN and only when nonempty, independently for each field. BG uses the
originals. This deliberate fallback displays original content, **not a verified
translation**. English text incorrectly entered in an original field is still a
content error; no language detector, automatic translation or overwrite was added.

Create/Edit bind all four fields; edit GET restores both versions. English input
is trimmed and whitespace-only input becomes null. Name limits are 200 characters,
description limits 4000 in both form validation and EF mappings. Existing isolated
tests verify Cyrillic saving, BG/EN display across home/list/details, independent
fallback, cookie/query selection, validation and slug preservation. No binding or
save bug was found. Added visible server validation summaries and missing field
feedback to both room forms, plus a reminder explaining language ownership and
fallback. Client validation/middleware interaction has not been browser-tested.

## Image audit

Home/list query only published rooms and select that room's explicit cover first,
otherwise its first image by `SortOrder`, then descending ID. Details keep gallery
order independently of the cover. No property-gallery or other-room photograph is
substituted into a room card. Existing About reuse remains tied to a featured room.

The new `RoomPhoto.Exists` helper checks a nonempty associated filename in that
room's upload directory through the web-root file provider. Missing, empty and
directory entries produce localized “No photo available” in cards; malformed path
segments are rejected. If a selected cover file is missing, a placeholder appears
until the owner repairs the cover; another room is never substituted. About falls
back to its existing brand placeholder if no featured cover file is available.
Card photos retain localized room-name alt text, lazy loading and async decoding.
No image derivative pipeline, original-file changes or new assets were introduced.

Existence does not prove an image can decode or is authentic. A corrupt but
nonempty file, network failure after rendering, or unrelated test screenshot still
needs manual review. Room-detail gallery behavior remains unchanged; broken detail
gallery files also need admin repair. Upload validation currently checks extensions
and sizes; content/signature validation remains a separately scoped improvement.

## Pricing and content cleanup

Homepage count, minimum price and maximum individual room capacity are calculated
from all published rooms, and featured cards are the three cheapest published
rooms. The maximum is not total property capacity. New isolated tests verify that
an unpublished cheap/high-capacity room cannot affect these statistics, and that
an empty published set returns zeros and no cards. No calculation bug was found.

The owner reports these content tasks; specific record IDs/names remain unverified:

| Content issue | Manual action |
| --- | --- |
| Unrelated video-game screenshot | In Admin > Rooms > Gallery, find its thumbnail; record the owning room ID/name and photo filename before changing anything. Upload an authentic replacement and choose its cover. Keep the test upload until the owner explicitly decides to remove it. |
| Rooms without authentic photographs | Upload owned photographs to the correct room; leave the neutral fallback until then. |
| English text in Bulgarian fields | Enter approved Bulgarian content in the original fields; retain approved English in the English fields. |
| Empty English names/descriptions | Complete each independently and verify EN instead of assuming fallback is translated. |
| Test nightly prices | Confirm each room's EUR price before making the production site available to guests. Do not infer business prices from fixtures. |

The screenshot's owning room could not be identified from tracked source: uploads
and record associations are local runtime data. No database connection was opened
to guess that association.

## Owner's administrative checklist

1. Admin > Rooms > Edit: review each published room. Fill Bulgarian Name and
   Description, then the separate English fields. Keep the existing slug/URL.
2. Open that room's Gallery. Confirm room identity before uploading owned photos.
   Use Set cover for the card; use Position/Move for gallery order independently.
   Fill captions in both languages and save captions. No direct database edit needed.
3. Identify the unrelated screenshot as above. Replace its card cover first;
   deleting the old test upload is a separate owner decision.
4. Confirm Price per night (EUR) through Edit and save only approved prices.
   Check homepage starting price against the cheapest published room. Verify
   capacities without changing them merely to improve statistics.
5. Review BG and EN home, room list and details, including all descriptions,
   captions, crops and missing translations. Keep unfinished records unpublished
   if they should not be shown to guests.

## Verification and remaining QA

The owner approved the Phase 2.4 desktop and mobile design after manual visual
review on 2026-11-16 and authorized commit/push. Exact viewport/language and
interaction coverage was not supplied; that approval does not establish the
individual accessibility, responsive edge-case or interaction checks below.
The browser limitations recorded here describe the agent's implementation checks.

Solution and both harnesses built with `--no-restore -m:1 -v:minimal` and isolated
`-p:BaseOutputPath=bin/Phase24Verification/`: zero warnings/errors on final builds.
An initial preview build had a local-variable name collision; it was corrected.
Availability **14/14**, GalleryUpload **28/28**, all three Node suites **20/20**:
**62 checks passed**. The new photo test covers file absence/emptiness, invalid
paths and room isolation. Existing suites cover localization and cover/order.
These are model/controller/file checks and simulated DOM interactions, not
real-browser rendering or SQL Server concurrency checks.

The isolated preview now supplies synthetic bilingual home cards with long names,
varying/empty descriptions and absent/missing-file covers. Use the existing host:

```powershell
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj -- --preview --contentRoot "$PWD\KeinishkataKushta"
```

Open `http://127.0.0.1:5192/?lang=bg`, `/?lang=en`, or `/?empty&lang=en`.
Fixture Details destinations intentionally return 404; they are not real rooms.
The host intercepts these requests before controllers and never queries a database.

Attempted browser navigation returned `net::ERR_CONNECTION_TIMED_OUT`. A local
HTTP HTML check was blocked by socket permissions and also timed out after an
approved retry. The preview reported listening, with a Data Protection key-ring
access warning, and was stopped. No rendered HTML assertions or screenshots were
obtained; no breakpoint or interaction is claimed as passed.

Remaining manual QA: BG/EN at **320, 375, 390, 480, 768, 1024, 1440px**;
long names/descriptions, huge formatted test prices, empty rooms and missing photos;
authentic-photo crops; keyboard order/focus and Details click-through; 200% zoom
and 400% reflow; reduced motion; language switching; offline calendar recovery.
Real admin form errors and saved values need browser review on a deliberately
selected development database. Final diff/whitespace and local document links
were checked. No commit, push, branch switch or PR modification.

## Files changed

- `KeinishkataKushta/Views/Shared/_RoomCard.cshtml`: one Details action and photo fallback.
- `KeinishkataKushta/Views/Home/Index.cshtml`: About photo availability and alt text.
- `KeinishkataKushta/wwwroot/css/site.css`: card layout and interaction styling.
- `KeinishkataKushta/Infrastructure/RoomPhoto.cs`: file-provider availability helper.
- `KeinishkataKushta/Infrastructure/SiteText.cs`: BG/EN missing-photo label.
- `KeinishkataKushta/Areas/Admin/Views/Room/Create.cshtml` and `Edit.cshtml`: form feedback and language reminder.
- `tests/GalleryUpload.Tests/Program.cs`: two focused regression checks.
- `tests/Availability.Tests/PreviewHost.cs`: synthetic homepage fixture.
- `AVAILABILITY.md`, `DEVLOG.md`, `docs/ROOM-CARDS.md`: preview instructions, verified log and audit/checklist.

Suggested commit: `feat: polish room cards and document content readiness`
