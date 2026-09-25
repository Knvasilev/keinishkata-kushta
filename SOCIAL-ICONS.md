# Footer Social Icons

The footer uses static PNG icons. A small CSS hover effect is disabled when the
visitor requests reduced motion. Both links have accessible names and native
title tooltips, with 44px click/touch targets. Icons are hosted locally and lazy
loaded; the original supplied images have not been changed.

## Assets and Credits

- Facebook: the supplied `icons8-facebook-50.png`, copied unchanged to
  `KeinishkataKushta/wwwroot/images/social/facebook.png`.
- Instagram: Icons8 outlined static icon, downloaded from
  https://img.icons8.com/ios/50/instagram-new--v1.png and stored as
  `KeinishkataKushta/wwwroot/images/social/instagram.png`.
- Instagram source page: https://icons8.com/icon/32292/instagram
- Icons8 free-use attribution information: https://icons8.com/license

These are Icons8 assets depicting the social brands, not files obtained directly
from Meta's brand-resource site. A visible Icons8 link is included in the shared
footer. Keep that credit unless your applicable license permits removing it.

## Future Changes

Replace the PNG files above to change the icons. Keep the same filenames, or
update their paths in `Views/Shared/_Layout.cshtml`. Razor's `asp-append-version`
adds a content hash so browsers can fetch a replacement instead of an old cached
copy. Update account links in `Infrastructure/SiteContact.cs`.

Restart the updated web project after changing Razor or C# files. No database
migration is needed.
