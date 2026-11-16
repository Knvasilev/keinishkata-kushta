# Safe development roadmap

Prepared 2026-11-12 from baseline `b95618a`. Every task below is proposed and unchecked. The audit made documentation changes only. P0 means a pre-release safety prerequisite; P1 means a high-value correctness/usability improvement; P2 means a later enhancement. No estimates or implementation promises are implied.

## Phase 0 — Establish a reproducible baseline (P0)

- [ ] Establish fresh-checkout build/test reproducibility and investigate the default-worker solution-build failure recorded in DEVLOG if it recurs. Single-worker solution build and all 54 checks passed during this audit. Add a repeatable CI build/test workflow after confirming package restore and platform needs. Acceptance: clean build and recorded standalone harness/Node results from a fresh checkout; do not rely on `dotnet test` to discover executable harnesses outside the solution.
- [ ] Identify a disposable development database and upload storage, inspect intended startup migration effects, and document deployment migration/backup/restore ownership. Existing startup automatically migrates and optionally seeds an admin; do not launch against unknown storage. Acceptance: documented, tested recovery of matching database and uploads without exposing configuration values.
- [ ] Capture a BG/EN route/state inventory and desktop/mobile screenshots using disposable content. The existing preview covers only Availability. Acceptance: representative home, rooms, details, property gallery, places, contact, calendar and admin states are reviewable before layout changes.
- [ ] Replace placeholder privacy content through an owner-reviewed content task and plan visible privacy navigation. Acceptance: accurate bilingual notice describing actual stored enquiry data and owner decisions on retention/access; no invented policy commitments.

## Phase 1 — Correctness and operational risks (P1)

These changes should be separate from a visual redesign, with focused regression checks.

| Task | Source evidence | Acceptance |
| --- | --- | --- |
| Define safe room/photo deletion | Room deletion does not load/delete associated images or remove files; optional FK has no cascade in initial migration. Gallery delete removes files before saving DB changes. | Explicit behavior for rooms with images; recoverable consistency under DB/file failures; no accidental loss of originals. |
| Harden upload validation/storage | `GetUploadError` checks extension/count/length; no file signature/decode check; static files are public. | Validate actual supported image content, reject malformed input, define durable storage and limits; preserve existing batch rollback guarantees. |
| Protect concurrent gallery edits | Positions rewrite a whole gallery without rowversion; cover replacement spans separate updates. | Reproduce simultaneous reorder/upload/cover/delete operations; enforce a defined result without conflicting covers/order or unhandled errors. |
| Review authentication lifecycle/deployment | Cookie claims created at login; no revalidation callback, password recovery UI or proxy setup in Program. | Verify deactivation/session expiry, simultaneous failures, HTTPS/proxy IP handling and data-protection persistence in the intended deployment. Scope account features separately. |
| Add contact abuse and retention controls | Anonymous Contact has antiforgery and field validation but no endpoint rate limit/anti-spam mechanism; list loads all messages. | Owner-approved protection, retention and pagination decisions; normal enquiries still work. Do not add email delivery without explicit scope. |
| Verify real SQL availability behavior | Serializable transactions and rowversion exist; isolated tests do not prove SQL Server runtime behavior. | Overlap races, deadlock retry feedback, stale edit/delete, boundaries and private-field exclusion verified against disposable SQL Server. |
| Fix language query/cookie conflicts | Query language wins while switch return URL retains query. | BG/EN switch works from both language query variants, calendar navigation and ordinary pages; preserve local redirect protection. |

Dependency/library updates need a separate version and vulnerability review using then-current primary sources. This audit records installed versions only; it does not assert a known vulnerability or prescribe an unverified upgrade.

## Phase 2 — Accessible usability fixes (P1)

- [ ] Fix admin room table cell/header order, image alt text and narrow-screen wrapper.
- [ ] Correct admin validation script dependencies and render room-form validation feedback, including duplicate-slug errors.
- [ ] Make public navigation state, skip link, heading hierarchy and control labels accessible in both languages; choose consistent admin document language/navigation behavior.
- [ ] Review gallery visible navigation, announcements, background interaction and no-JS fallback while preserving swipe, focus and reduced-motion behavior.
- [ ] Measure contrast, test focus and zoom/reflow, localize errors and improve contact input purpose/status semantics.

Acceptance: route-specific browser and keyboard evidence for both languages, with existing JavaScript regressions passing. These targeted fixes can precede broader visual changes without schema modifications.

## Phase 3 — Agree redesign requirements (P1, depends on baseline/content)

- [ ] Agree audience, enquiry journey and whole-house/room pricing explanation with the owner.
- [ ] Approve bilingual copy, amenities/rules/accessibility facts and existing-asset selection.
- [ ] Approve representative desktop/mobile layouts and token/component specification from DESIGN.
- [ ] Decide any behavior additions separately: contextual room/date enquiries, processed-message workflow, richer amenities, booking or email integrations. None is currently implemented by this audit.

Acceptance: reviewable design and content specification, route compatibility list, clear boundary between styling and behavior/data work. No SPA, authentication or database rewrite is assumed.

## Phase 4 — Incremental redesign implementation (P2, requires separate authorization)

1. Shared public layout/navigation/footer and tokens.
2. Homepage and room cards/list/details.
3. Property gallery and lightbox consistency.
4. Contact and nearby places.
5. Calendar visual integration, preserving date policy and AJAX/native fallbacks.
6. Admin forms, tables and navigation.

Each increment should have a small diff, BG/EN screenshot comparison, relevant behavior checks and rollback plan. Keep existing slugs, controller contracts, publication filtering, photo order/cover independence and private-data boundaries. Existing images and migrations remain unchanged unless a later task explicitly authorizes otherwise.

## Phase 5 — Performance, maintainability and release (P2)

- [ ] Measure actual image/font loading and layout shifts; separately scope responsive image derivatives and optional font self-hosting with licenses retained.
- [ ] Add page metadata and language-indexing behavior based on verified business content and agreed URLs.
- [ ] Consolidate repeated room projections/forms/gallery markup as needed, with behavior checks; avoid abstraction without demonstrated need.
- [ ] Decide pagination/filtering thresholds for growing admin data sets and galleries.
- [ ] Complete deployment review, storage permissions, security headers, logging without personal data/secrets, backup recovery and real-browser smoke checks. Record rollback/release procedure.

Acceptance: documented results against agreed performance/accessibility goals and all critical public/admin journeys. Tests and screenshots must use disposable data, not expose guest enquiries or admin credentials.
