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
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/social-links")]
    public class CmsSocialLinksController : Controller
    {
        private readonly ICmsSocialLinkService _socialLinkService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly ILogger<CmsSocialLinksController> _logger;

        public CmsSocialLinksController(
            ICmsSocialLinkService socialLinkService,
            ICmsCompanyProfileService profileService,
            ILogger<CmsSocialLinksController> logger)
        {
            _socialLinkService = socialLinkService;
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var links = await _socialLinkService.GetLinksByCompanyAsync(companyId, includeUnpublished: true);

            var model = new CmsSocialLinkListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                SocialLinks = links
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsSocialLinkFormViewModel
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
        public async Task<IActionResult> Create(CmsSocialLinkFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsSocialLinkUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Platform = model.Platform,
                Url = model.Url,
                DisplayName = model.DisplayName,
                IconName = model.IconName,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _socialLinkService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create social link.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Social link '{model.Platform}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var link = await _socialLinkService.GetByIdAsync(id);
            if (link == null)
            {
                TempData["ErrorMessage"] = "Social link not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsSocialLinkFormViewModel
            {
                Id = link.Id,
                CompanyProfileId = link.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == link.CompanyProfileId
                }),
                Platform = link.Platform,
                Url = link.Url,
                DisplayName = link.DisplayName,
                IconName = link.IconName,
                DisplayOrder = link.DisplayOrder,
                IsPublished = link.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsSocialLinkFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsSocialLinkUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Platform = model.Platform,
                Url = model.Url,
                DisplayName = model.DisplayName,
                IconName = model.IconName,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _socialLinkService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update social link.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Social link '{model.Platform}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var link = await _socialLinkService.GetByIdAsync(id);
            if (link == null)
            {
                TempData["ErrorMessage"] = "Social link not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsSocialLinkDetailsViewModel
            {
                SocialLink = link
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _socialLinkService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Social link published." : "Social link unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _socialLinkService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete social link.";
            }
            else
            {
                TempData["SuccessMessage"] = "Social link deleted.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateCompanyOptionsAsync(CmsSocialLinkFormViewModel model)
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
