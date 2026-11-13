# Development log

## 2026-11-13 — Selectively restore the original editorial split hero

Inspected current directory, branch `feature/website-redesign`, Git status/diff, and the original implementation plus both experiment edits recorded in this chat. Selectively reversed only the atmospheric wrapper/gradient and blurred photo bleed, without a Git reset/checkout or discarding other uncommitted work. The experiment entries below remain as historical records and are superseded by this restoration.

Removed the `homepage-atmosphere` wrapper, its four color tokens, gradient, and transparent-section overrides. Removed all hero bleed tokens, mobile overrides, photo pseudo-element, masks, blur, stacking isolation, and experiment-only text positioning/overflow clipping. Restored the original media wrapper's `overflow: hidden`, 8px corners, and neutral fallback surface; removed the corresponding experimental foreground-image surface rules. The original hero has its solid `#F7F5E9` background, sharp photograph at `center 30%`, 45/55 layout, responsive sources/sizes, bilingual copy, original CTAs, focus styling, and reduced-motion-aware animations.

Source comparison against the original implementation edits recorded in this chat confirms the first redesign's markup/styles were restored. All homepage content from the contact strip onward matches HEAD after normalizing line endings; original opaque section backgrounds are effective again. Localization, images, shared layout, JavaScript, controllers, data/schema/migrations, and other uncommitted changes were not modified. Files changed in this restoration: `Views/Home/Index.cshtml`, `wwwroot/css/site.css`, and this log.

Verification: solution and both standalone harness builds with `--no-restore -m:1 -v:minimal` passed with zero warnings/errors. Availability **14/14** and GalleryUpload **26/26** passed using `dotnet run --no-build --no-restore`; Node interaction suites **14/14** passed (**54 total**). `git diff --check` passed. Confirmed no experimental wrapper, tokens, masks, or photo pseudo-element remain in homepage markup/public CSS. The original gold eyebrow pseudo-element is intentionally retained.

Browser rendering was not rechecked: the isolated preview timed out earlier in this chat and was not retried. Exact visual/screenshot equality, BG/EN breakpoint rendering, keyboard/zoom behavior, and live CTA click-through therefore remain unverified in a browser; restoration is confirmed at source level. No production startup, database operations, branch reset, commits, pushes, merges, or PR modifications were performed.

## 2026-11-13 — Soft blurred photo bleed around the hero

Verified directory, `feature/website-redesign` branch, and the existing uncommitted Phase 1/1.1 changes before editing. This increment changes only `wwwroot/css/site.css` and this log. Preserved the homepage gradient, all Razor/localized content, responsive foreground image sources, 45/55 layout, image dimensions/crop, CTAs, remaining homepage sections, and backend code.

Added a decorative `.editorial-hero-media::before` layer using the existing hero photograph. Desktop tokens: `--hero-bleed-spread: 6%` (negative inset on every side, making the layer 112% of the image box), `--hero-bleed-blur: 36px`, `--hero-bleed-opacity: .4`. Under 768px these become 5% (110% box), 24px, and .28, using the 768px image variant. Both layers align at `center 30%`; the foreground remains unfiltered. A radial ellipse mask is opaque through 72% and transparent at 100%, with a WebKit-prefixed equivalent. Unsupported masks omit the decorative layer entirely. The photo retains its 8px corners and fallback background on the foreground image; its wrapper permits the atmosphere to spread. Local stacking isolation keeps the pseudo-element behind the photograph, the text is raised above it, and hero `overflow: clip` contains overflow. The effect uses no JavaScript, new assets, or animation; existing reduced-motion-aware entrance effects are retained.

Verification: solution and both standalone harnesses built with `--no-restore -m:1 -v:minimal`, zero warnings/errors. Availability **14/14** and GalleryUpload **26/26** passed via `dotnet run --no-build --no-restore`; Node interaction suites **14/14** passed (**54 total**). `git diff --check` passed. Reviewed the source for preserved crop, mobile order, localization, CTA destinations, and hero-only CSS scope.

Visual limits: no browser rendering was verified for this increment. The isolated in-app preview previously timed out in this chat and was not retried. The softness/strength of the bleed, browser mask/filter behavior, text contrast with the composited atmosphere, and absence of clipping/scrolling at 320, 390, 768, 1024, and 1440px remain for manual BG/EN review, along with keyboard/zoom/reduced-motion checks. Earlier contrast calculations concern the base gradient only, not this composited photo layer. No real application/database startup, image edits, dependencies, commits, pushes, merges, or PR modifications were made.

## 2026-11-13 — Phase 1.1 atmospheric homepage background

