using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.Common;
using Neurix.Localization;
using Neurix.Models;

namespace Neurix.Controllers
{
    /// <summary>
    /// Site-wide colour theme: a brand palette plus one light and one dark surface shade,
    /// each picked from the presets in <see cref="SiteTheme"/>. Stored as theme.* site settings
    /// on the "neurix" profile, which is the profile every layout reads.
    /// </summary>
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/settings/theme")]
    public class CmsThemeController : Controller
    {
        private const string SiteSlug = "neurix";

        private readonly ICmsSiteSettingService _settingService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly ICmsLocalizer _localizer;

        public CmsThemeController(
            ICmsSiteSettingService settingService,
            ICmsCompanyProfileService profileService,
            ICmsLocalizer localizer)
        {
            _settingService = settingService;
            _profileService = profileService;
            _localizer = localizer;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var theme = SiteTheme.FromSettings(await _settingService.GetSettingsByCompanySlugAsync(SiteSlug));

            return View(new CmsThemeViewModel
            {
                Palette = theme.Palette.Key,
                Light = theme.Light.Key,
                Dark = theme.Dark.Key
            });
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CmsThemeViewModel model)
        {
            if (!SiteTheme.IsKnownPalette(model.Palette) || !SiteTheme.IsKnownLight(model.Light) || !SiteTheme.IsKnownDark(model.Dark))
            {
                TempData["ErrorMessage"] = _localizer["Please choose a theme option from the list."];
                return RedirectToAction(nameof(Index));
            }

            var profile = await _profileService.GetBySlugAsync(SiteSlug);
            if (profile == null)
            {
                TempData["ErrorMessage"] = _localizer["The main site profile was not found."];
                return RedirectToAction(nameof(Index));
            }

            var result = await _settingService.SaveBatchAsync(profile.Id, new[]
            {
                Setting(profile.Id, SiteTheme.PaletteKey, model.Palette, "Theme colour palette"),
                Setting(profile.Id, SiteTheme.LightKey, model.Light, "Light mode shade"),
                Setting(profile.Id, SiteTheme.DarkKey, model.Dark, "Dark mode shade"),
            }, GetCurrentUserId());

            if (result.Success)
            {
                TempData["SuccessMessage"] = _localizer["Site theme updated successfully."];
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? _localizer["Failed to save the theme."];
            }

            return RedirectToAction(nameof(Index));
        }

        private static CmsSiteSettingUpsertDto Setting(Guid companyId, string key, string value, string label) => new()
        {
            CompanyProfileId = companyId,
            Key = key,
            ValueEn = value,
            ValueAr = value,
            SettingType = "text",
            GroupName = SiteTheme.SettingGroup,
            Label = label
        };

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
