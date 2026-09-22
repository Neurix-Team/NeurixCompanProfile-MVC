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
    [Route("cms/projects")]
    public class CmsProjectsController : Controller
    {
        private readonly ICmsProjectService _projectService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsProjectsController> _logger;

        public CmsProjectsController(
            ICmsProjectService projectService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsProjectsController> logger)
        {
            _projectService = projectService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var projects = await _projectService.GetProjectsByCompanyAsync(companyId, includeUnpublished: true);

            var model = new CmsProjectListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                Projects = projects
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsProjectFormViewModel
            {
                CompanyProfileId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})"
                }),
                IsPublished = true,
                IsFeatured = true,
                DisplayOrder = 1
            };

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsProjectFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var coverPath = model.CoverImagePath;
            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                try
                {
                    coverPath = await CmsMediaUploadHelper.SaveImageAsync(model.CoverImageFile, _environment, "projects");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.CoverImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsProjectUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Slug = model.Slug,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                SummaryEn = model.SummaryEn,
                SummaryAr = model.SummaryAr,
                DescriptionEn = model.DescriptionEn,
                DescriptionAr = model.DescriptionAr,
                CoverImagePath = coverPath,
                Category = model.Category,
                ClientName = model.ClientName,
                TechnologiesUsed = model.TechnologiesUsed,
                ProjectUrl = model.ProjectUrl,
                GithubUrl = model.GithubUrl,
                CompletedAtUtc = model.CompletedAtUtc,
                DisplayOrder = model.DisplayOrder,
                IsFeatured = model.IsFeatured,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _projectService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create project.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Project '{model.TitleEn}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsProjectFormViewModel
            {
                Id = project.Id,
                CompanyProfileId = project.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == project.CompanyProfileId
                }),
                Slug = project.Slug,
                TitleEn = project.TitleEn,
                TitleAr = project.TitleAr,
                SummaryEn = project.SummaryEn,
                SummaryAr = project.SummaryAr,
                DescriptionEn = project.DescriptionEn,
                DescriptionAr = project.DescriptionAr,
                CoverImagePath = project.CoverImagePath,
                Category = project.Category,
                ClientName = project.ClientName,
                TechnologiesUsed = project.TechnologiesUsed,
                ProjectUrl = project.ProjectUrl,
                GithubUrl = project.GithubUrl,
                CompletedAtUtc = project.CompletedAtUtc,
                DisplayOrder = project.DisplayOrder,
                IsFeatured = project.IsFeatured,
                IsPublished = project.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsProjectFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var coverPath = model.CoverImagePath;
            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                try
                {
                    coverPath = await CmsMediaUploadHelper.SaveImageAsync(model.CoverImageFile, _environment, "projects");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.CoverImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsProjectUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Slug = model.Slug,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                SummaryEn = model.SummaryEn,
                SummaryAr = model.SummaryAr,
                DescriptionEn = model.DescriptionEn,
                DescriptionAr = model.DescriptionAr,
                CoverImagePath = coverPath,
                Category = model.Category,
                ClientName = model.ClientName,
                TechnologiesUsed = model.TechnologiesUsed,
                ProjectUrl = model.ProjectUrl,
                GithubUrl = model.GithubUrl,
                CompletedAtUtc = model.CompletedAtUtc,
                DisplayOrder = model.DisplayOrder,
                IsFeatured = model.IsFeatured,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _projectService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update project.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Project '{model.TitleEn}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsProjectDetailsViewModel
            {
                Project = project
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _projectService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Project published." : "Project unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _projectService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete project.";
            }
            else
            {
                TempData["SuccessMessage"] = "Project deleted.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateCompanyOptionsAsync(CmsProjectFormViewModel model)
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
