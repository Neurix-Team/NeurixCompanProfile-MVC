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
    [Route("cms/blog")]
    public class CmsBlogPostsController : Controller
    {
        private readonly ICmsBlogPostService _blogService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsBlogPostsController> _logger;

        public CmsBlogPostsController(
            ICmsBlogPostService blogService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsBlogPostsController> logger)
        {
            _blogService = blogService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var posts = await _blogService.GetBlogPostsByCompanyAsync(companyId, includeUnpublished: true);

            var model = new CmsBlogPostListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                Posts = posts
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsBlogPostFormViewModel
            {
                CompanyProfileId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})"
                }),
                IsPublished = true,
                ReadTimeMinutes = 5
            };

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsBlogPostFormViewModel model)
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
                    coverPath = await CmsMediaUploadHelper.SaveImageAsync(model.CoverImageFile, _environment, "blog");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.CoverImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsBlogPostUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Slug = model.Slug,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                SummaryEn = model.SummaryEn,
                SummaryAr = model.SummaryAr,
                BodyEn = model.BodyEn,
                BodyAr = model.BodyAr,
                CoverImagePath = coverPath,
                Category = model.Category,
                AuthorNameEn = model.AuthorNameEn,
                AuthorNameAr = model.AuthorNameAr,
                Tags = model.Tags,
                ReadTimeMinutes = model.ReadTimeMinutes,
                PublishedAtUtc = model.PublishedAtUtc,
                IsFeatured = model.IsFeatured,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _blogService.CreateAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create article.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Article '{model.TitleEn}' created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var post = await _blogService.GetByIdAsync(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Article not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsBlogPostFormViewModel
            {
                Id = post.Id,
                CompanyProfileId = post.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == post.CompanyProfileId
                }),
                Slug = post.Slug,
                TitleEn = post.TitleEn,
                TitleAr = post.TitleAr,
                SummaryEn = post.SummaryEn,
                SummaryAr = post.SummaryAr,
                BodyEn = post.BodyEn,
                BodyAr = post.BodyAr,
                CoverImagePath = post.CoverImagePath,
                Category = post.Category,
                AuthorNameEn = post.AuthorNameEn,
                AuthorNameAr = post.AuthorNameAr,
                Tags = post.Tags,
                ReadTimeMinutes = post.ReadTimeMinutes,
                PublishedAtUtc = post.PublishedAtUtc,
                IsFeatured = post.IsFeatured,
                IsPublished = post.IsPublished
            };

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsBlogPostFormViewModel model)
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
                    coverPath = await CmsMediaUploadHelper.SaveImageAsync(model.CoverImageFile, _environment, "blog");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.CoverImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsBlogPostUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Slug = model.Slug,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                SummaryEn = model.SummaryEn,
                SummaryAr = model.SummaryAr,
                BodyEn = model.BodyEn,
                BodyAr = model.BodyAr,
                CoverImagePath = coverPath,
                Category = model.Category,
                AuthorNameEn = model.AuthorNameEn,
                AuthorNameAr = model.AuthorNameAr,
                Tags = model.Tags,
                ReadTimeMinutes = model.ReadTimeMinutes,
                PublishedAtUtc = model.PublishedAtUtc,
                IsFeatured = model.IsFeatured,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _blogService.UpdateAsync(id, dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update article.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Article '{model.TitleEn}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var post = await _blogService.GetByIdAsync(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Article not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsBlogPostDetailsViewModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _blogService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Article published." : "Article unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _blogService.SoftDeleteAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete article.";
            }
            else
            {
                TempData["SuccessMessage"] = "Article deleted.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateCompanyOptionsAsync(CmsBlogPostFormViewModel model)
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
