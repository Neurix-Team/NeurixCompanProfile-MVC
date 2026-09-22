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
    /// <summary>
    /// Edits the bands of the secondary public pages — the five division pages, the AI
    /// capabilities page, the insights and portfolio list pages, and the About page. The
    /// homepage is handled by <see cref="CmsHomeSectionsController"/>; the division pages'
    /// hero title, hero subtitle and mission stay with <see cref="CmsDivisionPagesController"/>.
    /// </summary>
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/page-sections")]
    public class CmsPageBandsController : Controller
    {
        private readonly ICmsPageBandService _pageBandService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsPageBandsController> _logger;

        public CmsPageBandsController(
            ICmsPageBandService pageBandService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsPageBandsController> logger)
        {
            _pageBandService = pageBandService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        // ════════════════════════════════════════════════════════════════
        // DESCRIPTORS
        //
        // One row per band, describing it for the dashboard: what to call it, and which
        // fields the public page actually renders. The form hides everything a band does
        // not use, so an editor is never shown a box that would have no visible effect.
        // ════════════════════════════════════════════════════════════════

        private sealed record PageDescriptor(string Label, string PublicUrl, bool HasDivisionPageRow);

        private sealed record BandDescriptor(
            string Label,
            string ItemsLabel,
            bool Badge = true,
            bool Title = true,
            bool TitleSuffix = false,
            bool Body = true,
            bool Image = false,
            bool Button = false,
            bool ItemLink = false,
            bool EmptyState = false,
            bool Items = false,
            bool ItemIcon = true,
            bool ItemDescription = true,
            bool ItemImage = false,
            bool ItemLinkUrl = false);

        private static readonly IReadOnlyDictionary<string, PageDescriptor> Pages =
            new Dictionary<string, PageDescriptor>(StringComparer.OrdinalIgnoreCase)
            {
                [CmsPageKeys.Labs] = new("Neurix AI Labs", "/Home/Labs", HasDivisionPageRow: true),
                [CmsPageKeys.Technology] = new("Neurix AI Technology", "/Home/Technology", HasDivisionPageRow: true),
                [CmsPageKeys.Hq] = new("Neurix AI HQ", "/Home/HQ", HasDivisionPageRow: true),
                [CmsPageKeys.Plus] = new("Neurix AI Plus", "/Home/Plus", HasDivisionPageRow: true),
                [CmsPageKeys.Club] = new("Neurix AI Club", "/Home/Club", HasDivisionPageRow: true),
                [CmsPageKeys.Ai] = new("AI Capabilities", "/Home/AI", HasDivisionPageRow: false),
                [CmsPageKeys.Insights] = new("Insights", "/Home/Insights", HasDivisionPageRow: false),
                [CmsPageKeys.Portfolio] = new("Portfolio", "/Home/Portfolio", HasDivisionPageRow: false),
                [CmsPageKeys.About] = new("About", "/Home/About", HasDivisionPageRow: false)
            };

        /// <summary>Keyed "page/band" so a band can be described differently on each page.</summary>
        private static readonly IReadOnlyDictionary<string, BandDescriptor> Bands =
            new Dictionary<string, BandDescriptor>(StringComparer.OrdinalIgnoreCase)
            {
                // Division pages: hero badge plus the uppercase chip strip under the subtitle.
                ["labs/hero"] = new("Hero Band", "Hero chips", Title: false, Body: false,
                    Items: true, ItemIcon: false, ItemDescription: false),
                ["technology/hero"] = new("Hero Band", "Hero chips", Title: false, Body: false,
                    Items: true, ItemIcon: false, ItemDescription: false),
                ["hq/hero"] = new("Hero Band", "Hero chips", Title: false, Body: false,
                    Items: true, ItemIcon: false, ItemDescription: false),
                ["plus/hero"] = new("Hero Band", "Hero chips", Title: false, Body: false),
                ["club/hero"] = new("Hero Band", "Hero chips", Title: false, Body: false),

                // The two-column band under the hero.
                ["labs/overview"] = new("Research Core Band", "Band images",
                    Items: true, ItemIcon: false, ItemDescription: false, ItemImage: true),
                ["technology/overview"] = new("What We Build Band", "Build cards",
                    Badge: false, Image: true, Items: true),
                ["hq/overview"] = new("Central Hub Band", "Band items",
                    TitleSuffix: true, Image: true),
                ["plus/overview"] = new("Scope Band", "Scope cards",
                    Badge: false, Image: true, Items: true, ItemIcon: false),
                ["club/overview"] = new("Find Your Path Band", "Audience cards",
                    Image: true, Items: true),

                // The card grid that closes a division page.
                ["labs/cards"] = new("Laboratory Capabilities Band", "Capability cards",
                    Badge: false, Items: true),
                ["hq/cards"] = new("Core Functions Band", "Function cards",
                    Badge: false, Items: true),
                ["plus/cards"] = new("Partnership Pathways Band", "Pathway cards",
                    Badge: false, Items: true, ItemIcon: false),
                ["club/cards"] = new("Membership Offer Band", "Offer cards",
                    Badge: false, Items: true),

                ["technology/cta"] = new("Closing Call to Action", "Band items", Button: true),

                // AI capabilities: one band, and its two buttons are items.
                ["ai/hero"] = new("Hero Band", "Hero buttons",
                    Items: true, ItemIcon: false, ItemDescription: false, ItemLinkUrl: true),

                // List pages: header copy, the per-card link label and the empty-list message.
                ["insights/hero"] = new("Page Header", "Band items",
                    ItemLink: true, EmptyState: true),
                ["portfolio/hero"] = new("Page Header", "Band items",
                    ItemLink: true, EmptyState: true),

                // About: the three bands that complete the About Us page.
                ["about/principles"] = new("What We Believe In (Principles) Band", "Principle cards",
                    Badge: false, Items: true),
                ["about/structure"] = new("Five Pillars, One Vision (Structure) Band", "Subsidiary cards",
                    ItemLink: true, Items: true, ItemLinkUrl: true),
                ["about/team"] = new("Leadership Band", "Band items")
            };

        private static string DescriptorKey(string pageKey, string bandKey) =>
            $"{CmsPageKeys.Normalise(pageKey)}/{CmsPageKeys.Normalise(bandKey)}";

        private static BandDescriptor? BandDescriptorFor(string pageKey, string bandKey) =>
            Bands.TryGetValue(DescriptorKey(pageKey, bandKey), out var descriptor) ? descriptor : null;

        // ════════════════════════════════════════════════════════════════
        // INDEX
        // ════════════════════════════════════════════════════════════════

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            // Both collections are fetched once and matched in memory, so the overview costs
            // two queries rather than one per band.
            var bands = selectedId == Guid.Empty
                ? Array.Empty<CmsPageBandDto>()
                : (await _pageBandService.GetAllBandsByCompanyIdAsync(selectedId)).ToArray();

            var allItems = selectedId == Guid.Empty
                ? Array.Empty<CmsPageBandItemDto>()
                : (await _pageBandService.GetAllBandItemsByCompanyIdAsync(selectedId)).ToArray();

            var pages = new List<CmsPageOverviewViewModel>();
            foreach (var pageKey in CmsPageKeys.All)
            {
                var page = Pages[pageKey];
                var rows = new List<CmsPageBandRowViewModel>();

                foreach (var bandKey in CmsPageKeys.BandsFor(pageKey))
                {
                    var descriptor = BandDescriptorFor(pageKey, bandKey);
                    var band = bands.FirstOrDefault(b => b.PageKey == pageKey && b.BandKey == bandKey);

                    var itemCount = descriptor?.Items == true
                        ? allItems.Count(i => i.PageKey == pageKey && i.BandKey == bandKey)
                        : 0;

                    rows.Add(new CmsPageBandRowViewModel
                    {
                        BandKey = bandKey,
                        Label = descriptor?.Label ?? bandKey,
                        ItemsLabel = descriptor?.ItemsLabel ?? "Band items",
                        Band = band,
                        ItemCount = itemCount,
                        SupportsItems = descriptor?.Items ?? false
                    });
                }

                pages.Add(new CmsPageOverviewViewModel
                {
                    PageKey = pageKey,
                    Label = page.Label,
                    PublicUrl = page.PublicUrl,
                    HasDivisionPageRow = page.HasDivisionPageRow,
                    Bands = rows
                });
            }

            var model = new CmsPageBandsIndexViewModel
            {
                SelectedCompanyId = selectedId,
                Companies = companies,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                Pages = pages
            };

            return View(model);
        }

        // ════════════════════════════════════════════════════════════════
        // BAND EDITOR
        // ════════════════════════════════════════════════════════════════

        [HttpGet("{pageKey}/{bandKey}/edit")]
        public async Task<IActionResult> EditBand(string pageKey, string bandKey, [FromQuery] Guid? companyId)
        {
            var descriptor = BandDescriptorFor(pageKey, bandKey);
            if (descriptor == null || !Pages.TryGetValue(CmsPageKeys.Normalise(pageKey), out var page))
            {
                TempData["ErrorMessage"] = "Unknown page section.";
                return RedirectToAction(nameof(Index));
            }

            var pk = CmsPageKeys.Normalise(pageKey);
            var bk = CmsPageKeys.Normalise(bandKey);

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _pageBandService.GetBandByCompanyIdAsync(selectedId, pk, bk);

            var items = descriptor.Items && selectedId != Guid.Empty
                ? await _pageBandService.GetBandItemsByCompanyIdAsync(selectedId, pk, bk)
                : Array.Empty<CmsPageBandItemDto>();

            var model = new CmsPageBandFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = CompanyOptions(companies, selectedId),
                PageKey = pk,
                BandKey = bk,
                PageLabel = page.Label,
                BandLabel = descriptor.Label,
                PublicUrl = page.PublicUrl,
                ItemsLabel = descriptor.ItemsLabel,
                BadgeEn = existing?.BadgeEn,
                BadgeAr = existing?.BadgeAr,
                TitlePrefixEn = existing?.TitlePrefixEn,
                TitlePrefixAr = existing?.TitlePrefixAr,
                TitleHighlightEn = existing?.TitleHighlightEn,
                TitleHighlightAr = existing?.TitleHighlightAr,
                TitleSuffixEn = existing?.TitleSuffixEn,
                TitleSuffixAr = existing?.TitleSuffixAr,
                BodyEn = existing?.BodyEn,
                BodyAr = existing?.BodyAr,
                ImagePath = existing?.ImagePath,
                ButtonTextEn = existing?.ButtonTextEn,
                ButtonTextAr = existing?.ButtonTextAr,
                ButtonUrl = existing?.ButtonUrl,
                ItemLinkTextEn = existing?.ItemLinkTextEn,
                ItemLinkTextAr = existing?.ItemLinkTextAr,
                EmptyStateEn = existing?.EmptyStateEn,
                EmptyStateAr = existing?.EmptyStateAr,
                DisplayOrder = existing?.DisplayOrder ?? 0,
                IsPublished = existing?.IsPublished ?? true,
                Items = items
            };

            ApplyBandFlags(model, descriptor);
            return View(model);
        }

        [HttpPost("{pageKey}/{bandKey}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBand(string pageKey, string bandKey, CmsPageBandFormViewModel model)
        {
            var descriptor = BandDescriptorFor(pageKey, bandKey);
            if (descriptor == null || !Pages.TryGetValue(CmsPageKeys.Normalise(pageKey), out var page))
            {
                TempData["ErrorMessage"] = "Unknown page section.";
                return RedirectToAction(nameof(Index));
            }

            var pk = CmsPageKeys.Normalise(pageKey);
            var bk = CmsPageKeys.Normalise(bandKey);

            model.PageKey = pk;
            model.BandKey = bk;
            model.PageLabel = page.Label;
            model.BandLabel = descriptor.Label;
            model.PublicUrl = page.PublicUrl;
            model.ItemsLabel = descriptor.ItemsLabel;
            ApplyBandFlags(model, descriptor);

            if (!ModelState.IsValid)
            {
                await RepopulateBandFormAsync(model, descriptor);
                return View(model);
            }

            if (descriptor.Image && model.ImageFile != null)
            {
                try
                {
                    model.ImagePath = await CmsMediaUploadHelper.SaveImageAsync(
                        model.ImageFile,
                        _environment,
                        "page-bands");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload the image for the {BandKey} band on the {PageKey} page.", bk, pk);
                    ModelState.AddModelError(nameof(model.ImageFile), $"Image upload failed: {ex.Message}");
                    await RepopulateBandFormAsync(model, descriptor);
                    return View(model);
                }
            }

            // Fields the band does not render are stored as null rather than silently kept,
            // so the row never carries copy that no page can show.
            var dto = new CmsPageBandUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                PageKey = pk,
                BandKey = bk,
                BadgeEn = descriptor.Badge ? model.BadgeEn : null,
                BadgeAr = descriptor.Badge ? model.BadgeAr : null,
                TitlePrefixEn = descriptor.Title ? model.TitlePrefixEn : null,
                TitlePrefixAr = descriptor.Title ? model.TitlePrefixAr : null,
                TitleHighlightEn = descriptor.Title ? model.TitleHighlightEn : null,
                TitleHighlightAr = descriptor.Title ? model.TitleHighlightAr : null,
                TitleSuffixEn = descriptor.TitleSuffix ? model.TitleSuffixEn : null,
                TitleSuffixAr = descriptor.TitleSuffix ? model.TitleSuffixAr : null,
                BodyEn = descriptor.Body ? model.BodyEn : null,
                BodyAr = descriptor.Body ? model.BodyAr : null,
                ImagePath = descriptor.Image ? model.ImagePath : null,
                ButtonTextEn = descriptor.Button ? model.ButtonTextEn : null,
                ButtonTextAr = descriptor.Button ? model.ButtonTextAr : null,
                ButtonUrl = descriptor.Button ? model.ButtonUrl : null,
                ItemLinkTextEn = descriptor.ItemLink ? model.ItemLinkTextEn : null,
                ItemLinkTextAr = descriptor.ItemLink ? model.ItemLinkTextAr : null,
                EmptyStateEn = descriptor.EmptyState ? model.EmptyStateEn : null,
                EmptyStateAr = descriptor.EmptyState ? model.EmptyStateAr : null,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var result = await _pageBandService.UpsertBandAsync(dto, CurrentUserId());
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not save this section.");
                await RepopulateBandFormAsync(model, descriptor);
                return View(model);
            }

            TempData["SuccessMessage"] = $"{page.Label} — {descriptor.Label} saved.";
            return RedirectToAction(nameof(EditBand), new { pageKey = pk, bandKey = bk, companyId = model.CompanyProfileId });
        }

        // ════════════════════════════════════════════════════════════════
        // BAND ITEMS
        // ════════════════════════════════════════════════════════════════

        [HttpGet("{pageKey}/{bandKey}/items/create")]
        public async Task<IActionResult> CreateItem(string pageKey, string bandKey, [FromQuery] Guid? companyId)
        {
            var descriptor = BandDescriptorFor(pageKey, bandKey);
            if (descriptor == null || !descriptor.Items || !Pages.TryGetValue(CmsPageKeys.Normalise(pageKey), out var page))
            {
                TempData["ErrorMessage"] = "This section does not have editable items.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var pk = CmsPageKeys.Normalise(pageKey);
            var bk = CmsPageKeys.Normalise(bandKey);

            var existing = await _pageBandService.GetBandItemsByCompanyIdAsync(selectedId, pk, bk);

            var model = new CmsPageBandItemFormViewModel
            {
                CompanyProfileId = selectedId,
                CompanyOptions = CompanyOptions(companies, selectedId),
                PageKey = pk,
                BandKey = bk,
                PageLabel = page.Label,
                BandLabel = descriptor.Label,
                ItemsLabel = descriptor.ItemsLabel,
                DisplayOrder = existing.Count == 0 ? 0 : existing.Max(i => i.DisplayOrder) + 1
            };

            ApplyItemFlags(model, descriptor);
            return View("ItemForm", model);
        }

        [HttpPost("{pageKey}/{bandKey}/items/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(string pageKey, string bandKey, CmsPageBandItemFormViewModel model)
        {
            var descriptor = BandDescriptorFor(pageKey, bandKey);
            if (descriptor == null || !descriptor.Items || !Pages.TryGetValue(CmsPageKeys.Normalise(pageKey), out var page))
            {
                TempData["ErrorMessage"] = "This section does not have editable items.";
                return RedirectToAction(nameof(Index));
            }

            var pk = CmsPageKeys.Normalise(pageKey);
            var bk = CmsPageKeys.Normalise(bandKey);
            PrepareItemModel(model, pk, bk, page, descriptor);

            if (!ModelState.IsValid)
            {
                await PopulateItemCompanyOptionsAsync(model);
                return View("ItemForm", model);
            }

            if (descriptor.ItemImage && model.ImageFile != null)
            {
                try
                {
                    model.ImagePath = await CmsMediaUploadHelper.SaveImageAsync(
                        model.ImageFile,
                        _environment,
                        "page-bands");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload an item image for the {BandKey} band on the {PageKey} page.", bk, pk);
                    ModelState.AddModelError(nameof(model.ImageFile), $"Image upload failed: {ex.Message}");
                    await PopulateItemCompanyOptionsAsync(model);
                    return View("ItemForm", model);
                }
            }

            var result = await _pageBandService.CreateBandItemAsync(ToItemDto(model, descriptor), CurrentUserId());
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not save this item.");
                await PopulateItemCompanyOptionsAsync(model);
                return View("ItemForm", model);
            }

            TempData["SuccessMessage"] = $"{model.TitleEn} added.";
            return RedirectToAction(nameof(EditBand), new { pageKey = pk, bandKey = bk, companyId = model.CompanyProfileId });
        }

        [HttpGet("{pageKey}/{bandKey}/items/{id:guid}/edit")]
        public async Task<IActionResult> EditItem(string pageKey, string bandKey, Guid id)
        {
            var descriptor = BandDescriptorFor(pageKey, bandKey);
            if (descriptor == null || !descriptor.Items || !Pages.TryGetValue(CmsPageKeys.Normalise(pageKey), out var page))
            {
                TempData["ErrorMessage"] = "This section does not have editable items.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _pageBandService.GetBandItemByIdAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "This item no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsPageBandItemFormViewModel
            {
                Id = existing.Id,
                CompanyProfileId = existing.CompanyProfileId,
                CompanyOptions = CompanyOptions(companies, existing.CompanyProfileId),
                PageKey = existing.PageKey,
                BandKey = existing.BandKey,
                PageLabel = page.Label,
                BandLabel = descriptor.Label,
                ItemsLabel = descriptor.ItemsLabel,
                IconName = existing.IconName,
                TitleEn = existing.TitleEn,
                TitleAr = existing.TitleAr,
                DescriptionEn = existing.DescriptionEn,
                DescriptionAr = existing.DescriptionAr,
                ImagePath = existing.ImagePath,
                LinkUrl = existing.LinkUrl,
                LinkTextEn = existing.LinkTextEn,
                LinkTextAr = existing.LinkTextAr,
                DisplayOrder = existing.DisplayOrder,
                IsPublished = existing.IsPublished
            };

            ApplyItemFlags(model, descriptor);
            return View("ItemForm", model);
        }

        [HttpPost("{pageKey}/{bandKey}/items/{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(string pageKey, string bandKey, Guid id, CmsPageBandItemFormViewModel model)
        {
            var descriptor = BandDescriptorFor(pageKey, bandKey);
            if (descriptor == null || !descriptor.Items || !Pages.TryGetValue(CmsPageKeys.Normalise(pageKey), out var page))
            {
                TempData["ErrorMessage"] = "This section does not have editable items.";
                return RedirectToAction(nameof(Index));
            }

            var pk = CmsPageKeys.Normalise(pageKey);
            var bk = CmsPageKeys.Normalise(bandKey);
            model.Id = id;
            PrepareItemModel(model, pk, bk, page, descriptor);

            if (!ModelState.IsValid)
            {
                await PopulateItemCompanyOptionsAsync(model);
                return View("ItemForm", model);
            }

            if (descriptor.ItemImage && model.ImageFile != null)
            {
                try
                {
                    model.ImagePath = await CmsMediaUploadHelper.SaveImageAsync(
                        model.ImageFile,
                        _environment,
                        "page-bands");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload an item image for the {BandKey} band on the {PageKey} page.", bk, pk);
                    ModelState.AddModelError(nameof(model.ImageFile), $"Image upload failed: {ex.Message}");
                    await PopulateItemCompanyOptionsAsync(model);
                    return View("ItemForm", model);
                }
            }

            var result = await _pageBandService.UpdateBandItemAsync(id, ToItemDto(model, descriptor), CurrentUserId());
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not save this item.");
                await PopulateItemCompanyOptionsAsync(model);
                return View("ItemForm", model);
            }

            TempData["SuccessMessage"] = $"{model.TitleEn} updated.";
            return RedirectToAction(nameof(EditBand), new { pageKey = pk, bandKey = bk, companyId = model.CompanyProfileId });
        }

        [HttpPost("{pageKey}/{bandKey}/items/{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(string pageKey, string bandKey, Guid id, Guid? companyId)
        {
            var result = await _pageBandService.DeleteBandItemAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Item removed.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not remove this item.";
            }

            return RedirectToAction(nameof(EditBand), new
            {
                pageKey = CmsPageKeys.Normalise(pageKey),
                bandKey = CmsPageKeys.Normalise(bandKey),
                companyId
            });
        }

        // ════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════

        private Guid? CurrentUserId() =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        private static IEnumerable<SelectListItem> CompanyOptions(
            IReadOnlyList<CmsCompanyProfileSummaryDto> companies, Guid selectedId) =>
            companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == selectedId
            });

        private static void ApplyBandFlags(CmsPageBandFormViewModel model, BandDescriptor descriptor)
        {
            model.SupportsBadge = descriptor.Badge;
            model.SupportsTitle = descriptor.Title;
            model.SupportsTitleSuffix = descriptor.TitleSuffix;
            model.SupportsBody = descriptor.Body;
            model.SupportsImage = descriptor.Image;
            model.SupportsButton = descriptor.Button;
            model.SupportsItemLink = descriptor.ItemLink;
            model.SupportsEmptyState = descriptor.EmptyState;
            model.SupportsItems = descriptor.Items;
        }

        private static void ApplyItemFlags(CmsPageBandItemFormViewModel model, BandDescriptor descriptor)
        {
            model.SupportsIcon = descriptor.ItemIcon;
            model.SupportsDescription = descriptor.ItemDescription;
            model.SupportsImage = descriptor.ItemImage;
            model.SupportsLink = descriptor.ItemLinkUrl;
        }

        private void PrepareItemModel(
            CmsPageBandItemFormViewModel model, string pk, string bk,
            PageDescriptor page, BandDescriptor descriptor)
        {
            model.PageKey = pk;
            model.BandKey = bk;
            model.PageLabel = page.Label;
            model.BandLabel = descriptor.Label;
            model.ItemsLabel = descriptor.ItemsLabel;
            ApplyItemFlags(model, descriptor);
        }

        private static CmsPageBandItemUpsertDto ToItemDto(
            CmsPageBandItemFormViewModel model, BandDescriptor descriptor) => new()
            {
                CompanyProfileId = model.CompanyProfileId,
                PageKey = model.PageKey,
                BandKey = model.BandKey,
                IconName = descriptor.ItemIcon ? model.IconName : null,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                DescriptionEn = descriptor.ItemDescription ? model.DescriptionEn : null,
                DescriptionAr = descriptor.ItemDescription ? model.DescriptionAr : null,
                ImagePath = descriptor.ItemImage ? model.ImagePath : null,
                LinkUrl = descriptor.ItemLinkUrl ? model.LinkUrl : null,
                LinkTextEn = descriptor.ItemLinkUrl ? model.LinkTextEn : null,
                LinkTextAr = descriptor.ItemLinkUrl ? model.LinkTextAr : null,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

        private async Task RepopulateBandFormAsync(CmsPageBandFormViewModel model, BandDescriptor descriptor)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = CompanyOptions(companies, model.CompanyProfileId);

            model.Items = descriptor.Items && model.CompanyProfileId != Guid.Empty
                ? await _pageBandService.GetBandItemsByCompanyIdAsync(model.CompanyProfileId, model.PageKey, model.BandKey)
                : Array.Empty<CmsPageBandItemDto>();
        }

        private async Task PopulateItemCompanyOptionsAsync(CmsPageBandItemFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = CompanyOptions(companies, model.CompanyProfileId);
        }
    }
}
