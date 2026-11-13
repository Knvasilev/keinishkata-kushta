# Current design audit and future considerations

2026-11-12 source audit at `b95618a`. Findings below come from Razor, CSS, JavaScript and controllers. No live application, browser screenshot, performance measurement, contrast measurement, or assistive-technology session was used. Distinguish definite source defects from layout risks that need rendering. This document proposes future work; no redesign was implemented.

## Existing visual system

`wwwroot/css/site.css` defines cream backgrounds (`#fbfbe2`, `#f5f5dc`), dark green (`#154212`), gold (`#fed65b`), dark body text, muted text, borders and shadows through `--kk-*` variables. Public headings use Playfair Display; body text uses Plus Jakarta Sans, requested through Google Fonts. The layout uses a sticky translucent header, logo plus text brand, BG/EN switch, availability action, pill buttons, 8px card corners and generous section spacing.

The homepage uses `hero-clean.png` as a background behind a centered translucent text panel. `hero-image.png` is bundled but is not used by the reviewed public views. Room cards have cropped 4:3 photographs, prices and capacity; details start with the first ordered gallery photo, independently of the room-card cover. Nearby places use a mixed-width card grid. Contact uses a dark introduction and overlapping cards/form. The calendar uses a compact separate stylesheet and a text/symbol legend. Admin has a separate Bootstrap layout and small stylesheet; room forms still use mostly unstyled native inputs.

Responsive rules exist at 991.98px and 767.98px, with calendar rules at 767px. Auto-fit grids handle room and photo tiles; public multi-column sections stack on smaller screens; Bootstrap collapses public navigation below its lg breakpoint. Photo cropping, font fit and overflow still need visual checks with actual content.

## Existing strengths to retain

- Clear server-rendered navigation, phone/email/map actions, room price/capacity and enquiry links.
- BG/EN public document language and editable business translations with per-field fallback.
- Lazy loading/async decoding on many secondary images; CSS aspect ratios reserve many image areas.
- Gallery triggers are buttons with accessible names; dialogs support Escape, keyboard navigation, focus trapping/restoration, swipe and single-image handling.
- Calendar table captions, weekday headers, hidden date/status text, symbols in addition to colour, busy/status announcements, focus recovery and no-JavaScript navigation.
- Reduced-motion rules and motion preference checks; social links have accessible names and 44px targets with preserved license attribution.
- Empty room/gallery states and useful contact/calendar success or error states already exist.

## Verified issues and source-based risks

| Priority | Finding and evidence | Future action / validation |
| --- | --- | --- |
| P0 | `Views/Home/Privacy.cshtml` still contains template text. Public footer has no privacy link, while Contact persists personal data. | Have the owner supply/review accurate retention/contact/data-use content; expose it from relevant pages in BG/EN. Policy adequacy has not been legally assessed. |
| P1 | Admin Room Index headers say Id, Name, Slug, Capacity, Price, Published, Cover; body inserts the cover before slug. | Align cells/headings; provide cover alt text (currently absent), table context and narrow-screen treatment. |
| P1 | Admin layout omits jQuery while Login and Setup render `_ValidationScriptsPartial`, whose plugins depend on it. Server validation remains present. | Restore dependency order in a separate change; check console and client/server validation on both forms. |
| P1 | Room Create/Edit expose validation spans only for English fields and have no validation summary. Slug conflicts are model errors that lack a displayed field error there. | Show actionable per-field and summary errors, preserve input, announce/focus feedback; use consistent labelled form styling. |
| P1 | Public layout has no skip link or active-page `aria-current`; Menu/Language accessible labels are hardcoded English. Shared room cards use h3 even on the listing directly below h1. | Add keyboard bypass, route-aware state, localized control labels and context-appropriate heading hierarchy. Verify keyboard and screen-reader navigation. |
| P1 | Admin `<html lang="en">` surrounds mixed English/Bulgarian screens; navbar has no collapse control despite `navbar-expand-lg`. Room list lacks the responsive wrapper used by message/availability tables. | Choose/localize admin language deliberately; verify navigation, reflow and tables at phone widths and zoom. |
| P1 | Public gallery triggers depend on JavaScript for opening full images; room lightbox has click-to-next but no visible previous/next controls. Property caption lacks the room dialog's live region. | Assess explicit controls, translated instructions, announcement consistency, inert background and usable full-image fallback. Preserve gestures and focus behavior. |
| P2 | Homepage hero remains at least 620px tall under 992px with large spacing; brand, links and form content can be long in either language. | Capture 320-1440px baselines, 200% zoom and 400% reflow before changing layout. Check header wrapping, hero prominence, contact overlap and long captions. |
| P2 | Uploads are served at original size; no generated thumbnails, `srcset`/`sizes`, decoding/resize pipeline or explicit image dimensions on many tags. | Measure transfer size, LCP/CLS and crops using approved assets. Plan derived variants separately; preserve originals and scope storage changes explicitly. |
| P2 | Shared head has title and viewport but no page descriptions, canonical/hreflang, social metadata or structured guest-house data. | Establish URL/language indexing rules and verified business facts before adding metadata. No SEO crawler audit was performed. |
| P2 | Public language resolution prioritizes a query over the cookie; switching language preserves the current query string. A retained `?lang=bg` can override a newly selected EN cookie. | Reproduce query/cookie switch combinations; define stable language URLs and return behavior without breaking calendar navigation. |
| P2 | Contact fields have labels but no explicit autocomplete hints; success uses an ordinary alert div. DataAnnotations default messages are not localized by `SiteText`. | Review input purpose, localized errors, status announcements and keyboard error recovery. |
| P2 | Tokens exist publicly, but admin duplicates colours, room form markup is repeated, lightbox markup differs, and room-card EF projections repeat in Home/Rooms. | Consolidate gradually after baseline behavior checks; avoid a framework rewrite just to restyle pages. |

Contrast, touch targets beyond the explicitly sized controls, focus visibility on all backgrounds, font fallback/Cyrillic rendering, forced colours and reduced-motion coverage need measurement. Existing accessibility features do not establish WCAG conformance. External fonts introduce third-party requests; assess self-hosting and licensing during performance/privacy planning.

## Product and content decisions before redesign

Clarify how room-level pricing and the homepage's maximum room capacity should explain a stay reserved exclusively for one group. Availability currently blocks the entire house, even when only one room is used. Keep that policy explicit; do not imply an instant booking or guaranteed available dates. Room details link to a generic enquiry form without carrying room/date context; contextual enquiries would require a separately scoped behavior change.

Obtain owner-approved amenities, house rules, check-in/out details, accessibility information, prices, captions and both language versions. Do not invent amenities, reviews, travel times, availability, ratings or booking integrations. Nearby content is currently code-owned. Preserve photos and branding; any new image selection, optimization or derived assets must be separately authorized.

## Future design acceptance checklist

Before implementing, capture route/state screenshots with disposable, representative data and agree a small design specification: type scale, spacing, colour roles, buttons, form states, navigation, cards and gallery controls. Keep availability/enquiry wording consistent and retain existing URLs.

Verify BG/EN at 320, 375, 768, 1024 and 1440 CSS pixels; desktop and phone landscape; 200% zoom and 400% reflow; keyboard-only use; screen-reader status and errors; reduced motion; touch gestures; high contrast; long/missing translations and captions; empty rooms/galleries; one/many photos; failed calendar fetch; unavailable images; signed-out admin redirects. Use measured contrast and automated accessibility checks as support for manual review. Record results per route rather than claiming a site-wide pass from the calendar preview.
