using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    [Route("cms/media")]
    public class CmsMediaAssetsController : Controller
    {
        private readonly ICmsMediaAssetService _mediaService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsMediaAssetsController> _logger;

        public CmsMediaAssetsController(
            ICmsMediaAssetService mediaService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsMediaAssetsController> logger)
        {
            _mediaService = mediaService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId, [FromQuery] string? category)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var assets = await _mediaService.GetAssetsByCompanyAsync(companyId, category);
            var usageCounts = await _mediaService.GetUsageCountsAsync(companyId, category);

            var categories = new[] { "All", "General", "Logos", "Team", "Blog", "Projects", "Banners" };

            var model = new CmsMediaAssetListViewModel
            {
                SelectedCompanyId = companyId,
                SelectedCategory = category,
                Companies = companies,
                Assets = assets,
                UsageCounts = usageCounts,
                CategoryOptions = categories.Select(c => new SelectListItem
                {
                    Value = c == "All" ? "" : c,
                    Text = c,
                    Selected = (string.IsNullOrWhiteSpace(category) && c == "All") || category == c
                })
            };

            return View(model);
        }

        [HttpPost("upload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(CmsMediaAssetUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
            }

            try
            {
                var folder = string.IsNullOrWhiteSpace(model.Category) ? "media" : model.Category.ToLowerInvariant();
                var savedPath = await CmsMediaUploadHelper.SaveImageAsync(model.File, _environment, $"media/{folder}");

                if (string.IsNullOrEmpty(savedPath))
                {
                    TempData["ErrorMessage"] = "Failed to upload file.";
                    return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
                }

                var userId = GetCurrentUserId();
                var dto = new CmsMediaAssetCreateDto
                {
                    CompanyProfileId = model.CompanyProfileId,
                    FileName = Path.GetFileName(savedPath),
                    OriginalFileName = model.File.FileName,
                    FilePath = savedPath,
                    FileSizeBytes = model.File.Length,
                    ContentType = model.File.ContentType,
                    AltTextEn = model.AltTextEn,
                    AltTextAr = model.AltTextAr,
                    Category = model.Category ?? "General",
                    Tags = model.Tags
                };

                var result = await _mediaService.CreateAsync(dto, userId);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Asset '{model.File.FileName}' uploaded to library.";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to save asset metadata.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload media asset.");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId, [FromForm] bool force = false)
        {
            var userId = GetCurrentUserId();
            var result = await _mediaService.SoftDeleteAsync(id, userId, force);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete media asset.";
            }
            else
            {
                TempData["SuccessMessage"] = "Asset removed from library.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpGet("{id:guid}/usages")]
        public async Task<IActionResult> Usages(Guid id)
        {
            var usages = await _mediaService.GetUsagesAsync(id);
            var result = usages.Select(u => new
            {
                entityType = u.EntityType,
                entityLabel = u.EntityLabel,
                entityId = u.EntityId,
                propertyName = u.PropertyName,
                controllerName = u.ControllerName,
                actionName = u.ActionName,
                editUrl = GetEditUrl(u)
            });

            return Json(result);
        }

        private string GetEditUrl(CmsMediaAssetUsageDto usage)
        {
            if (usage.ControllerName == "CmsHomeSections")
            {
                return Url.Action(usage.ActionName, "CmsHomeSections", new { companyId = usage.CompanyProfileId }) ?? "#";
            }

            return Url.Action(usage.ActionName, usage.ControllerName, new { id = usage.EntityId }) ?? "#";
        }

        [HttpGet("browse-api")]
        public async Task<IActionResult> BrowseApi([FromQuery] Guid? companyId, [FromQuery] string? category)
        {
            var assets = await _mediaService.GetAssetsByCompanyAsync(companyId, category);
            return Json(assets);
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
