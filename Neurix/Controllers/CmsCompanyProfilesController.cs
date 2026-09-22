using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.Common;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/companies")]
    public class CmsCompanyProfilesController : Controller
    {
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsCompanyProfilesController> _logger;

        public CmsCompanyProfilesController(
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsCompanyProfilesController> logger)
        {
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var profiles = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsCompanyProfileListViewModel
            {
                Profiles = profiles
            };
            return View(model);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            var model = new CmsCompanyProfileFormViewModel
            {
                IsPublished = true
            };
            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsCompanyProfileFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var logoPath = model.LogoPath;
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                try
                {
                    logoPath = await CmsMediaUploadHelper.SaveImageAsync(model.LogoFile, _environment, "logos");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.LogoFile), ex.Message);
                    return View(model);
                }
            }

            var faviconPath = model.FaviconPath;
            if (model.FaviconFile != null && model.FaviconFile.Length > 0)
            {
                try
                {
                    faviconPath = await CmsMediaUploadHelper.SaveImageAsync(model.FaviconFile, _environment, "favicons");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.FaviconFile), ex.Message);
                    return View(model);
                }
            }

            var dto = new CmsCompanyProfileUpsertDto
            {
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                Slug = model.Slug,
                LogoPath = logoPath,
                FaviconPath = faviconPath,
                PrimaryColor = model.PrimaryColor,
                AccentColor = model.AccentColor,
                TaglineEn = model.TaglineEn,
                TaglineAr = model.TaglineAr,
                ShortDescriptionEn = model.ShortDescriptionEn,
                ShortDescriptionAr = model.ShortDescriptionAr,
                DescriptionEn = model.DescriptionEn,
                DescriptionAr = model.DescriptionAr,
                Email = model.Email,
                Phone = model.Phone,
                AddressEn = model.AddressEn,
                AddressAr = model.AddressAr,
                Website = model.Website,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _profileService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create company profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Company profile '{model.NameEn}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var profile = await _profileService.GetByIdAsync(id);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Company profile not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsCompanyProfileFormViewModel
            {
                Id = profile.Id,
                NameEn = profile.NameEn,
                NameAr = profile.NameAr,
                Slug = profile.Slug,
                LogoPath = profile.LogoPath,
                FaviconPath = profile.FaviconPath,
                PrimaryColor = profile.PrimaryColor,
                AccentColor = profile.AccentColor,
                TaglineEn = profile.TaglineEn,
                TaglineAr = profile.TaglineAr,
                ShortDescriptionEn = profile.ShortDescriptionEn,
                ShortDescriptionAr = profile.ShortDescriptionAr,
                DescriptionEn = profile.DescriptionEn,
                DescriptionAr = profile.DescriptionAr,
                Email = profile.Email,
                Phone = profile.Phone,
                AddressEn = profile.AddressEn,
                AddressAr = profile.AddressAr,
                Website = profile.Website,
                IsPublished = profile.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsCompanyProfileFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var logoPath = model.LogoPath;
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                try
                {
                    logoPath = await CmsMediaUploadHelper.SaveImageAsync(model.LogoFile, _environment, "logos");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.LogoFile), ex.Message);
                    return View(model);
                }
            }

            var faviconPath = model.FaviconPath;
            if (model.FaviconFile != null && model.FaviconFile.Length > 0)
            {
                try
                {
                    faviconPath = await CmsMediaUploadHelper.SaveImageAsync(model.FaviconFile, _environment, "favicons");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.FaviconFile), ex.Message);
                    return View(model);
                }
            }

            var dto = new CmsCompanyProfileUpsertDto
            {
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                Slug = model.Slug,
                LogoPath = logoPath,
                FaviconPath = faviconPath,
                PrimaryColor = model.PrimaryColor,
                AccentColor = model.AccentColor,
                TaglineEn = model.TaglineEn,
                TaglineAr = model.TaglineAr,
                ShortDescriptionEn = model.ShortDescriptionEn,
                ShortDescriptionAr = model.ShortDescriptionAr,
                DescriptionEn = model.DescriptionEn,
                DescriptionAr = model.DescriptionAr,
                Email = model.Email,
                Phone = model.Phone,
                AddressEn = model.AddressEn,
                AddressAr = model.AddressAr,
                Website = model.Website,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _profileService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update company profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Company profile '{model.NameEn}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var profile = await _profileService.GetByIdAsync(id);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Company profile not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsCompanyProfileDetailsViewModel
            {
                Profile = profile
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished)
        {
            var userId = GetCurrentUserId();
            var result = await _profileService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Profile published." : "Profile unpublished (draft).";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _profileService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete company profile.";
            }
            else
            {
                TempData["SuccessMessage"] = "Company profile deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
