# Phase 2.3 — Light contact information panel

Implemented 2026-11-16 on `feature/website-redesign` from `0c55510` with a clean
working tree. The owner has given final visual approval for both the desktop
four-column panel and refined narrow-mobile single-column layout. Finalization
is recorded below; remaining interaction and breakpoint QA is separate.

## Audit and design

The four-link component belongs only to `Views/Home/Index.cshtml`. The former
full-width green surface, gold bottom border and inverse text have been replaced
by one warm-ivory panel with a thin border, 8px corners and no shadow. The section
continues the Hero ivory using `--kk-display-bg`; the panel stays within the
existing 1280px content container, aligned with the summary/About content below.
Phone gutters use 16px, matching the approved Hero. Section top padding is 24px,
with no overlap or added bottom padding. The following section is the published
room summary when available, otherwise About. Its markup and styles are unchanged.

Each whole contact item remains one native anchor. A small 36px pale-cream icon
container holds a 20px green icon, followed by a muted 14px label and green 16px
value. Phone, envelope and location are simple inline line icons; Facebook reuses
the bundled Bootstrap Icons path and its retained MIT license. All icon wrappers
are `aria-hidden="true"`; SVGs are non-focusable. There are no new dependencies,
images, JavaScript, animation or localization keys.

The existing `hero-contact-*` selectors remain component-specific. Added icon,
copy and label classes prevent styling all nested spans indiscriminately. Text
uses `minmax(0, 1fr)` and `min-width: 0`; narrow mobile uses normal word boundaries
and `overflow-wrap: break-word` for emergencies. Wider layouts retain their
approved `overflow-wrap: anywhere` rule. No fixed height or clipped text.
Whole-item hover/focus uses the existing cream surface, underline
on the value and a 2px green inset focus outline. Links exceed 44px touch targets.

Reused tokens: `--kk-display-bg`, `--kk-green`, `--kk-muted`, `--kk-cream`,
`--kk-cream-strong`, `--kk-border`, `--kk-radius`, `--kk-focus`, `--kk-type-support`
and spacing 1/2/3/4/5/6. Existing body typography is inherited. No root token,
palette, font family, header or Hero rule changed.

## Responsive behavior

| Width | Layout |
| --- | --- |
| 1200px and above (including 1440px) | Four columns; icon beside copy; 24px vertical/20px horizontal item padding. |
| 768–1199.98px (including 1024px) | Two columns; icon beside copy; dividers form a coherent 2×2 panel. |
| Above 480px through 767.98px (including 481px) | Two columns; icon above copy; 16px vertical/12px horizontal padding; text wraps. |
| 480px and below (including 320/375/390/480px) | One column; icon beside copy, vertically centered; 88px minimum item height, growing with text; horizontal separators. |

These describe implemented CSS, not verified browser measurements. The narrow
single-column fallback gives the existing email/address enough readable width.

## Contact data and localization

Contact values are still referenced from `SiteContact`; address and the existing
BG/EN labels come from `SiteText`. No contact value or URL is duplicated in new
configuration or changed. All four anchor opening tags match the previous source:

- Phone: `SiteContact.PhoneHref`, with `PhoneDisplay` as the visible value.
- Email: visible `SiteContact.Email`; **existing Home/Contact form route**, not
  `mailto:`. This destination was intentionally preserved.
- Facebook: `SiteContact.FacebookUrl`, `_blank`, `noopener noreferrer`.
- Address: localized `HouseAddress`, `SiteContact.MapsUrl`, `_blank`,
  `noopener noreferrer`.

The localized `QuickContacts` section name and `CallUs`, `WriteToUs`,
`VisitFacebook`, `Facebook`, `FindUs`, `HouseAddress` references are unchanged.
Language switching and persistence are outside this change.

## Verification and limits

- Solution and both standalone harness builds passed with zero warnings/errors,
  using `--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase23Verification/`.
  Used the documented isolated output procedure to avoid owner-run app locks.
- Fresh DLLs: Availability **14/14**, GalleryUpload **26/26**. All three Node suites
  (availability, gallery, header navigation): **20/20**. **60 existing checks passed**.
  No new tests were added for this presentation-only change.
- A temporary source check confirmed exact equality of homepage markup outside
  the contact section, all four anchor opening tags and all localized/value
  references inside it. A recursive CSS comparison confirmed every non-contact
  rule, including media/keyframe context, matches HEAD. This protects Hero,
  navigation, root foundations and adjacent-section source; it is not pixel QA.
