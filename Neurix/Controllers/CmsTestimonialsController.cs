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
    [Route("cms/testimonials")]
    public class CmsTestimonialsController : Controller
    {
        private readonly ICmsTestimonialService _testimonialService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsTestimonialsController> _logger;

        public CmsTestimonialsController(
            ICmsTestimonialService testimonialService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsTestimonialsController> logger)
        {
            _testimonialService = testimonialService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var testimonials = await _testimonialService.GetTestimonialsByCompanyAsync(companyId, includeUnpublished: true);

            var model = new CmsTestimonialListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                Testimonials = testimonials
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsTestimonialFormViewModel
            {
                CompanyProfileId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})"
                }),
                IsPublished = true,
                IsFeatured = true,
                Rating = 5,
                DisplayOrder = 1
            };

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsTestimonialFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var photoPath = model.AuthorPhotoPath;
            if (model.AuthorPhotoFile != null && model.AuthorPhotoFile.Length > 0)
            {
                try
                {
                    photoPath = await CmsMediaUploadHelper.SaveImageAsync(model.AuthorPhotoFile, _environment, "testimonials");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.AuthorPhotoFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsTestimonialUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                CompanyName = model.CompanyName,
                AuthorPhotoPath = photoPath,
                QuoteEn = model.QuoteEn,
                QuoteAr = model.QuoteAr,
                Rating = model.Rating,
                DisplayOrder = model.DisplayOrder,
                IsFeatured = model.IsFeatured,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _testimonialService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create testimonial.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Testimonial by '{model.NameEn}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var testimonial = await _testimonialService.GetByIdAsync(id);
            if (testimonial == null)
            {
                TempData["ErrorMessage"] = "Testimonial not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsTestimonialFormViewModel
            {
                Id = testimonial.Id,
                CompanyProfileId = testimonial.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == testimonial.CompanyProfileId
                }),
                NameEn = testimonial.AuthorNameEn,
                NameAr = testimonial.AuthorNameAr,
                TitleEn = testimonial.AuthorTitleEn,
                TitleAr = testimonial.AuthorTitleAr,
                CompanyName = testimonial.CompanyName,
                AuthorPhotoPath = testimonial.AuthorPhotoPath,
                QuoteEn = testimonial.QuoteEn,
                QuoteAr = testimonial.QuoteAr,
                Rating = testimonial.Rating,
                DisplayOrder = testimonial.DisplayOrder,
                IsFeatured = testimonial.IsFeatured,
                IsPublished = testimonial.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsTestimonialFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var photoPath = model.AuthorPhotoPath;
            if (model.AuthorPhotoFile != null && model.AuthorPhotoFile.Length > 0)
            {
                try
                {
                    photoPath = await CmsMediaUploadHelper.SaveImageAsync(model.AuthorPhotoFile, _environment, "testimonials");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.AuthorPhotoFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsTestimonialUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                CompanyName = model.CompanyName,
                AuthorPhotoPath = photoPath,
                QuoteEn = model.QuoteEn,
                QuoteAr = model.QuoteAr,
                Rating = model.Rating,
                DisplayOrder = model.DisplayOrder,
                IsFeatured = model.IsFeatured,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _testimonialService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update testimonial.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Testimonial by '{model.NameEn}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var testimonial = await _testimonialService.GetByIdAsync(id);
            if (testimonial == null)
            {
                TempData["ErrorMessage"] = "Testimonial not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsTestimonialDetailsViewModel
            {
                Testimonial = testimonial
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _testimonialService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Testimonial published." : "Testimonial unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _testimonialService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete testimonial.";
            }
            else
            {
                TempData["SuccessMessage"] = "Testimonial deleted.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateCompanyOptionsAsync(CmsTestimonialFormViewModel model)
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
