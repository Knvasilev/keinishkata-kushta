# Phase 3.3 — Bulk multilingual caption management

Status: completed and approved by the project owner on 2026-11-17 for final
verification, one commit and push on `feature/website-redesign`. PR #1 remains
a draft; no next phase is authorized by this finalization.

## Owner-performed acceptance verification

The owner reported all five manual checks passed:

1. Editing and saving an individual English photo caption works.
2. Saved captions persist after refreshing the Admin page.
3. Applying captions to multiple selected photos works, including explicit
   replacement of existing captions.
4. The public Gallery displays the updated English captions.
5. Switching to Bulgarian preserves the existing Bulgarian captions.

These results are owner-performed verification, not automated browser or database
tests. No new browser or real-database session is run during finalization.

## Implementation

The property and room Admin galleries share a caption editor. All caption inputs
belong to one antiforgery-protected POST form using HTML form association, so
position/cover/delete/upload forms remain independent without nested forms.
Save buttons at the top and bottom submit the same batch. No public markup,
lightbox code, assets, entities, migrations or dependencies change.

Selection tools enhance the editor with JavaScript. Apply fills empty values in
one language; replacing nonempty values requires a checkbox that resets after
application or language change. Individual fields remain editable, and applying
never writes to the database. Blank shared captions are rejected to avoid bulk
clearing. Selection counts, unsaved/applied state and validation feedback use
text and live regions. Bulk save remains available without JavaScript.

`SaveCaptions` retains `AdminOnly`, POST and antiforgery protection. Its dedicated
model accepts IDs, originals and the two caption values only. Every ID must belong
to the requested property (`RoomId == null`) or room scope; duplicate, foreign,
missing and empty ID batches are rejected before writes. All captions have a
500-character server limit. Whitespace-only changes normalize to null and other
text is trimmed, matching the existing caption behavior.

Only fields different from the submitted original are updated. Scoped conditional
updates compare the stored original before writing each changed field. One
transaction makes the whole batch atomic; a stale edited field rolls everything
back, while unchanged fields preserve concurrent edits. Validation/conflict
responses redisplay the draft and summary; successful saves redirect with a
changed-photo count. This does not add concurrency protection to the older upload,
position, cover, delete or legacy individual-caption endpoints. SQL Server's
configured string collation still governs comparisons; this is not a rowversion
mechanism or proof of real SQL Server concurrent behavior.

Usage is documented in [English Content](ENGLISH-CONTENT.md).

## Verification

Results and browser limitations are recorded in [DEVLOG](../DEVLOG.md).
Finalization reran the solution and both standalone builds with zero warnings/errors
using `--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase33FinalVerification/`.
Fresh harness DLLs passed GalleryUpload **33/33** and Availability **14/14**;
four Node suites passed **31/31**, confirming **78 automated checks**. A stopped
concurrent harness build attempt and the successful sequential retries are recorded
in the log. Documentation links and whitespace checks passed. Finalization adds
owner acceptance/documentation only; the approved implementation/tests are unchanged.

Implementation-session verification follows:

Solution and both standalone builds passed with zero warnings/errors using
`--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase33Verification/`.
Fresh harness DLLs passed GalleryUpload **33/33** and Availability **14/14**;
four Node suites passed **31/31**, for **78 automated checks** in total.
New SQLite checks cover multi-photo bilingual saves in both scopes, public BG/EN
projection and missing-English fallback, unrelated-field preservation, selective
subsets, clearing, validation/draft retention, wrong-scope/duplicate IDs, stale
field rollback, and POST/antiforgery metadata. Existing upload, room, cover, order,
failure-cleanup and localization checks remain in the same harness.

New dependency-free Node checks cover selected-only application, preservation of
existing translations and the other language, explicit replacement/reset, selection
controls, invalid shared values, independent editing and unsaved navigation warnings.
The isolated Razor preview supports both Admin gallery GET views using generated
graphics and sample bilingual captions; preview POST actions are not enabled.
It never starts the real application or connects to its database.

Browser fixture measurements passed both editors with BG/EN query settings at
320, 375, 768, 1024 and 1440px, with no horizontal overflow and correct native
caption-form association. Keyboard application preserved nonempty English values
until replacement was checked, preserved Bulgarian and permitted independent edits.
A 500-character caption and both empty galleries were checked at 320px. These are
sample-data checks; automated login/HTTP save integration, real SQL concurrency, full
keyboard/screen-reader, zoom/reflow, reduced motion, missing authentic images,
cross-browser and offline calendar QA remain outstanding. No public visual or
lightbox behavior was modified. Preview startup/routing corrections and cleanup
are recorded in the log.