- Calculated sRGB contrast for green/ivory **10.55:1**, green/hover cream
  **10.43:1**, green/icon cream **9.89:1**; muted text on those surfaces
  **8.51:1 / 8.42:1 / 7.98:1**. These exceed AA normal-text contrast thresholds;
  they are opaque-color calculations, not a complete accessibility audit.
- `git diff --check`, final diff review and local documentation links passed.

No Phase 2.3 browser session was run. Earlier isolated preview attempts in this
chat failed with `ERR_CONNECTION_TIMED_OUT`, and native browser selection also
failed previously. Browser rendering, real focus and link activation remain
unverified. No real web startup, EF commands, database connection, external-link
request or enquiry submission was performed.

Manual review remains required in **BG and EN** at **320, 375, 390, 480, 481, 768,
1024, 1440px**, plus the 480/768/1200px boundaries: panel/adjacent-section transition,
four groups, wrapping/overflow, short screens and long content. Verify Tab and
Shift+Tab, focus outlines, Enter/link activation, actual phone/form/Facebook/map
destinations, language switching, 200% zoom/reflow and reduced motion. Check the
next-section variants with/without published rooms and missing-photo states;
existing offline calendar QA remains outstanding. No browser check is claimed
as passed by the agent. Desktop and final mobile visual approval are
owner-provided; the outstanding detailed checks above remain separate.

### Final narrow-mobile refinement — 2026-11-16

Owner review approved desktop visuals and identified cramped email/address text
at 375/390px. Extended only the narrow-mobile media query from 374.98px to 480px.
Kept the 36px icon column, 12px gap, 16px vertical/12px horizontal padding, 88px
minimum rows, original item order and horizontal separators. Centered each icon
beside the existing label/value stack. Scoped `word-break: normal` and
`overflow-wrap: break-word` to both copy and values, prioritizing natural breaks
and retaining an emergency fallback for an unbroken string. No Razor, links,
translations, values, keyboard behavior or wider-screen rule changed.

A before/after CSS comparison confirmed that all text outside that media block
is identical, preserving desktop and tablet exactly at source level. Checked
rule selection for 320/375/390/480px (single column), 481/768px (two columns) and
1440px (four columns). These are source checks, not browser layout measurements.
Repeated solution and harness builds: zero warnings/errors. Availability 14/14,
GalleryUpload 26/26 and all Node suites 20/20 passed; `git diff --check` passed.

Browser verification remains unavailable from prior preview/selection failures;
no fresh browser run or new BG/EN visual result is claimed. Final review should
cover both languages at all seven widths above, full email fit where possible,
natural address wrapping, horizontal overflow, keyboard focus and contact-link
activation. Desktop approval remains intact; no commit or push.

Files: homepage `Index.cshtml`, public `site.css`, this note and `DEVLOG.md`.
No commit, push, merge or PR change during implementation. Commit message:
`feat: redesign homepage contact information panel`.

### Finalization after visual approval — 2026-11-16

The owner explicitly approved the desktop and refined single-column mobile
designs and authorized committing/pushing all four Phase 2.3 files. Reviewed the
complete initial implementation plus final mobile refinement against `0c55510`;
no unrelated changes. Repeated source comparison confirmed that homepage markup
outside the panel, all contact anchors/localized/value references and all
non-contact CSS rules (including Hero and Header & Navigation) remain unchanged.
No new visual change was introduced during finalization.

Fresh solution and both harness builds passed with zero warnings/errors using
the isolated-output procedure above. Availability 14/14, GalleryUpload 26/26 and
all three Node suites 20/20 passed (60 total). Staged whitespace and local
documentation links passed. PR #1 was confirmed open, draft and unmerged before
finalization; no PR editing/merge action is included. Final commit hash, push,
remote equality, clean working tree and post-push PR status are reported in the
completion response. Phase 2.4 is not started.

No new browser session or interaction check was run. Visual approval does not
establish the following outstanding manual QA as passed:

- BG/EN contact-panel verification and language switching.
- Responsive wrapping/overflow near 480px/481px and at 320px.
- Actual phone, contact-form, Facebook and map link destinations/activation.
- Keyboard traversal and visible focus.
- 200% zoom/reflow.
- Reduced-motion behavior.
