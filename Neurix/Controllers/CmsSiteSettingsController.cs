using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.Common;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/settings")]
    public class CmsSiteSettingsController : Controller
    {
        private readonly ICmsSiteSettingService _settingService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly ILogger<CmsSiteSettingsController> _logger;

        public CmsSiteSettingsController(
            ICmsSiteSettingService settingService,
            ICmsCompanyProfileService profileService,
            ILogger<CmsSiteSettingsController> logger)
        {
            _settingService = settingService;
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id;

            // Theme keys are managed on their own page (CmsThemeController); showing them here as
            // free-text fields would let SaveBatch overwrite them with arbitrary values.
            var settings = (await _settingService.GetSettingsByCompanyAsync(selectedId))
                .Where(s => !s.Key.StartsWith(SiteTheme.KeyPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var model = new CmsSiteSettingListViewModel
            {
                SelectedCompanyId = selectedId,
                Companies = companies,
                Settings = settings,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                })
            };

            return View(model);
        }

        [HttpPost("save-batch")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBatch([FromForm] CmsSiteSettingBatchUpdateViewModel model)
        {
            if (model.CompanyProfileId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid company profile.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            var dtos = model.Settings
                .Where(s => !(s.Key ?? string.Empty).Trim().StartsWith(SiteTheme.KeyPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(s => new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Key = s.Key,
                ValueEn = s.ValueEn,
                ValueAr = s.ValueAr,
                SettingType = s.SettingType,
                GroupName = s.GroupName,
                Label = s.Label
            });

            var result = await _settingService.SaveBatchAsync(model.CompanyProfileId, dtos, userId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to save settings.";
            }
            else
            {
                TempData["SuccessMessage"] = "Site settings updated successfully.";
            }

            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var model = new CmsSiteSettingFormViewModel
            {
                CompanyProfileId = selectedId
            };

            PopulateFormOptions(model, companies, selectedId);
            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsSiteSettingFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateFormOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Key = model.Key,
                ValueEn = model.ValueEn,
                ValueAr = model.ValueAr,
                SettingType = model.SettingType,
                GroupName = model.GroupName,
                Label = model.Label
            };

            var userId = GetCurrentUserId();
            var result = await _settingService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create setting.");
                await RepopulateFormOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Setting '{model.Key}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _settingService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete setting.";
            }
            else
            {
                TempData["SuccessMessage"] = "Setting deleted successfully.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private void PopulateFormOptions(CmsSiteSettingFormViewModel model, IReadOnlyList<CmsCompanyProfileSummaryDto> companies, Guid? selectedId)
        {
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == selectedId
            });

            model.SettingTypeOptions = new[]
            {
                new SelectListItem("Text", "text"),
                new SelectListItem("Textarea", "textarea"),
                new SelectListItem("HTML", "html"),
                new SelectListItem("URL", "url"),
                new SelectListItem("Boolean", "boolean")
            };

            model.GroupOptions = new[]
            {
                new SelectListItem("General", "General"),
                new SelectListItem("SEO", "SEO"),
                new SelectListItem("Contact", "Contact"),
                new SelectListItem("Footer", "Footer")
            };
        }

        private async Task RepopulateFormOptionsAsync(CmsSiteSettingFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            PopulateFormOptions(model, companies, model.CompanyProfileId);
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
