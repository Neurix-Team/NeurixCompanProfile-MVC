using System;
using System.Collections.Generic;
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
    /// <summary>
    /// One screen for every standalone page (About, Privacy, Contact, ...). There is no
    /// Create action on purpose: pages are seeded per brand and the public routes are fixed,
    /// so the CMS only edits them. Adding a page means seeding a row, not a migration.
    /// </summary>
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/content-pages")]
    public class CmsContentPagesController : Controller
    {
        private readonly ICmsContentPageService _contentPageService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly ICmsPageBandService? _pageBandService;
        private readonly ILogger<CmsContentPagesController> _logger;

        public CmsContentPagesController(
            ICmsContentPageService contentPageService,
            ICmsCompanyProfileService profileService,
            ILogger<CmsContentPagesController> logger,
            ICmsPageBandService? pageBandService = null)
        {
            _contentPageService = contentPageService;
            _profileService = profileService;
            _logger = logger;
            _pageBandService = pageBandService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id;

            var pageDtos = selectedId.HasValue && selectedId.Value != Guid.Empty
                ? await _contentPageService.GetAllByCompanyAsync(selectedId)
                : Array.Empty<CmsContentPageSummaryDto>();

            var pageItems = new List<CmsContentPageItemViewModel>();
            foreach (var p in pageDtos)
            {
                var itemVm = new CmsContentPageItemViewModel
                {
                    Id = p.Id,
                    CompanyProfileId = p.CompanyProfileId,
                    CompanyNameEn = p.CompanyNameEn,
                    Slug = p.Slug,
                    TitleEn = p.TitleEn,
                    TitleAr = p.TitleAr,
                    HasBody = p.HasBody,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                };

                if (p.Slug.Equals("about", StringComparison.OrdinalIgnoreCase) && _pageBandService != null)
                {
                    try
                    {
                        var aboutBands = await _pageBandService.GetBandsByCompanyIdAsync(p.CompanyProfileId, "about");
                        itemVm.LiveBandsCount = aboutBands.Count;
                        int totalItems = 0;
                        foreach (var b in aboutBands)
                        {
                            var bandItems = await _pageBandService.GetBandItemsByCompanyIdAsync(p.CompanyProfileId, "about", b.BandKey);
                            totalItems += bandItems.Count;
                        }
                        itemVm.TotalItemsCount = totalItems;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load about bands count.");
                    }
                }

                pageItems.Add(itemVm);
            }

            var model = new CmsContentPageListViewModel
            {
                SelectedCompanyId = selectedId,
                Companies = companies,
                Pages = pageItems
            };

            return View(model);
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var page = await _contentPageService.GetByIdAsync(id);
            if (page == null)
            {
                TempData["ErrorMessage"] = "Content page not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CmsContentPageFormViewModel
            {
                Id = page.Id,
                CompanyProfileId = page.CompanyProfileId,
                Slug = page.Slug,
                TitleEn = page.TitleEn,
                TitleAr = page.TitleAr,
                MetaDescriptionEn = page.MetaDescriptionEn,
                MetaDescriptionAr = page.MetaDescriptionAr,
                HeroBadgeEn = page.HeroBadgeEn,
                HeroBadgeAr = page.HeroBadgeAr,
                HeroTitlePrefixEn = page.HeroTitlePrefixEn,
                HeroTitlePrefixAr = page.HeroTitlePrefixAr,
                HeroTitleHighlightEn = page.HeroTitleHighlightEn,
                HeroTitleHighlightAr = page.HeroTitleHighlightAr,
                HeroSubtitleEn = page.HeroSubtitleEn,
                HeroSubtitleAr = page.HeroSubtitleAr,
                BodyEn = page.BodyEn,
                BodyAr = page.BodyAr,
                MissionTitleEn = page.MissionTitleEn,
                MissionTitleAr = page.MissionTitleAr,
                MissionTextEn = page.MissionTextEn,
                MissionTextAr = page.MissionTextAr,
                VisionTitleEn = page.VisionTitleEn,
                VisionTitleAr = page.VisionTitleAr,
                VisionTextEn = page.VisionTextEn,
                VisionTextAr = page.VisionTextAr,
                CtaBadgeEn = page.CtaBadgeEn,
                CtaBadgeAr = page.CtaBadgeAr,
                CtaTitleEn = page.CtaTitleEn,
                CtaTitleAr = page.CtaTitleAr,
                CtaSubtitleEn = page.CtaSubtitleEn,
                CtaSubtitleAr = page.CtaSubtitleAr,
                CtaButtonTextEn = page.CtaButtonTextEn,
                CtaButtonTextAr = page.CtaButtonTextAr,
                CtaButtonUrl = page.CtaButtonUrl,
                ContactEmail = page.ContactEmail,
                IsPublished = page.IsPublished
            };

            await PopulateCompanyOptionsAsync(model);
            await PopulateContentPageBandsAsync(model, page.CompanyProfileId, page.Slug);
            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsContentPageFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                await PopulateContentPageBandsAsync(model, model.CompanyProfileId, model.Slug);
                return View(model);
            }

            var dto = new CmsContentPageUpsertDto
            {
                Id = id,
                CompanyProfileId = model.CompanyProfileId,
                Slug = model.Slug,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                MetaDescriptionEn = model.MetaDescriptionEn,
                MetaDescriptionAr = model.MetaDescriptionAr,
                HeroBadgeEn = model.HeroBadgeEn,
                HeroBadgeAr = model.HeroBadgeAr,
                HeroTitlePrefixEn = model.HeroTitlePrefixEn,
                HeroTitlePrefixAr = model.HeroTitlePrefixAr,
                HeroTitleHighlightEn = model.HeroTitleHighlightEn,
                HeroTitleHighlightAr = model.HeroTitleHighlightAr,
                HeroSubtitleEn = model.HeroSubtitleEn,
                HeroSubtitleAr = model.HeroSubtitleAr,
                BodyEn = model.BodyEn,
                BodyAr = model.BodyAr,
                MissionTitleEn = model.MissionTitleEn,
                MissionTitleAr = model.MissionTitleAr,
                MissionTextEn = model.MissionTextEn,
                MissionTextAr = model.MissionTextAr,
                VisionTitleEn = model.VisionTitleEn,
                VisionTitleAr = model.VisionTitleAr,
                VisionTextEn = model.VisionTextEn,
                VisionTextAr = model.VisionTextAr,
                CtaBadgeEn = model.CtaBadgeEn,
                CtaBadgeAr = model.CtaBadgeAr,
                CtaTitleEn = model.CtaTitleEn,
                CtaTitleAr = model.CtaTitleAr,
                CtaSubtitleEn = model.CtaSubtitleEn,
                CtaSubtitleAr = model.CtaSubtitleAr,
                CtaButtonTextEn = model.CtaButtonTextEn,
                CtaButtonTextAr = model.CtaButtonTextAr,
                CtaButtonUrl = model.CtaButtonUrl,
                ContactEmail = model.ContactEmail,
                IsPublished = model.IsPublished
            };

            var result = await _contentPageService.UpsertAsync(dto, GetCurrentUserId());

            if (!result.Success)
            {
                _logger.LogWarning("Content page upsert failed for slug {Slug}: {Error}", model.Slug, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save the content page.");
                await PopulateCompanyOptionsAsync(model);
                await PopulateContentPageBandsAsync(model, model.CompanyProfileId, model.Slug);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Page '{model.Slug}' saved.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        private async Task PopulateContentPageBandsAsync(CmsContentPageFormViewModel model, Guid companyProfileId, string slug)
        {
            if (!slug.Equals("about", StringComparison.OrdinalIgnoreCase) || _pageBandService == null)
            {
                return;
            }

            try
            {
                var bands = await _pageBandService.GetBandsByCompanyIdAsync(companyProfileId, "about");
                var bandSummaries = new List<CmsDivisionBandSummaryViewModel>();

                foreach (var bk in CmsPageKeys.BandsFor("about"))
                {
                    var band = bands.FirstOrDefault(b => b.BandKey.Equals(bk, StringComparison.OrdinalIgnoreCase));
                    var items = await _pageBandService.GetBandItemsByCompanyIdAsync(companyProfileId, "about", bk);

                    var label = bk switch
                    {
                        CmsPageKeys.Bands.Principles => "What We Believe In (Principles) Band",
                        CmsPageKeys.Bands.Structure => "Five Pillars, One Vision (Structure) Band",
                        CmsPageKeys.Bands.Team => "Leadership Band",
                        _ => $"{char.ToUpper(bk[0])}{bk.Substring(1)} Band"
                    };

                    var itemsLabel = bk switch
                    {
                        CmsPageKeys.Bands.Principles => "Principle cards",
                        CmsPageKeys.Bands.Structure => "Subsidiary cards",
                        _ => "Items"
                    };

                    bandSummaries.Add(new CmsDivisionBandSummaryViewModel
                    {
                        BandKey = bk,
                        Label = label,
                        ItemsLabel = itemsLabel,
                        IsPublished = band?.IsPublished ?? true,
                        BadgeEn = band?.BadgeEn,
                        BadgeAr = band?.BadgeAr,
                        TitlePrefixEn = band?.TitlePrefixEn,
                        TitlePrefixAr = band?.TitlePrefixAr,
                        TitleHighlightEn = band?.TitleHighlightEn,
                        TitleHighlightAr = band?.TitleHighlightAr,
                        BodyEn = band?.BodyEn,
                        BodyAr = band?.BodyAr,
                        ImagePath = band?.ImagePath,
                        SupportsItems = bk == CmsPageKeys.Bands.Principles || bk == CmsPageKeys.Bands.Structure,
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
                            LinkUrl = i.LinkUrl,
                            DisplayOrder = i.DisplayOrder,
                            IsPublished = i.IsPublished,
                            EditItemUrl = $"/cms/page-sections/about/{bk}/items/{i.Id}/edit?companyId={companyProfileId}"
                        }).ToList(),
                        EditBandUrl = $"/cms/page-sections/about/{bk}/edit?companyId={companyProfileId}",
                        CreateItemUrl = $"/cms/page-sections/about/{bk}/items/create?companyId={companyProfileId}"
                    });
                }

                model.LiveBands = bandSummaries;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load live page bands for content page '{Slug}'.", slug);
            }
        }

        private async Task PopulateCompanyOptionsAsync(CmsContentPageFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            }).ToList();
        }

        private Guid? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
