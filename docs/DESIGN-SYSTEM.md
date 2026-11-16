# Phase 2.1 — Public design foundations

Implemented 2026-11-13 against approved editorial Hero commit `f741032` on
`feature/website-redesign`. This is a value-preserving CSS extraction, not a new
palette or component redesign. Source of truth: `wwwroot/css/site.css` in the web
project. Existing `--kk-*` names remain compatible with `availability.css`.

## Audit and consolidation

- The older public palette and the approved Hero have intentionally different
  greens and ivory surfaces. Retain both; replacing one with the other would
  change the approved appearance.
- Serif declarations occurred across headings, summary figures, prices and
  placeholders. The full body fallback stack differs from the compact UI stack;
  preserve both, including Cyrillic/system fallback. No font dependency changed.
- Repeated 8px corners, pill corners, card shadows, gaps and padding now reference
  shared values. Measurements such as 18px, 26px, 34px and one-off shadows remain
  component-owned. No geometry was rounded to fit a scale.
- Primary/outline buttons already share base, hover/focus/active and disabled
  rules. Their padding, weight, corners, disabled opacity and focus ring now use
  foundations. Hero CTA overrides remain separate, preserving their colors,
  padding, wrapping, 48px minimum height and focus outline. Standard buttons keep
  their existing padding, including `.btn-sm` and `.btn-lg`; no broad `button`
  selector or new minimum size was introduced.
- Borders already use `--kk-border`; translucent header, inverse contact-strip,
  lightbox and hover borders remain local. Do not merge visually distinct alpha
  values simply because their base color matches.
- Breakpoints remain `991.98px` and `767.98px`; calendar keeps its existing
  `767px` boundary. No selectors, media queries, animations or cascade order changed.

## Color tokens and final values

| Token | Value / role |
| --- | --- |
| `--kk-green` | `#154212`, shared brand/actions |
| `--kk-green-soft` | `#2d5a27`, shared hover/active |
| `--kk-bg` | `#fbfbe2`, page |
| `--kk-surface` | `#ffffff`, elevated surface |
| `--kk-cream` | `#f5f5dc`, alternate surface/outline button |
| `--kk-cream-strong` | `#efefd7`, stronger alternate/outline hover |
| `--kk-ink` / `--kk-muted` | `#1b1d0e` / `#42493e`, primary/secondary text |
| `--kk-border` | `#cfd5c7`, border/divider; contact grid retains its local inverse override |
| `--kk-gold` / `--kk-gold-dark` | `#fed65b` / `#735c00`, accent/readable gold text |
| `--kk-on-brand` | `#fff`, inverse text |
| `--kk-focus` | `var(--kk-green)`, shared focus border |
| `--kk-focus-ring` | `0 0 0 .2rem rgba(254, 214, 91, .45)` |
| `--kk-display-green` / `--kk-display-green-hover` | `#173e20` / `#295333`, Hero brand/action |
| `--kk-display-bg` / `--kk-display-surface` | `#f7f5e9` / `#eae7d8`, Hero background/hover/photo fallback |
| `--kk-display-ink` / `--kk-display-accent` | `#30352c` / `#b69150`, Hero copy/decorative gold |

Hero-local `--hero-green` and `--hero-ivory` now alias the corresponding display
tokens. Decorative gold is not a normal-text color.

Status ownership stays with existing Bootstrap and the calendar rather than
introducing unused competing tokens. Bundled Bootstrap exposes `--bs-success:
#198754`, `--bs-warning: #ffc107`, `--bs-danger: #dc3545`. Contact success alerts
retain text/background/border `#0f5132 / #d1e7dd / #badbcc`; validation retains
Bootstrap danger text. Warning alerts, when used, retain `#664d03 / #fff3cd /
#ffecb5`. Do not use warning yellow as normal text on white. Calendar status
colors, text/symbol labels, loading states and error recovery are unchanged.

## Typography and geometry tokens

| Token | Final value |
| --- | --- |
| `--kk-font-body` | `"Plus Jakarta Sans", system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif` |
| `--kk-font-ui` | `"Plus Jakarta Sans", system-ui, sans-serif` |
| `--kk-font-heading` | `"Playfair Display", Georgia, serif` |
| `--kk-type-h1` / `--kk-type-h2` / `--kk-type-h3` | `3.5rem / 2.5rem / 1.6rem` |
| `--kk-type-display` | `clamp(2.5rem, 4.1vw, 3.75rem)` |
| `--kk-type-display-mobile` | `clamp(2.25rem, 7vw, 3rem)` |
| `--kk-type-support` | `.875rem`, existing Hero CTA/contact-strip supporting text |
| `--kk-leading-body` / `--kk-leading-heading` / `--kk-leading-display` | `1.6 / 1.14 / 1.12` |
| `--kk-weight-action` | `700`, buttons/navigation |
| `--kk-space-1` through `--kk-space-8` | `4 / 8 / 12 / 16 / 20 / 24 / 28 / 32px` |
| `--kk-space-10` | `40px` |
| `--kk-section-space` / `--kk-section-space-mobile` | `96px / 64px` |
| `--kk-container` / `--kk-container-display` | `1280px / 1376px` |
| `--kk-radius` / `--kk-radius-pill` | `8px / 999px` |
| `--kk-shadow` | `0 20px 44px rgba(62, 39, 35, .08)` |
| `--kk-shadow-card` | `0 14px 34px rgba(62, 39, 35, .06)` |
| `--kk-button-padding` / `--kk-display-button-padding` | `.78rem 1.35rem / .85rem 1.3rem` |
| `--kk-display-button-height` / `--kk-disabled-opacity` | `48px / .65` |

