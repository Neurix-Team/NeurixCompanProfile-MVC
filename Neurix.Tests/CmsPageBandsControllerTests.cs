using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Common;
using Neurix.BLL.Services.Cms;
using Neurix.Controllers;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Neurix.Models;
using Xunit;

namespace Neurix.Tests
{
    /// <summary>
    /// The controller's descriptor table decides which fields each band actually renders.
    /// These tests pin that behaviour: a field the band does not show must be stored as null
    /// rather than kept, so a row never carries copy no page can display.
    /// </summary>
    public class CmsPageBandsControllerTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsPageBandService _pageBandService;
        private readonly CmsCompanyProfileService _profileService;
        private readonly CmsPageBandsController _controller;
        private readonly Guid _profileId;

        public CmsPageBandsControllerTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _pageBandService = new CmsPageBandService(_db, NullLogger<CmsPageBandService>.Instance);
            _profileService = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));

            _controller = new CmsPageBandsController(
                _pageBandService,
                _profileService,
                new FakeWebHostEnvironment(),
                NullLogger<CmsPageBandsController>.Instance
            );

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

            _profileId = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = _profileId,
                Slug = "neurix",
                NameEn = "Neurix AI",
                NameAr = "نيوركس AI",
                IsPublished = true
            });
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        /// <summary>Every optional field filled, so a test can assert which ones survive.</summary>
        private CmsPageBandFormViewModel FullForm(string page, string band) => new()
        {
            CompanyProfileId = _profileId,
            PageKey = page,
            BandKey = band,
            BadgeEn = "Badge",
            BadgeAr = "شارة",
            TitlePrefixEn = "Prefix",
            TitlePrefixAr = "بداية",
            TitleHighlightEn = "Highlight",
            TitleHighlightAr = "مميّز",
            TitleSuffixEn = "Suffix",
            TitleSuffixAr = "نهاية",
            BodyEn = "Body",
            BodyAr = "نص",
            ImagePath = "/images/example.png",
            ButtonTextEn = "Go",
            ButtonTextAr = "اذهب",
            ButtonUrl = "/Home/Contact",
            ItemLinkTextEn = "Read",
            ItemLinkTextAr = "اقرأ",
            EmptyStateEn = "Nothing here",
            EmptyStateAr = "لا يوجد",
            IsPublished = true
        };

        // ── index ─────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_ListsEveryPageAndItsBands()
        {
            var result = Assert.IsType<ViewResult>(await _controller.Index(null));
            var model = Assert.IsType<CmsPageBandsIndexViewModel>(result.Model);

            Assert.Equal(CmsPageKeys.All.Length, model.Pages.Count);

            foreach (var pageRow in model.Pages)
            {
                Assert.Equal(CmsPageKeys.BandsFor(pageRow.PageKey).Count, pageRow.Bands.Count);
                Assert.All(pageRow.Bands, b => Assert.False(string.IsNullOrWhiteSpace(b.Label)));
                // Nothing is saved yet, so every band shows as a default.
                Assert.All(pageRow.Bands, b => Assert.Null(b.Band));
            }
        }

        [Fact]
        public async Task Index_MarksTheFiveDivisionPagesAsHavingTheirOwnRow()
        {
            var result = Assert.IsType<ViewResult>(await _controller.Index(null));
            var model = Assert.IsType<CmsPageBandsIndexViewModel>(result.Model);

            var flagged = model.Pages.Where(p => p.HasDivisionPageRow).Select(p => p.PageKey);
            Assert.Equal(CmsPageKeys.DivisionPages, flagged);
        }

        // ── band editor ───────────────────────────────────────────────────────────

        [Fact]
        public async Task EditBand_UnknownPageRedirectsWithoutSaving()
        {
            var result = await _controller.EditBand("careers", CmsPageKeys.Bands.Hero, FullForm("careers", "hero"));

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsPageBandsController.Index), redirect.ActionName);
            Assert.Empty(await _db.PageBands.ToListAsync());
        }

        [Fact]
        public async Task EditBand_UnknownBandOnAKnownPageRedirectsWithoutSaving()
        {
            var result = await _controller.EditBand(CmsPageKeys.Labs, "cta", FullForm(CmsPageKeys.Labs, "cta"));

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(await _db.PageBands.ToListAsync());
        }

        [Fact]
        public async Task EditBand_DivisionHeroKeepsOnlyTheBadge()
        {
            await _controller.EditBand(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero,
                FullForm(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero));

            var stored = await _db.PageBands.SingleAsync();

            // The hero band renders a badge and the chip strip, nothing else.
            Assert.Equal("Badge", stored.BadgeEn);
            Assert.Null(stored.TitlePrefixEn);
            Assert.Null(stored.TitleHighlightEn);
            Assert.Null(stored.BodyEn);
            Assert.Null(stored.ImagePath);
            Assert.Null(stored.ButtonTextEn);
            Assert.Null(stored.ItemLinkTextEn);
            Assert.Null(stored.EmptyStateEn);
        }

        [Fact]
        public async Task EditBand_HqOverviewIsTheOnlyBandThatKeepsAThirdTitleRun()
        {
            await _controller.EditBand(CmsPageKeys.Hq, CmsPageKeys.Bands.Overview,
                FullForm(CmsPageKeys.Hq, CmsPageKeys.Bands.Overview));

            var hq = await _db.PageBands.SingleAsync(b => b.PageKey == CmsPageKeys.Hq);
            Assert.Equal("Suffix", hq.TitleSuffixEn);
            Assert.Equal("/images/example.png", hq.ImagePath);

            await _controller.EditBand(CmsPageKeys.Club, CmsPageKeys.Bands.Overview,
                FullForm(CmsPageKeys.Club, CmsPageKeys.Bands.Overview));

            var club = await _db.PageBands.SingleAsync(b => b.PageKey == CmsPageKeys.Club);
            Assert.Null(club.TitleSuffixEn);
        }

        [Fact]
        public async Task EditBand_TechnologyCtaKeepsTheButtonAndNoListFields()
        {
            await _controller.EditBand(CmsPageKeys.Technology, CmsPageKeys.Bands.Cta,
                FullForm(CmsPageKeys.Technology, CmsPageKeys.Bands.Cta));

            var stored = await _db.PageBands.SingleAsync();

            Assert.Equal("Go", stored.ButtonTextEn);
            Assert.Equal("/Home/Contact", stored.ButtonUrl);
            Assert.Null(stored.ItemLinkTextEn);
            Assert.Null(stored.EmptyStateEn);
            Assert.Null(stored.ImagePath);
        }

        [Fact]
        public async Task EditBand_ListPageHeaderKeepsTheCardLinkAndEmptyStateButNoButton()
        {
            await _controller.EditBand(CmsPageKeys.Insights, CmsPageKeys.Bands.Hero,
                FullForm(CmsPageKeys.Insights, CmsPageKeys.Bands.Hero));

            var stored = await _db.PageBands.SingleAsync();

            Assert.Equal("Read", stored.ItemLinkTextEn);
            Assert.Equal("Nothing here", stored.EmptyStateEn);
            Assert.Equal("Prefix", stored.TitlePrefixEn);
            Assert.Null(stored.ButtonTextEn);
            Assert.Null(stored.ButtonUrl);
        }

        [Fact]
        public async Task EditBand_RedirectsBackToTheSameBandOnSuccess()
        {
            var result = await _controller.EditBand(CmsPageKeys.Portfolio, CmsPageKeys.Bands.Hero,
                FullForm(CmsPageKeys.Portfolio, CmsPageKeys.Bands.Hero));

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsPageBandsController.EditBand), redirect.ActionName);
            Assert.Equal(CmsPageKeys.Portfolio, redirect.RouteValues!["pageKey"]);
            Assert.Equal(CmsPageKeys.Bands.Hero, redirect.RouteValues["bandKey"]);
        }

        [Fact]
        public async Task EditBand_GetPrefillsWhatWasSaved()
        {
            await _controller.EditBand(CmsPageKeys.Plus, CmsPageKeys.Bands.Cards,
                FullForm(CmsPageKeys.Plus, CmsPageKeys.Bands.Cards));

            var result = Assert.IsType<ViewResult>(
                await _controller.EditBand(CmsPageKeys.Plus, CmsPageKeys.Bands.Cards, _profileId));
            var model = Assert.IsType<CmsPageBandFormViewModel>(result.Model);

            Assert.Equal("Prefix", model.TitlePrefixEn);
            Assert.True(model.SupportsItems);
            Assert.False(model.SupportsBadge);
        }

        // ── band items ────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateItem_OnABandWithNoItemsRedirectsWithoutSaving()
        {
            var model = new CmsPageBandItemFormViewModel
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.Hq,
                BandKey = CmsPageKeys.Bands.Overview,
                TitleEn = "Nope",
                TitleAr = "لا"
            };

            var result = await _controller.CreateItem(CmsPageKeys.Hq, CmsPageKeys.Bands.Overview, model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(await _db.PageBandItems.ToListAsync());
        }

        [Fact]
        public async Task CreateItem_ChipStripKeepsOnlyTheTitles()
        {
            var model = new CmsPageBandItemFormViewModel
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.Labs,
                BandKey = CmsPageKeys.Bands.Hero,
                IconName = "cpu",
                TitleEn = "Research",
                TitleAr = "البحث",
                DescriptionEn = "Should not be kept",
                DescriptionAr = "لا يُحفظ",
                ImagePath = "/images/x.png",
                LinkUrl = "/somewhere",
                IsPublished = true
            };

            await _controller.CreateItem(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero, model);

            var stored = await _db.PageBandItems.SingleAsync();
            Assert.Equal("Research", stored.TitleEn);
            Assert.Null(stored.IconName);
            Assert.Null(stored.DescriptionEn);
            Assert.Null(stored.ImagePath);
            Assert.Null(stored.LinkUrl);
        }

        [Fact]
        public async Task CreateItem_AiHeroButtonKeepsItsLink()
        {
            var model = new CmsPageBandItemFormViewModel
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.Ai,
                BandKey = CmsPageKeys.Bands.Hero,
                TitleEn = "Go to Neurix AI Labs",
                TitleAr = "اذهب إلى Neurix AI Labs",
                LinkUrl = "/Home/Labs",
                LinkTextEn = "Go",
                LinkTextAr = "اذهب",
                IsPublished = true
            };

            await _controller.CreateItem(CmsPageKeys.Ai, CmsPageKeys.Bands.Hero, model);

            var stored = await _db.PageBandItems.SingleAsync();
            Assert.Equal("/Home/Labs", stored.LinkUrl);
            Assert.Equal("Go", stored.LinkTextEn);
        }

        [Fact]
        public async Task CreateItem_LabsOverviewImageKeepsItsPath()
        {
            var model = new CmsPageBandItemFormViewModel
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.Labs,
                BandKey = CmsPageKeys.Bands.Overview,
                TitleEn = "Diagram",
                TitleAr = "رسم",
                ImagePath = "/images/diagram.png",
                IconName = "cpu",
                IsPublished = true
            };

            await _controller.CreateItem(CmsPageKeys.Labs, CmsPageKeys.Bands.Overview, model);

            var stored = await _db.PageBandItems.SingleAsync();
            Assert.Equal("/images/diagram.png", stored.ImagePath);
            Assert.Null(stored.IconName);
        }

        [Fact]
        public async Task CreateItem_GetSuggestsTheNextDisplayOrder()
        {
            await _pageBandService.CreateBandItemAsync(new Neurix.BLL.Dtos.Cms.CmsPageBandItemUpsertDto
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.Labs,
                BandKey = CmsPageKeys.Bands.Cards,
                TitleEn = "Existing",
                TitleAr = "قائم",
                DisplayOrder = 4,
                IsPublished = true
            });

            var result = Assert.IsType<ViewResult>(
                await _controller.CreateItem(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards, _profileId));
            var model = Assert.IsType<CmsPageBandItemFormViewModel>(result.Model);

            Assert.Equal(5, model.DisplayOrder);
            Assert.Equal("ItemForm", result.ViewName);
        }

        [Fact]
        public async Task DeleteItem_RedirectsBackToItsBand()
        {
            var created = await _pageBandService.CreateBandItemAsync(new Neurix.BLL.Dtos.Cms.CmsPageBandItemUpsertDto
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.About,
                BandKey = CmsPageKeys.Bands.Principles,
                TitleEn = "Doomed",
                TitleAr = "محذوف",
                IsPublished = true
            });

            var result = await _controller.DeleteItem(
                CmsPageKeys.About, CmsPageKeys.Bands.Principles, created.Value, _profileId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsPageBandsController.EditBand), redirect.ActionName);
            Assert.Empty(await _pageBandService.GetBandItemsByCompanyIdAsync(
                _profileId, CmsPageKeys.About, CmsPageKeys.Bands.Principles));
        }

        private class FakeWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.GetTempPath();
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string ApplicationName { get; set; } = "Neurix";
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public string EnvironmentName { get; set; } = "Development";
        }

        private class TestTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) {}
        }
    }
}
