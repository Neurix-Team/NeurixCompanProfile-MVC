using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
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
    public class CmsDivisionPagesControllerTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsDivisionPageService _divisionService;
        private readonly CmsPageBandService _pageBandService;
        private readonly CmsCompanyProfileService _profileService;
        private readonly CmsDivisionPagesController _controller;
        private readonly Guid _profileId;
        private readonly Guid _labsDivisionId;

        public CmsDivisionPagesControllerTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _divisionService = new CmsDivisionPageService(_db);
            _pageBandService = new CmsPageBandService(_db, NullLogger<CmsPageBandService>.Instance);
            _profileService = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));

            _controller = new CmsDivisionPagesController(
                _divisionService,
                _pageBandService,
                _profileService,
                new FakeWebHostEnvironment(),
                NullLogger<CmsDivisionPagesController>.Instance
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

            _labsDivisionId = Guid.NewGuid();
            _db.DivisionPages.Add(new CmsDivisionPage
            {
                Id = _labsDivisionId,
                CompanyProfileId = _profileId,
                Slug = "labs",
                HeroTitleEn = "Neurix AI Labs — Shaping Tomorrow",
                HeroTitleAr = "مختبرات نيوركس AI — صناعة الغد",
                HeroSubtitleEn = "Advanced R&D lab",
                HeroSubtitleAr = "مختبر بحث وتطوير",
                MissionEn = "Applied research mandate",
                MissionAr = "رسالة البحث التطبيقي",
                IsPublished = true
            });

            // Add the Laboratory Capabilities band and cards
            _db.PageBands.Add(new CmsPageBand
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _profileId,
                PageKey = "labs",
                BandKey = "cards",
                TitlePrefixEn = "Laboratory Capabilities",
                TitlePrefixAr = "قدرات المختبر",
                BodyEn = "A research and technology infrastructure...",
                BodyAr = "بنية بحثية وتقنية...",
                IsPublished = true
            });

            _db.PageBandItems.AddRange(
                new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _profileId,
                    PageKey = "labs",
                    BandKey = "cards",
                    IconName = "bar-chart-2",
                    TitleEn = "Data Visualization & Analytics",
                    TitleAr = "تصور البيانات وتحليلها",
                    DescriptionEn = "Analytics desc",
                    DescriptionAr = "وصف التحليلات",
                    DisplayOrder = 0,
                    IsPublished = true
                },
                new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _profileId,
                    PageKey = "labs",
                    BandKey = "cards",
                    IconName = "brain-circuit",
                    TitleEn = "User & System Intelligence",
                    TitleAr = "ذكاء المستخدم والأنظمة",
                    DisplayOrder = 1,
                    IsPublished = true
                }
            );

            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task Index_ReturnsPageItemsWithLiveBandsCountAndCapabilitiesUrl()
        {
            var result = await _controller.Index(_profileId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsDivisionPageListViewModel>(viewResult.Model);

            Assert.Single(model.Pages);
            Assert.Single(model.PageItems);

            var labsItem = model.PageItems[0];
            Assert.Equal("labs", labsItem.Page.Slug);
            Assert.Equal(3, labsItem.LiveBandsCount); // hero, overview, cards
            Assert.Equal(2, labsItem.TotalItemsCount); // 2 cards added above
            Assert.NotNull(labsItem.CapabilitiesBandEditUrl);
            Assert.Contains("/cms/page-sections/labs/cards/edit", labsItem.CapabilitiesBandEditUrl);
        }

        [Fact]
        public async Task Edit_PopulatesLiveBandsWithLaboratoryCapabilitiesAndItems()
        {
            var result = await _controller.Edit(_labsDivisionId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsDivisionPageFormViewModel>(viewResult.Model);

            Assert.Equal("labs", model.Slug);
            Assert.NotEmpty(model.LiveBands);

            var cardsBand = Assert.Single(model.LiveBands, b => b.BandKey == "cards");
            Assert.Equal("Laboratory Capabilities Band", cardsBand.Label);
            Assert.Equal("Laboratory Capabilities", cardsBand.TitlePrefixEn);
            Assert.Equal("قدرات المختبر", cardsBand.TitlePrefixAr);
            Assert.True(cardsBand.IsPublished);
            Assert.Equal(2, cardsBand.ItemCount);

            var firstCard = cardsBand.Items[0];
            Assert.Equal("Data Visualization & Analytics", firstCard.TitleEn);
            Assert.Equal("bar-chart-2", firstCard.IconName);
            Assert.Contains("/cms/page-sections/labs/cards/items/", firstCard.EditItemUrl);
        }

        [Fact]
        public async Task Edit_UnknownDivisionPage_RedirectsToIndex()
        {
            var result = await _controller.Edit(Guid.NewGuid());

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsDivisionPagesController.Index), redirect.ActionName);
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
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }
    }
}
