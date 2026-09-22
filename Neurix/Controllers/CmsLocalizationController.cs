using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurix.BLL.Common;
using Neurix.Localization;

namespace Neurix.Controllers
{
    /// <summary>
    /// Dashboard localization plumbing: a switch endpoint for a future UI toggle and a
    /// diagnostics page that proves the infrastructure resolves language, direction and
    /// dictionary keys. No dashboard text is localized yet — that is deliberate.
    /// </summary>
    // AdminOrStaff at controller level: the language toggle lives in the CRM header too,
    // and CRM pages admit CrmStaff. Restricting the switch to Admin would hand staff an
    // Access Denied page when they click it. The diagnostics page stays Admin-only via its
    // own attribute (multiple [Authorize] attributes are AND-ed).
    [Authorize(Roles = CrmRoles.AdminOrStaff)]
    [Route("cms/localization")]
    public class CmsLocalizationController : Controller
    {
        private readonly ICmsLocalizer _localizer;

        public CmsLocalizationController(ICmsLocalizer localizer)
        {
            _localizer = localizer;
        }

        [HttpGet("")]
        [Authorize(Roles = CrmRoles.Admin)]
        public IActionResult Index()
        {
            var current = CmsLanguage.Current;

            ViewData["Title"] = "Localization";
            ViewData["CurrentCode"] = current.Code;
            ViewData["CurrentName"] = current.NativeName;
            ViewData["Dir"] = current.Dir;
            ViewData["IsRtl"] = current.IsRightToLeft;

            // A translated key, and one that exists only in English, to show the
            // per-key fallback chain working.
            ViewData["Probe"] = new Dictionary<string, string>
            {
                ["Common.Save"] = _localizer["Common.Save"],
                ["Diagnostics.Title"] = _localizer["Diagnostics.Title"],
                ["Diagnostics.MissingKeyProbe"] = _localizer["Diagnostics.MissingKeyProbe"],
                ["Totally.Absent.Key"] = _localizer["Totally.Absent.Key"]
            };

            return View();
        }

        /// <summary>
        /// Explicit switch for a toggle button. The ?lang= query alone already works
        /// anywhere in the dashboard (see UseCmsLanguageCookie); this just gives the UI a
        /// stable endpoint and returns the user to where they were.
        /// </summary>
        [HttpGet("set")]
        public IActionResult SetLanguage(string lang, string? returnUrl = null)
        {
            if (!CmsLanguage.IsSupported(lang))
            {
                lang = CmsLanguage.DefaultCode;
            }

            Response.Cookies.Append(
                CmsLanguageExtensions.CookieName,
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
                    new Microsoft.AspNetCore.Localization.RequestCulture(
                        CmsLanguage.DefaultCode, CmsLanguage.FromCode(lang).Code)),
                new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = System.DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Path = "/"
                });

            // Only ever redirect within this site.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
