using Microsoft.AspNetCore.Mvc;

namespace KeinishkataKushta.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult Set(string lang, string? returnUrl = null)
        {
            var normalized = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "bg";

            Response.Cookies.Append("kk_lang", normalized, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return LocalRedirect(returnUrl);
        }
    }
}
