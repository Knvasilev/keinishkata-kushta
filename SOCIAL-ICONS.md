# Footer Social Icons

The footer uses static SVG icons. A small CSS hover effect is disabled when the
visitor requests reduced motion. Both links have accessible names and native
title tooltips, with 44px click/touch targets. Icons are hosted locally and lazy
loaded. SVGs stay sharp at different screen densities without an icon font or CDN.

## Assets and Credits

- Source: [Bootstrap Icons v1.13.1](https://github.com/twbs/icons/tree/v1.13.1).
- [Facebook SVG](https://github.com/twbs/icons/blob/v1.13.1/icons/facebook.svg),
  stored unchanged at `KeinishkataKushta/wwwroot/images/social/facebook.svg`.
- [Instagram SVG](https://github.com/twbs/icons/blob/v1.13.1/icons/instagram.svg),
  stored unchanged at `KeinishkataKushta/wwwroot/images/social/instagram.svg`.
- Copyright (c) 2019-2024 The Bootstrap Authors.
- [MIT license](KeinishkataKushta/wwwroot/images/social/LICENSE.txt), bundled
  unchanged from the same release. Retain this notice and license when copying
  or redistributing the icons. A visible footer attribution is not required.

These are Bootstrap Icons assets depicting social brands, not files obtained
directly from Meta. The asset license does not grant ownership of the Facebook
or Instagram trademarks. The icons identify links to the house's own profiles.

## Future Changes

Replace the SVG files above with appropriately licensed assets to change the
icons, and update the source and license documentation. Keep the same filenames, or
update their paths in `Views/Shared/_Layout.cshtml`. Razor's `asp-append-version`
adds a content hash so browsers can fetch a replacement instead of an old cached
copy. Update account links in `Infrastructure/SiteContact.cs`.

Restart the updated web project after changing Razor or C# files. No database
migration is needed.