H1–H3 retain their existing weights and contextual/responsive overrides. H4–H6
retain Bootstrap's inherited sans-serif family, weight 500, line height 1.2 and
sizes 1.5rem (fluid below 1200px), 1.25rem and 1rem. They have no public-view
elements currently; imposing serif styles would silently change future semantics.
Bootstrap also owns `.lead`, general small/caption typography and base button
font sizing. Existing labels retain weight 800; eyebrows, prices, captions and
supporting descriptions keep their contextual sizes. Hero weight 600, line height
1.12, letter spacing, copy line height 1.8 and both fluid headline expressions
are preserved. These are foundations for reuse, not an exhaustive utility system.

## Verification and remaining review

- Solution and both standalone harness builds: `--no-restore -m:1 -v:minimal`,
  zero warnings/errors. Availability 14/14, GalleryUpload 26/26, Node 14/14 passed.
- A temporary comparison outside the repository expanded root tokens in both
  HEAD and working CSS, removed comments/root definitions and normalized whitespace.
  Every remaining rule matched, including selectors, declarations, order,
  component-local custom properties, media queries and keyframes. This verifies
  source equivalence for the current token values, not rendered pixel equality.
- Calculated sRGB contrast: body/page 16.28:1, secondary/page 8.87:1, Hero
  heading/ivory 10.99:1, Hero copy/ivory 11.48:1, primary 11.54:1, primary hover
  8.07:1, Hero primary hover 8.83:1, outline hover 9.89:1, dark gold/page 6.13:1.
  These selected opaque text pairs meet AA normal-text contrast; this is not a
  complete accessibility or focus-indicator audit. Bootstrap danger text on cream
  would be 4.09:1: do not reuse it on that surface without a separately reviewed
  correction. Existing Contact validation spans sit on white form surfaces.
- An isolated Node static fixture used the existing Razor Hero fragment,
  localization strings, Bootstrap, photographs and baseline/current CSS. Browser
  navigation to loopback failed with `net::ERR_CONNECTION_TIMED_OUT`; no screenshots
  or computed-style results were obtained. The temporary host was stopped.
- BG/EN rendering at 320, 375, 768, 1024 and 1440px, keyboard-only focus,
  zoom/reflow, reduced motion, long content, empty/missing-photo states and offline
  calendar requests still require browser review. Automated interaction checks
  cover simulated calendar failures and gallery focus/reduced motion.

Only public CSS and documentation changed. Razor, routes/CTA destinations,
translations, fonts, photographs, controllers, admin styles, calendar styles,
JavaScript, entities and migrations are unchanged. No production host or real
database was started. No commit, push, merge or PR update was performed during
the initial 2026-11-13 implementation.

### Finalization review — 2026-11-16

The owner approved the desktop appearance and authorized commit/push. The full
diff and token/documentation inventory were reviewed again; Hero markup, image
references, copy and CTA destinations remain unchanged. The expanded-value CSS
comparison passed for the full stylesheet, including the representative Contact
view's shared styles. No regression fix was needed in this finalization.

The existing running website locked normal build output (MSB3027/MSB3021).
Solution and both harness builds passed with zero warnings/errors using
`--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase21Verification/`.
Fresh harness DLL execution and Node suites passed all 54 checks.

Computer Use could inventory Chrome but stopped before reading the homepage
because it could not confidently determine the current browser URL. No new
browser screenshot, bilingual/mobile check or rendered Contact comparison was
obtained. The remaining manual checklist above still applies, with desktop
appearance approval supplied by the owner. Temporary helpers remain outside the
repository and are excluded from the commit. The draft PR status is unchanged.

## Phase 2.2 — Header & Navigation recommendations

Implemented scope and verification are now recorded in
[Header & Navigation Refinement](HEADER-NAVIGATION.md). The original recommendations
below remain the manual review baseline. The owner approved Phase 2.2 desktop
and mobile visuals on 2026-11-16; detailed interaction/accessibility browser
verification remains outstanding as recorded in the linked notes.

Capture bilingual baselines first, including long English navigation, 320px width,
expanded mobile menu, 200% zoom and 400% reflow. Then separately review localized
Menu/Language accessible names, skip link, route-aware `aria-current`, language
query/cookie behavior, keyboard focus and touch targets. Reuse existing tokens;
avoid changing Hero typography, CTA behavior or the brand palette as a side effect.
