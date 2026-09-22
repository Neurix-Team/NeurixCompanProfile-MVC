using System;
using System.Collections.Generic;
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
    [Route("cms/divisions")]
    public class CmsDivisionPagesController : Controller
    {
        private readonly ICmsDivisionPageService _divisionService;
        private readonly ICmsPageBandService _pageBandService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsDivisionPagesController> _logger;

        public CmsDivisionPagesController(
            ICmsDivisionPageService divisionService,
            ICmsPageBandService pageBandService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsDivisionPagesController> logger)
        {
            _divisionService = divisionService;
            _pageBandService = pageBandService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var pages = await _divisionService.GetDivisionPagesByCompanyAsync(companyId, includeUnpublished: true);

            var pageItems = new List<CmsDivisionPageItemViewModel>();
            foreach (var p in pages)
            {
                var bands = await _pageBandService.GetBandsByCompanyIdAsync(p.CompanyProfileId, p.Slug);
                var bandKeys = CmsPageKeys.BandsFor(p.Slug);
                var totalItems = 0;
                string? capEditUrl = null;
                string? capTitleEn = null;

                foreach (var bk in bandKeys)
                {
                    var items = await _pageBandService.GetBandItemsByCompanyIdAsync(p.CompanyProfileId, p.Slug, bk);
                    totalItems += items.Count;
                    if (bk == CmsPageKeys.Bands.Cards || (capEditUrl == null && bk != CmsPageKeys.Bands.Hero))
                    {
                        var matchingBand = bands.FirstOrDefault(b => b.BandKey == bk);
                        capTitleEn = matchingBand?.TitlePrefixEn ?? (bk == CmsPageKeys.Bands.Cards ? "Capabilities" : "Page Sections");
                        capEditUrl = $"/cms/page-sections/{p.Slug}/{bk}/edit?companyId={p.CompanyProfileId}";
                    }
                }

                if (capEditUrl == null && bandKeys.Count > 0)
                {
                    capEditUrl = $"/cms/page-sections/{p.Slug}/{bandKeys[0]}/edit?companyId={p.CompanyProfileId}";
                }

                pageItems.Add(new CmsDivisionPageItemViewModel
                {
                    Page = p,
                    LiveBandsCount = bandKeys.Count,
                    TotalItemsCount = totalItems,
                    CapabilitiesBandEditUrl = capEditUrl,
                    CapabilitiesBandTitleEn = capTitleEn
                });
            }

            var model = new CmsDivisionPageListViewModel
            {
                SelectedCompanyId = companyId,
                Companies = companies,
                Pages = pages,
                PageItems = pageItems
            };

            return View(model);
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var page = await _divisionService.GetByIdAsync(id);
            if (page == null)
            {
                TempData["ErrorMessage"] = "Division page not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsDivisionPageFormViewModel
            {
                Id = page.Id,
                CompanyProfileId = page.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == page.CompanyProfileId
                }),
                Slug = page.Slug,
                HeroTitleEn = page.HeroTitleEn,
                HeroTitleAr = page.HeroTitleAr,
                HeroSubtitleEn = page.HeroSubtitleEn,
                HeroSubtitleAr = page.HeroSubtitleAr,
                MissionEn = page.MissionEn,
                MissionAr = page.MissionAr,
                CoverImagePath = page.CoverImagePath,
                IsPublished = page.IsPublished
            };

            await PopulateDivisionBandsAsync(model, page.CompanyProfileId, page.Slug);

            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsDivisionPageFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                await PopulateDivisionBandsAsync(model, model.CompanyProfileId, model.Slug);
                return View(model);
            }

            var coverPath = model.CoverImagePath;
            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                try
                {
                    coverPath = await CmsMediaUploadHelper.SaveImageAsync(model.CoverImageFile, _environment, "divisions");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.CoverImageFile), ex.Message);
                    await PopulateCompanyOptionsAsync(model);
                    await PopulateDivisionBandsAsync(model, model.CompanyProfileId, model.Slug);
                    return View(model);
                }
            }

            var dto = new CmsDivisionPageUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                Slug = model.Slug,
                HeroTitleEn = model.HeroTitleEn,
                HeroTitleAr = model.HeroTitleAr,
                HeroSubtitleEn = model.HeroSubtitleEn,
                HeroSubtitleAr = model.HeroSubtitleAr,
                MissionEn = model.MissionEn,
                MissionAr = model.MissionAr,
                CoverImagePath = coverPath,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _divisionService.UpsertAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update division page.");
                await PopulateCompanyOptionsAsync(model);
                await PopulateDivisionBandsAsync(model, model.CompanyProfileId, model.Slug);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Division page '{model.HeroTitleEn}' updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var page = await _divisionService.GetByIdAsync(id);
            if (page == null)
            {
                TempData["ErrorMessage"] = "Division page not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(page);
        }

        [HttpPost("{id:guid}/toggle-publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(Guid id, bool isPublished, Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _divisionService.SetPublishedStatusAsync(id, isPublished, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update publish status.";
            }
            else
            {
                TempData["SuccessMessage"] = isPublished ? "Division page published." : "Division page unpublished (draft).";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        private async Task PopulateDivisionBandsAsync(CmsDivisionPageFormViewModel model, Guid companyProfileId, string slug)
        {
            try
            {
                var bands = await _pageBandService.GetBandsByCompanyIdAsync(companyProfileId, slug);
                var bandKeys = CmsPageKeys.BandsFor(slug);
                var bandSummaries = new List<CmsDivisionBandSummaryViewModel>();

                foreach (var bk in bandKeys)
                {
                    var band = bands.FirstOrDefault(b => b.BandKey == bk);
                    var items = await _pageBandService.GetBandItemsByCompanyIdAsync(companyProfileId, slug, bk);
                    var label = GetBandLabel(slug, bk);
                    var itemsLabel = GetItemsLabel(slug, bk);

                    bandSummaries.Add(new CmsDivisionBandSummaryViewModel
                    {
                        BandKey = bk,
                        Label = label,
                        ItemsLabel = itemsLabel,
                        IsPublished = band?.IsPublished ?? true,
                        TitlePrefixEn = band?.TitlePrefixEn,
                        TitlePrefixAr = band?.TitlePrefixAr,
                        BodyEn = band?.BodyEn,
                        BodyAr = band?.BodyAr,
                        ImagePath = band?.ImagePath,
                        SupportsItems = items.Count > 0 || bk == CmsPageKeys.Bands.Cards || bk == CmsPageKeys.Bands.Overview || bk == CmsPageKeys.Bands.Hero,
                        ItemCount = items.Count,
                        Items = items.Select(i => new CmsDivisionBandItemSummaryViewModel
                        {
                            Id = i.Id,
                            TitleEn = i.TitleEn,
                            TitleAr = i.TitleAr,
                            DescriptionEn = i.DescriptionEn,
                            DescriptionAr = i.DescriptionAr,
                            IconName = i.IconName,
                            ImagePath = i.ImagePath,
                            DisplayOrder = i.DisplayOrder,
                            IsPublished = i.IsPublished,
                            EditItemUrl = $"/cms/page-sections/{slug}/{bk}/items/{i.Id}/edit?companyId={companyProfileId}"
                        }).ToList(),
                        EditBandUrl = $"/cms/page-sections/{slug}/{bk}/edit?companyId={companyProfileId}",
                        CreateItemUrl = $"/cms/page-sections/{slug}/{bk}/items/create?companyId={companyProfileId}"
                    });
                }

                model.LiveBands = bandSummaries;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load live page bands for division '{Slug}'.", slug);
            }
        }

        private static string GetBandLabel(string pageKey, string bandKey) => (pageKey.ToLowerInvariant(), bandKey.ToLowerInvariant()) switch
        {
            ("labs", "hero") => "Hero Chips Band",
            ("labs", "overview") => "Research Core Band",
            ("labs", "cards") => "Laboratory Capabilities Band",
            ("technology", "hero") => "Hero Chips Band",
            ("technology", "overview") => "What We Build Band",
            ("technology", "cta") => "Closing Call to Action",
            ("hq", "hero") => "Hero Chips Band",
            ("hq", "overview") => "Central Hub Band",
            ("hq", "cards") => "Core Functions Band",
            ("plus", "hero") => "Hero Chips Band",
            ("plus", "overview") => "Scope Band",
            ("plus", "cards") => "Partnership Pathways Band",
            ("club", "hero") => "Hero Chips Band",
            ("club", "overview") => "Find Your Path Band",
            ("club", "cards") => "Membership Offer Band",
            _ => $"{char.ToUpper(bandKey[0])}{bandKey.Substring(1)} Band"
        };

        private static string GetItemsLabel(string pageKey, string bandKey) => (pageKey.ToLowerInvariant(), bandKey.ToLowerInvariant()) switch
        {
            ("labs", "cards") => "Capability cards",
            ("labs", "overview") => "Gallery images",
            ("hq", "cards") => "Function cards",
            ("plus", "cards") => "Pathway cards",
            ("plus", "overview") => "Scope cards",
            ("club", "cards") => "Offer cards",
            ("club", "overview") => "Audience cards",
            ("technology", "overview") => "Build cards",
            _ => "Items"
        };

        private async Task PopulateCompanyOptionsAsync(CmsDivisionPageFormViewModel model)
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