Verified the solution directory and `feature/website-redesign` branch. Preserved the uncommitted Phase 1 hero, translations, and two supplied WebP assets. Added one `homepage-atmosphere` wrapper around the existing homepage sections and 22 lines of homepage-scoped CSS. No sections were rewritten, reordered, or removed.

The single vertical background uses sky `#EEF5F6` at 0%, mist `#F4F5EE` at 18%, sunlit ivory `#F8F4E9` at 40%, and sage `#F0F4E9` at 82–100%. Stops scale with the content height at every viewport. Hero, About, room-preview section, and stay-experience section backgrounds become transparent only inside this wrapper, so they reveal the same gradient without restarting it. The dark contact strip and local guide remain opaque anchors. Room cards, summary panel, missing-photo placeholder, final enquiry panel, header, and footer retain their existing surfaces; those deliberately mask the wash locally. No image, crop, layout dimension, text, route, localization key, interaction, dependency, or animation was changed in this increment.

Verification:

- Solution and both standalone harness builds with `--no-restore -m:1 -v:minimal`: passed, zero warnings/errors. Ran each harness with `--no-build --no-restore`: Availability **14/14**, GalleryUpload **26/26**. Node interaction suites: **14/14**. Total **54 checks passed**.
- Calculated sRGB contrast for the existing exposed text colors against all four stops and 101 interpolated samples per interval: hero body `#30352C` minimum **11.26:1**, muted body `#42493E` **8.35:1**, brand green `#154212` **10.34:1**, hero green `#173E20` **10.78:1**, dark gold `#735C00` **5.77:1**. These calculations support normal-text contrast on the gradient, not a full rendered accessibility audit. Button borders and keyboard-focus styling remain unchanged.
- `git diff --check` passed; source review confirmed the new selectors require the homepage wrapper. Other pages, existing Phase 1 changes, and runtime/database code were preserved.

Browser QA remains outstanding: the in-app browser timed out against the isolated loopback preview in Phase 1; no browser attempt was repeated for this CSS-only increment. BG/EN rendering at 320, 375, 390, 768, 1024, and 1440px, gradient balance/crop appearance, overflow, keyboard focus, zoom/reflow, mobile menu, language switching, and CTA click-through have not been visually verified. No real web startup, database migration, commit, push, merge, or draft pull-request update was performed. Manual visual review remains required.

## 2026-11-13 — Phase 1 editorial split homepage hero

Verified the solution directory, branch `feature/website-redesign`, and initially clean working tree. Read the project guidance and design/architecture documents; inspected the homepage, layout, public CSS/JavaScript, localization helper, routes, and branding assets.

Replaced only the homepage hero with an ivory 45/55 editorial split, existing serif typography, localized introductory label/headline/description, and differentiated links to the existing Gallery and Availability actions. Added the supplied `keinishkata-hero-768.webp` and `keinishkata-hero-1086.webp` references with responsive source selection, high fetch priority, intrinsic dimensions, and cover cropping. The owner supplied these two assets during the task; no existing image was edited or removed. Hero styles include a stacked phone layout, wrapping actions, visible keyboard focus, and CSS entrance/hover effects that respect reduced motion. Global tokens, shared layout, JavaScript, all subsequent homepage sections, controllers, database code, and migrations are unchanged.

Verification:

- Solution build with `--no-restore -m:1 -v:minimal`: passed with zero warnings/errors.
- Default `dotnet run --no-restore` for both standalone harnesses failed with no useful build diagnostics. Building each separately with `--no-restore -m:1 -v:minimal` succeeded with zero warnings/errors; executing their built DLLs passed Availability **14/14** and GalleryUpload **26/26** checks.
- Node availability/gallery interaction suites: **14/14** passed. Total: **54 checks passed**.
- `git diff --check` passed. Reviewed the final changes for hero-only scope and retained existing images/licenses.
- Attempted an isolated homepage preview using a temporary in-memory homepage model in the existing preview host. The host reported listening on loopback, but the in-app browser timed out connecting. The temporary fixture change was restored and the host stopped. Production startup and its database migration/seeding path were not invoked.

Limits and remaining review: no rendered BG/EN screenshots or browser checks at 320, 375, 768, 1024, 1440, or 1920px could be completed. CTA destinations were verified against the existing MVC actions in source, not clicked through in a browser. Keyboard-only interaction, zoom/reflow, reduced-motion rendering, long content, missing-image rendering, and offline calendar behavior remain for browser review; automated interaction tests cover simulated calendar failures and gallery reduced motion. No real SQL Server, deployment, or performance measurements were performed. The supplied largest image is 1086px wide, so high-density desktop displays may upscale it. No dependencies were added, and no commit or push was made.

