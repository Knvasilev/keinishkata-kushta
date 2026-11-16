# Phase 2.2 — Header & navigation refinement

Implemented 2026-11-16 on `feature/website-redesign`, starting at `c0a6380`
with a clean working tree. The owner subsequently approved desktop and mobile
visual review on 2026-11-16 and authorized finalization; see the record below.

## Audit and implemented behavior

The shared public layout has one navigation tree using Bootstrap 5.1 collapse.
Previously the mobile list inherited a 12px gap between links, 18px top padding
and an 18px secondary-controls margin. Menu/Language names were hardcoded English;
there was no current-page state, close icon or Escape handler. Bootstrap already
owned native button activation, `aria-expanded` and hidden-menu display.

- Preserve the logo, brand, five links, BG/EN controls, full-width Availability
  CTA and in-flow expansion. Keep the approved desktop arrangement and navbar
  dimensions. No Hero, photography, footer, calendar, forms or data changes.
- Use `navbar-expand-xl` with matching CSS/JS boundaries: collapsed below 1200px,
  expanded at 1200px and above. The old boundary was 992px. Source inspection
  identified horizontal competition from the full bilingual brand, links and
  actions near 1024px; the exact rendered fit remains for manual review.
- Mobile links have no inter-row gap and a 48px minimum height with 12px padding
  (about 50px at the default text metrics). They can grow with wrapped text.
  Header side padding is 16px, matching the phone Hero. Secondary controls have
  a thin divider, 12px spacing, and remain below the links. Language targets are
  at least 44×44px, and the existing toggle remains 48×44px.
- Small-screen header scrolling is bounded by `100dvh` with `100vh` fallback,
  keeping expanded controls reachable in short viewports. Brand text can wrap.
- Current links use `--kk-green` on `--kk-cream-strong`, without extra bars or
  heavier typography (navigation was already weight 700). Controller/action
  matching gives `aria-current="page"` to exact navigation destinations. Room
  detail routes mark the Rooms parent with `aria-current="location"`; Home does
  not become current on Contact, Privacy or error pages. Availability exposes
  its current-page state on the CTA, which retains its existing green treatment.
- CSS derives hamburger/X from Bootstrap's `aria-expanded` with no animation.
  New standalone `navigation.js` localizes open/close names through `SiteText`,
  closes an open mobile menu on Escape and restores toggle focus. It also
  recovers focus when breakpoint changes would hide the focused control.
  Enter/Space remain native button actions; no focus trap or duplicate menu.
- All header links and the toggle receive a 2px green `:focus-visible` outline
  with 3px offset. Bootstrap keeps responsibility for collapse visibility and
  reduced-motion-aware transitions.
- Keep query/cookie selection and one-year cookie persistence. Header language
  return URLs omit only `lang`, retaining other query values and PathBase. This
  resolves the existing case where an old `?lang=en` overrode a newly selected BG
  cookie. Language group and menu names use `SiteText`; language names use their
  native spellings. Destinations remain the existing MVC actions.

No new CSS tokens or dependencies. Reused `--kk-green`, `--kk-cream-strong`,
`--kk-border`, `--kk-focus`, spacing 2/3/4 and existing radius/typography tokens.
The new 1200px boundary affects only the header; other page breakpoints stay put.

## Verification and remaining manual review

Solution and both separate harness builds passed with zero warnings/errors using
`--no-restore -m:1 -v:minimal -p:BaseOutputPath=bin/Phase22Verification/`.
Used the documented isolated-output procedure because an owner-run Visual Studio
site locked standard output during Phase 2.1; did not interrupt that process.
Fresh DLL execution passed Availability 14/14 and GalleryUpload 26/26.

Run all Node interaction suites with:

```powershell
node --test .\tests\availability-navigation.test.cjs .\tests\gallery-interaction.test.cjs .\tests\header-navigation.test.cjs
```

All 20 passed, including six new simulated-DOM tests for labels, Escape, focus
restoration, breakpoint focus, non-interception and absent-header initialization.
These tests do not exercise Bootstrap rendering or native keyboard activation.
Source review checked routes, localization, exact-page/parent matching, the one
menu tree, Bootstrap visibility behavior and preserved Hero rules. Diff whitespace
and local documentation links were checked.

The existing isolated Availability preview host was started without invoking web
startup or connecting to a database. It reported listening on loopback, alongside
a Data Protection key-ring access-denied warning. The in-app browser could not
open it (`net::ERR_CONNECTION_TIMED_OUT`); no rendered page or screenshots were
obtained. The preview was stopped. No real web/database startup was performed.

Pending owner/browser review in **both BG and EN**, expanded/collapsed at **320,
375, 390, 768, 1024 and 1440px**, plus either side of 1200px:

- Spacing, brand wrapping, X shape, active states, no horizontal overflow and CTA
  reachability, including short landscape screens and long labels.
- Tab/Shift+Tab order, Enter/Space, Escape, visible focus, hidden-menu exclusion,
  and focus when resizing. Screen-reader announcement of labels/current state.
- 200% zoom, text reflow, reduced motion, language click-through (including an
  initial `?lang=` with other query parameters), all navigation/CTA destinations.
- Existing full-page long/empty/missing-photo states and offline calendar recovery
  remain browser QA; unchanged gallery/calendar simulation tests passed.

No commit, push, branch switch, merge or draft PR change during implementation. Suggested commit:
`feat: refine responsive header navigation and accessibility`.

## Finalization — 2026-11-16

The owner reports that desktop and mobile visual review passed and approves the
implementation. This supersedes the pending visual approval above; specific
viewport/language coverage was not supplied. It does not establish that keyboard,
screen-reader, zoom/reflow, reduced-motion or browser interaction checks passed.
Those checks, language/CTA click-through, short-screen/long-content edge cases,
empty/missing-photo states and offline calendar recovery remain manual QA unless
separately verified. No new browser session was run during finalization.

Reviewed all eight Phase 2.2 files with no unrelated changes. Hero markup/assets,
root design-system tokens and unrelated source remain unchanged. Repeated the
solution and both harness builds using the isolated-output command above: all
passed with zero warnings/errors. Fresh harness execution passed 14/14 and 26/26;
all three Node suites passed 20/20 (60 total). Final staged whitespace and
documentation link checks passed.

Commit/push are explicitly authorized for `origin/feature/website-redesign` with
the suggested message above. PR #1 was confirmed open and draft before
finalization; no PR editing or merge action is part of this work. Final commit
hash, push verification and post-push PR status are reported in the completion
response. Phase 2.3 is not started.
