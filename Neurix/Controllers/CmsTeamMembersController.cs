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
    [Route("cms/team")]
    public class CmsTeamMembersController : Controller
    {
        private readonly ICmsTeamMemberService _teamService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsTeamMembersController> _logger;

        public CmsTeamMembersController(
            ICmsTeamMemberService teamService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsTeamMembersController> logger)
        {
            _teamService = teamService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var members = await _teamService.GetTeamMembersByCompanyAsync(companyId, includeUnpublished: true);

            var model = new CmsTeamMemberListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                TeamMembers = members
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsTeamMemberFormViewModel
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
        public async Task<IActionResult> Create(CmsTeamMemberFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var photoPath = model.PhotoPath;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                try
                {
                    photoPath = await CmsMediaUploadHelper.SaveImageAsync(model.PhotoFile, _environment, "team");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.PhotoFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsTeamMemberUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                BioEn = model.BioEn,
                BioAr = model.BioAr,
                PhotoPath = photoPath,
                LinkedInUrl = model.LinkedInUrl,
                Email = model.Email,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _teamService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create team member.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Team member '{model.NameEn}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var member = await _teamService.GetByIdAsync(id);
            if (member == null)
            {
                TempData["ErrorMessage"] = "Team member not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsTeamMemberFormViewModel
            {
                Id = member.Id,
                CompanyProfileId = member.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == member.CompanyProfileId
                }),
                NameEn = member.NameEn,
                NameAr = member.NameAr,
                TitleEn = member.TitleEn,
                TitleAr = member.TitleAr,
                BioEn = member.BioEn,
                BioAr = member.BioAr,
                PhotoPath = member.PhotoPath,
                LinkedInUrl = member.LinkedInUrl,
                Email = member.Email,
                DisplayOrder = member.DisplayOrder,
                IsPublished = member.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsTeamMemberFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var photoPath = model.PhotoPath;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                try
                {
                    photoPath = await CmsMediaUploadHelper.SaveImageAsync(model.PhotoFile, _environment, "team");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.PhotoFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsTeamMemberUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                NameEn = model.NameEn,
                NameAr = model.NameAr,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                BioEn = model.BioEn,
                BioAr = model.BioAr,
                PhotoPath = photoPath,
                LinkedInUrl = model.LinkedInUrl,
                Email = model.Email,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _teamService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update team member.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Team member '{model.NameEn}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var member = await _teamService.GetByIdAsync(id);
            if (member == null)
            {
                TempData["ErrorMessage"] = "Team member not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsTeamMemberDetailsViewModel
            {
                Member = member
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _teamService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Team member published." : "Team member unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _teamService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete team member.";
            }
            else
            {
                TempData["SuccessMessage"] = "Team member deleted.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateCompanyOptionsAsync(CmsTeamMemberFormViewModel model)
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