## Recorded repository history

The entries below summarize the existing Git log; they do not reconstruct undocumented sessions or treat migration timestamps as development dates.

| Commit date | Commit | Recorded change |
| --- | --- | --- |
| 2026-09-25 | `242bb33` | Initial portfolio version of Keinishkata Kushta. |
| 2026-09-25 | `2c10f5b` | Improve README structure and update credits. |
| 2026-09-25 | `2fcd88a` | Clean up portfolio documentation and remove personal publishing notes. |
| 2026-09-25 | `728b8d9` | Replace social icons with MIT-licensed Bootstrap Icons. |
| 2026-11-12 | `b95618a` | Improve gallery management and add English content support. |

Existing detailed feature notes remain in [AVAILABILITY.md](AVAILABILITY.md), [SOCIAL-ICONS.md](SOCIAL-ICONS.md), [docs/GALLERY-ORDERING.md](docs/GALLERY-ORDERING.md), and [docs/ENGLISH-CONTENT.md](docs/ENGLISH-CONTENT.md).

## 2026-11-12 — Architecture and redesign preparation audit

Verified working directory `D:\Programming\Keinishkata-Web\KeinishkataKushta`, branch `feature/website-redesign`, HEAD `b95618a`; initial Git working tree was clean. No branch switch, commit or push was performed.

Reviewed existing documentation before writing: README, availability, social icons, gallery ordering and English content notes. Inspected solution and all four project definitions; public/admin controllers, view models and Razor views; infrastructure helpers; EF entities/mappings and migration history; first-party CSS/JavaScript, bundled library headers, asset paths, ignore rules and test harnesses. Local credentials/settings values, live guest records and runtime photos were not part of the audit.

Created these five root documents:

- `AGENTS.md`: repository instructions, safety boundaries, conventions and verification commands.
- `ARCHITECTURE.md`: source structure, dependencies, startup/request flow, public/admin functionality, data model, migration inventory and operational boundaries.
- `DESIGN.md`: code-based design/accessibility assessment and future acceptance checks.
- `ROADMAP.md`: ordered, explicitly proposed development work and acceptance criteria.
- `DEVLOG.md`: verified Git history and this audit record.

Existing documentation was preserved unchanged. No application source, schema, migration, image or runtime data was changed. Build commands can update ignored generated build output; those files are not audit deliverables.

### Verification and limits

- `node --test .\tests\availability-navigation.test.cjs .\tests\gallery-interaction.test.cjs`: **14 passed, 0 failed**. These are simulated DOM interaction checks, not a real-browser run.
- `dotnet build .\KeinishkataKushta.sln --no-restore`: returned failure with **0 warnings and 0 errors reported**, without a diagnostic cause in its output. No dependency or application change was made to hide this result.
- `dotnet build .\KeinishkataKushta\KeinishkataKushta.Web.csproj --no-restore -m:1 -v:minimal`: **passed, 0 warnings / 0 errors**.
- `dotnet build .\KeinishkataKushta.sln --no-restore -m:1 -v:minimal`: **passed, 0 warnings / 0 errors**. The initial default-worker failure was not reproduced with single-worker MSBuild; its cause remains unconfirmed.
- Built both standalone harness projects with `--no-restore -m:1 -v:minimal`: **both passed, 0 warnings / 0 errors**. Executed their built DLLs with `dotnet`: **Availability 14/14 passed; GalleryUpload 26/26 passed**. These entry points use model/SQL translation checks and isolated SQLite/sample files; they do not invoke the production startup migration path.
- Total executed checks: **54 passed** (14 availability, 26 gallery/upload, 14 JavaScript).
- Final documentation checks: local Markdown links resolved; `git diff --check` passed; Git status showed only the five new documentation files.
- Did not run the real web host, EF database commands, or inspect/change the applied database schema. Startup migration/admin-seeding side effects were identified from source.
- Did not perform browser screenshots, live SQL Server integration/concurrency tests, deployment verification, external link validation, package vulnerability checks, measured performance/contrast checks, or a legal/privacy assessment.

Key findings: scaffold privacy page; admin room table column mismatch and missing alt text; admin validation plugins loaded without jQuery; incomplete room-form validation feedback; gallery file/database deletion consistency and concurrent ordering/cover risks; extension-only image validation; missing contact abuse controls; query/cookie language-switch conflict; remaining mobile/accessibility/SEO verification needs. These are documented recommendations, not fixes made in this session.
