using System;
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
    [Route("cms/services")]
    public class CmsServicesController : Controller
    {
        private readonly ICmsServiceService _serviceService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsServicesController> _logger;

        public CmsServicesController(
            ICmsServiceService serviceService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsServicesController> logger)
        {
            _serviceService = serviceService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var services = await _serviceService.GetServicesByCompanyAsync(companyId, includeUnpublished: true);

            var model = new CmsServiceListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                Services = services
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsServiceFormViewModel
            {
                CompanyProfileId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})"
                }),
                IsPublished = true,
                DisplayOrder = 1
            };

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsServiceFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var imagePath = model.ImagePath;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    imagePath = await CmsMediaUploadHelper.SaveImageAsync(model.ImageFile, _environment, "services");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsServiceUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                Slug = model.Slug,
                ShortDescriptionEn = model.ShortDescriptionEn,
                ShortDescriptionAr = model.ShortDescriptionAr,
                DescriptionEn = model.DescriptionEn,
                DescriptionAr = model.DescriptionAr,
                IconName = model.IconName,
                ImagePath = imagePath,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _serviceService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create service.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Service '{model.NameEn}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Service not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsServiceFormViewModel
            {
                Id = service.Id,
                CompanyProfileId = service.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == service.CompanyProfileId
                }),
                NameEn = service.NameEn,
                NameAr = service.NameAr,
                Slug = service.Slug,
                ShortDescriptionEn = service.ShortDescriptionEn,
                ShortDescriptionAr = service.ShortDescriptionAr,
                DescriptionEn = service.DescriptionEn,
                DescriptionAr = service.DescriptionAr,
                IconName = service.IconName,
                ImagePath = service.ImagePath,
                DisplayOrder = service.DisplayOrder,
                IsPublished = service.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsServiceFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var imagePath = model.ImagePath;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    imagePath = await CmsMediaUploadHelper.SaveImageAsync(model.ImageFile, _environment, "services");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsServiceUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                Slug = model.Slug,
                ShortDescriptionEn = model.ShortDescriptionEn,
                ShortDescriptionAr = model.ShortDescriptionAr,
                DescriptionEn = model.DescriptionEn,
                DescriptionAr = model.DescriptionAr,
                IconName = model.IconName,
                ImagePath = imagePath,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _serviceService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update service.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Service '{model.NameEn}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Service not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsServiceDetailsViewModel
            {
                Service = service
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Service published." : "Service unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete service.";
            }
            else
            {
                TempData["SuccessMessage"] = "Service deleted.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateCompanyOptionsAsync(CmsServiceFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
