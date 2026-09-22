using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    public class CmsContentPagesControllerTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsContentPageService _contentPageService;
        private readonly CmsPageBandService _pageBandService;
        private readonly CmsCompanyProfileService _profileService;
        private readonly CmsContentPagesController _controller;
        private readonly Guid _profileId;
        private readonly Guid _aboutPageId;
        private readonly Guid _privacyPageId;

        public CmsContentPagesControllerTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _contentPageService = new CmsContentPageService(_db);
            _pageBandService = new CmsPageBandService(_db, NullLogger<CmsPageBandService>.Instance);
            _profileService = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));

            _controller = new CmsContentPagesController(
                _contentPageService,
                _profileService,
                NullLogger<CmsContentPagesController>.Instance,
                _pageBandService
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

            _aboutPageId = Guid.NewGuid();
            _db.ContentPages.Add(new CmsContentPage
            {
                Id = _aboutPageId,
                CompanyProfileId = _profileId,
                Slug = "about",
                TitleEn = "About Neurix",
                TitleAr = "من نحن",
                HeroTitlePrefixEn = "Architecting",
                HeroTitlePrefixAr = "صناعة",
                HeroTitleHighlightEn = "Intelligence",
                HeroTitleHighlightAr = "الذكاء",
                HeroSubtitleEn = "Pioneering sovereign intelligence.",
                HeroSubtitleAr = "ريادة حلول الذكاء السيادي.",
                MissionTitleEn = "Our Mission",
                MissionTitleAr = "رسالتنا",
                MissionTextEn = "Mission statement",
                MissionTextAr = "نص الرسالة",
                VisionTitleEn = "Our Vision",
                VisionTitleAr = "رؤيتنا",
                VisionTextEn = "Vision statement",
                VisionTextAr = "نص الرؤية",
                IsPublished = true
            });

            _privacyPageId = Guid.NewGuid();
            _db.ContentPages.Add(new CmsContentPage
            {
                Id = _privacyPageId,
                CompanyProfileId = _profileId,
                Slug = "privacy",
                TitleEn = "Privacy Policy",
                TitleAr = "سياسة الخصوصية",
                HeroTitlePrefixEn = "Privacy",
                HeroTitlePrefixAr = "سياسة",
                HeroTitleHighlightEn = "Policy",
                HeroTitleHighlightAr = "الخصوصية",
                BodyEn = "Privacy body",
                BodyAr = "نص الخصوصية",
                IsPublished = true
            });

            // Add About Page Bands: principles, structure
            var principlesBand = new CmsPageBand
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _profileId,
                PageKey = "about",
                BandKey = "principles",
                TitlePrefixEn = "What We",
                TitlePrefixAr = "ما نؤمن",
                TitleHighlightEn = "Believe In",
                TitleHighlightAr = "به",
                DisplayOrder = 1,
                IsPublished = true
            };

            var structureBand = new CmsPageBand
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _profileId,
                PageKey = "about",
                BandKey = "structure",
                BadgeEn = "Organizational Structure",
                BadgeAr = "الهيكل التنظيمي",
                TitlePrefixEn = "Five Pillars,",
                TitlePrefixAr = "خمس ركائز،",
                TitleHighlightEn = "One Vision",
                TitleHighlightAr = "رؤية واحدة",
                DisplayOrder = 2,
                IsPublished = true
            };

            _db.PageBands.AddRange(principlesBand, structureBand);

            // Add 2 principle items and 2 structure items
            _db.PageBandItems.AddRange(
                new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _profileId,
                    PageKey = "about",
                    BandKey = "principles",
                    TitleEn = "Innovation First",
                    TitleAr = "الابتكار أولاً",
                    DescriptionEn = "Pushing boundaries",
                    DescriptionAr = "دفع الحدود",
                    DisplayOrder = 1,
                    IsPublished = true
                },
                new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _profileId,
                    PageKey = "about",
                    BandKey = "principles",
                    TitleEn = "Integrity",
                    TitleAr = "النزاهة",
                    DescriptionEn = "Ethical AI",
                    DescriptionAr = "ذكاء أخلاقي",
                    DisplayOrder = 2,
                    IsPublished = true
                },
                new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _profileId,
                    PageKey = "about",
                    BandKey = "structure",
                    TitleEn = "Neurix Labs",
                    TitleAr = "مختبرات نيوركس",
                    DescriptionEn = "R&D powerhouse",
                    DescriptionAr = "مركز البحث والتطوير",
                    LinkUrl = "/Home/Labs",
                    DisplayOrder = 1,
                    IsPublished = true
                },
                new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _profileId,
                    PageKey = "about",
                    BandKey = "structure",
                    TitleEn = "Neurix Technology",
                    TitleAr = "تكنولوجيا نيوركس",
                    DescriptionEn = "Enterprise platforms",
                    DescriptionAr = "منصات المؤسسات",
                    LinkUrl = "/Home/Technology",
                    DisplayOrder = 2,
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
        public async Task Index_ForAboutPage_PopulatesLiveBandsAndCardsCount()
        {
            // Act
            var result = await _controller.Index(_profileId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsContentPageListViewModel>(viewResult.Model);

            var aboutItem = model.Pages.FirstOrDefault(p => p.Slug == "about");
            Assert.NotNull(aboutItem);
            Assert.Equal(2, aboutItem.LiveBandsCount);
            Assert.Equal(4, aboutItem.TotalItemsCount);

            var privacyItem = model.Pages.FirstOrDefault(p => p.Slug == "privacy");
            Assert.NotNull(privacyItem);
            Assert.Equal(0, privacyItem.LiveBandsCount);
            Assert.Equal(0, privacyItem.TotalItemsCount);
        }

        [Fact]
        public async Task Edit_Get_ForAboutPage_PopulatesLiveBandsWithCards()
        {
            // Act
            var result = await _controller.Edit(_aboutPageId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsContentPageFormViewModel>(viewResult.Model);

            Assert.Equal("about", model.Slug);
            Assert.NotEmpty(model.LiveBands);

            var principlesBand = model.LiveBands.FirstOrDefault(b => b.BandKey == "principles");
            Assert.NotNull(principlesBand);
            Assert.True(principlesBand.SupportsItems);
            Assert.Equal(2, principlesBand.ItemCount);
            Assert.Equal("What We", principlesBand.TitlePrefixEn);
            Assert.Equal("Believe In", principlesBand.TitleHighlightEn);
            Assert.Equal(2, principlesBand.Items.Count);
            Assert.Contains(principlesBand.Items, i => i.TitleEn == "Innovation First");

            var structureBand = model.LiveBands.FirstOrDefault(b => b.BandKey == "structure");
            Assert.NotNull(structureBand);
            Assert.True(structureBand.SupportsItems);
            Assert.Equal(2, structureBand.ItemCount);
            Assert.Equal("Five Pillars,", structureBand.TitlePrefixEn);
            Assert.Equal("One Vision", structureBand.TitleHighlightEn);
            Assert.Equal("Organizational Structure", structureBand.BadgeEn);
            Assert.Equal(2, structureBand.Items.Count);
            Assert.Contains(structureBand.Items, i => i.TitleEn == "Neurix Labs" && i.LinkUrl == "/Home/Labs");
        }

        [Fact]
        public async Task Edit_Get_ForPrivacyPage_LeavesLiveBandsEmpty()
        {
            // Act
            var result = await _controller.Edit(_privacyPageId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsContentPageFormViewModel>(viewResult.Model);

            Assert.Equal("privacy", model.Slug);
            Assert.Empty(model.LiveBands);
        }

        private class TestTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }
    }
}
