using System;
using System.Collections.Generic;
using System.IO;
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
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.Controllers;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Neurix.Models;
using Xunit;

namespace Neurix.Tests
{
    public class CmsHomeSectionsControllerTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsHomeSectionService _homeSectionService;
        private readonly CmsCompanyProfileService _profileService;
        private readonly FakeWebHostEnvironment _env;
        private readonly CmsHomeSectionsController _controller;
        private readonly Guid _profileId;

        public CmsHomeSectionsControllerTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _homeSectionService = new CmsHomeSectionService(_db, NullLogger<CmsHomeSectionService>.Instance);
            _profileService = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));
            _env = new FakeWebHostEnvironment();

            _controller = new CmsHomeSectionsController(
                _homeSectionService,
                _profileService,
                _env,
                NullLogger<CmsHomeSectionsController>.Instance
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

        [Fact]
        public async Task Index_ReturnsViewWithPopulatedSections()
        {
            var result = await _controller.Index(_profileId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsHomeSectionsIndexViewModel>(viewResult.Model);
            Assert.Equal(_profileId, model.SelectedCompanyId);
            Assert.Single(model.Companies);
        }

        [Fact]
        public async Task EditHero_Post_ValidModel_RedirectsToIndex()
        {
            var formModel = new CmsHeroSectionFormViewModel
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Custom Hero Badge",
                BadgeAr = "شارة مخصصة",
                TitlePrefixEn = "Prefix ",
                TitleHighlightEn = "Highlight",
                TitleSuffixEn = " Suffix",
                TitlePrefixAr = "بادئة ",
                TitleHighlightAr = "تمييز",
                TitleSuffixAr = " لاحقة",
                SubtitleEn = "Hero Subtitle",
                SubtitleAr = "عنوان فرعي",
                PrimaryButtonTextEn = "Btn 1",
                PrimaryButtonTextAr = "زر 1",
                PrimaryButtonUrl = "#btn1",
                SecondaryButtonTextEn = "Btn 2",
                SecondaryButtonTextAr = "زر 2",
                SecondaryButtonUrl = "#btn2",
                Stat1Value = "10+",
                Stat1LabelEn = "Engines",
                Stat1LabelAr = "محركات",
                Stat2Value = "20+",
                Stat2LabelEn = "Clusters",
                Stat2LabelAr = "عناقيد",
                Stat3Value = "30+",
                Stat3LabelEn = "Nodes",
                Stat3LabelAr = "عقد",
                IsPublished = true
            };

            var result = await _controller.EditHero(formModel);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsHomeSectionsController.Index), redirectResult.ActionName);

            var hero = await _homeSectionService.GetHeroSectionByCompanySlugAsync("neurix");
            Assert.NotNull(hero);
            Assert.Equal("Custom Hero Badge", hero.BadgeEn);
            Assert.Equal("10+", hero.Stat1Value);
        }

        [Fact]
        public async Task EditHumanVision_Post_ValidModel_RedirectsToIndex()
        {
            var formModel = new CmsHumanVisionSectionFormViewModel
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Human Vision Badge",
                BadgeAr = "شارة الرؤية الإنسانية",
                TitlePrefixEn = "Title Prefix ",
                TitleHighlightEn = "Highlight Word",
                TitlePrefixAr = "بادئة ",
                TitleHighlightAr = "كلمة مميزة",
                Paragraph1En = "Vision Paragraph 1 EN",
                Paragraph1Ar = "فقرة الرؤية 1 عربي",
                Paragraph2En = "Vision Paragraph 2 EN",
                Paragraph2Ar = "فقرة الرؤية 2 عربي",
                ImagePath = "/images/Bringing Clarity to Complexity_1 2.png",
                ImageAltEn = "Alt text EN",
                ImageAltAr = "نص بديل عربي",
                IsPublished = true
            };

            var result = await _controller.EditHumanVision(formModel);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsHomeSectionsController.Index), redirectResult.ActionName);

            var vision = await _homeSectionService.GetHumanVisionSectionByCompanySlugAsync("neurix");
            Assert.NotNull(vision);
            Assert.Equal("Human Vision Badge", vision.BadgeEn);
            Assert.Equal("Vision Paragraph 1 EN", vision.Paragraph1En);
        }

        [Fact]
        public async Task EditPioneers_Post_ValidModel_RedirectsToIndex()
        {
            var formModel = new CmsPioneersSectionFormViewModel
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Pioneers Badge",
                BadgeAr = "شارة الرواد",
                TitlePrefixEn = "Pioneers Prefix ",
                TitleHighlightEn = "Pioneers Highlight",
                TitlePrefixAr = "بادئة الرواد ",
                TitleHighlightAr = "تمييز الرواد",
                Paragraph1En = "Pioneers Paragraph 1 EN",
                Paragraph1Ar = "فقرة الرواد 1 عربي",
                Paragraph2En = "Pioneers Paragraph 2 EN",
                Paragraph2Ar = "فقرة الرواد 2 عربي",
                ImagePath = "/images/Infrastructure.png",
                ImageAltEn = "Alt text EN",
                ImageAltAr = "نص بديل عربي",
                IsPublished = true
            };

            var result = await _controller.EditPioneers(formModel);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsHomeSectionsController.Index), redirectResult.ActionName);

            var pioneers = await _homeSectionService.GetPioneersSectionByCompanySlugAsync("neurix");
            Assert.NotNull(pioneers);
            Assert.Equal("Pioneers Badge", pioneers.BadgeEn);
            Assert.Equal("Pioneers Paragraph 1 EN", pioneers.Paragraph1En);
        }

        // ── blocks that used to have no dashboard form at all ──

        [Fact]
        public async Task Index_ExposesTheDivisionsAiEngineeringAndBandHeaderSections()
        {
            await _homeSectionService.UpsertDivisionsSectionAsync(new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Our Ecosystem",
                IsPublished = true
            });
            await _homeSectionService.CreateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                CompanyProfileId = _profileId,
                TitleEn = "Neurix AI Labs",
                TitleAr = "مختبرات نيوركس AI",
                LinkUrl = "/Home/Labs",
                IsPublished = true
            });
            await _homeSectionService.UpsertAiEngineeringSectionAsync(new CmsAiEngineeringSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                TitleEn = "AI & Engineering",
                IsPublished = true
            });
            await _homeSectionService.CreateAiEngineeringItemAsync(new CmsAiEngineeringItemUpsertDto
            {
                CompanyProfileId = _profileId,
                TitleEn = "Software Development",
                TitleAr = "تطوير البرمجيات",
                IsPublished = true
            });
            await _homeSectionService.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = CmsListSectionKeys.Portfolio,
                BadgeEn = "Engineering Portfolio",
                IsPublished = true
            });

            var result = await _controller.Index(_profileId);

            var model = Assert.IsType<CmsHomeSectionsIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotNull(model.DivisionsSection);
            Assert.Single(model.DivisionItems);
            Assert.NotNull(model.AiEngineeringSection);
            Assert.Single(model.AiEngineeringItems);
            Assert.Equal("Engineering Portfolio", model.ListHeader(CmsListSectionKeys.Portfolio)?.BadgeEn);
            Assert.Null(model.ListHeader(CmsListSectionKeys.Insights));
        }

        [Fact]
        public async Task EditDivisions_Post_ValidModel_PersistsAndRedirectsBack()
        {
            var result = await _controller.EditDivisions(new CmsDivisionsSectionFormViewModel
            {
                CompanyProfileId = _profileId,
                BadgeEn = "The Group",
                BadgeAr = "المجموعة",
                TitlePrefixEn = "Five subsidiaries. ",
                TitleHighlightEn = "One ecosystem.",
                TitlePrefixAr = "خمسة فروع. ",
                TitleHighlightAr = "منظومة واحدة.",
                DescriptionEn = "Intro copy.",
                DescriptionAr = "نص تعريفي.",
                IsPublished = true
            });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsHomeSectionsController.EditDivisions), redirect.ActionName);

            var saved = await _homeSectionService.GetDivisionsSectionByCompanyIdAsync(_profileId);
            Assert.Equal("The Group", saved!.BadgeEn);
        }

        [Fact]
        public async Task CreateAndDeleteDivisionItem_Post_RoundTripThroughTheDashboard()
        {
            var create = await _controller.CreateDivisionItem(new CmsDivisionItemFormViewModel
            {
                CompanyProfileId = _profileId,
                IconName = "beaker",
                TitleEn = "Neurix AI Labs",
                TitleAr = "مختبرات نيوركس AI",
                DescriptionEn = "R&D.",
                DescriptionAr = "البحث والتطوير.",
                LinkUrl = "/Home/Labs",
                DisplayOrder = 1,
                IsPublished = true
            });

            Assert.IsType<RedirectToActionResult>(create);

            var items = await _homeSectionService.GetDivisionItemsByCompanyIdAsync(_profileId, includeUnpublished: true);
            var created = Assert.Single(items);
            Assert.Equal("Neurix AI Labs", created.TitleEn);

            var edit = await _controller.EditDivisionItem(created.Id);
            var editModel = Assert.IsType<CmsDivisionItemFormViewModel>(Assert.IsType<ViewResult>(edit).Model);
            Assert.Equal("/Home/Labs", editModel.LinkUrl);

            var delete = await _controller.DeleteDivisionItem(created.Id, _profileId);
            Assert.IsType<RedirectToActionResult>(delete);
            Assert.Empty(await _homeSectionService.GetDivisionItemsByCompanyIdAsync(_profileId, includeUnpublished: true));
        }

        [Fact]
        public async Task EditAiEngineering_Post_ValidModel_PersistsAndRedirectsBack()
        {
            var result = await _controller.EditAiEngineering(new CmsAiEngineeringSectionFormViewModel
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Core Services",
                BadgeAr = "خدماتنا الأساسية",
                TitleEn = "AI & Engineering",
                TitleAr = "الذكاء الاصطناعي والهندسة",
                IsPublished = true
            });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AI & Engineering", (await _homeSectionService.GetAiEngineeringSectionByCompanyIdAsync(_profileId))!.TitleEn);
        }

        [Fact]
        public async Task EditListHeader_Get_DescribesTheBandAndWhereItsCardsLive()
        {
            var portfolio = await _controller.EditListHeader(CmsListSectionKeys.Portfolio, _profileId);
            var portfolioModel = Assert.IsType<CmsListSectionHeaderFormViewModel>(Assert.IsType<ViewResult>(portfolio).Model);
            Assert.Equal("#portfolio", portfolioModel.SectionAnchor);
            Assert.Equal("Projects", portfolioModel.ItemsManagedAt);
            Assert.True(portfolioModel.SupportsButton);
            Assert.True(portfolioModel.SupportsItemLink);
            Assert.True(portfolioModel.SupportsCategoryFallback);
            Assert.False(portfolioModel.SupportsReadTime);   // projects carry no read time
            Assert.False(portfolioModel.SupportsSubtitle);

            var testimonials = await _controller.EditListHeader(CmsListSectionKeys.Testimonials, _profileId);
            var testimonialsModel = Assert.IsType<CmsListSectionHeaderFormViewModel>(Assert.IsType<ViewResult>(testimonials).Model);
            Assert.True(testimonialsModel.SupportsSubtitle);
            Assert.False(testimonialsModel.SupportsButton);
            Assert.False(testimonialsModel.SupportsItemLink);
            Assert.False(testimonialsModel.SupportsCategoryFallback);

            var insights = await _controller.EditListHeader(CmsListSectionKeys.Insights, _profileId);
            var insightsModel = Assert.IsType<CmsListSectionHeaderFormViewModel>(Assert.IsType<ViewResult>(insights).Model);
            Assert.True(insightsModel.SupportsCategoryFallback);
            Assert.True(insightsModel.SupportsReadTime);
        }

        [Fact]
        public async Task EditListHeader_Post_PersistsAndDropsFieldsTheBandDoesNotRender()
        {
            // #testimonials has no "view all" button, so a posted button label must not be stored.
            var result = await _controller.EditListHeader(CmsListSectionKeys.Testimonials, new CmsListSectionHeaderFormViewModel
            {
                CompanyProfileId = _profileId,
                SectionKey = CmsListSectionKeys.Testimonials,
                BadgeEn = "Trusted by Leaders",
                BadgeAr = "موثوق من قادة الصناعة",
                TitlePrefixEn = "What Partners Say",
                TitlePrefixAr = "ماذا يقول شركاؤنا",
                SubtitleEn = "Endorsements.",
                SubtitleAr = "توصيات.",
                ItemLinkTextEn = "Should be ignored too",
                DefaultCategoryLabelEn = "Ignored as well",
                ReadTimeSuffixEn = "Ignored as well",
                UndatedLabelEn = "Ignored as well",
                ButtonTextEn = "Should be ignored",
                ButtonUrl = "/nowhere",
                IsPublished = true
            });

            Assert.IsType<RedirectToActionResult>(result);

            var saved = await _homeSectionService.GetListSectionHeaderByCompanyIdAsync(_profileId, CmsListSectionKeys.Testimonials);
            Assert.NotNull(saved);
            Assert.Equal("Trusted by Leaders", saved!.BadgeEn);
            Assert.Equal("Endorsements.", saved.SubtitleEn);
            Assert.Null(saved.ItemLinkTextEn);
            Assert.Null(saved.DefaultCategoryLabelEn);
            Assert.Null(saved.ReadTimeSuffixEn);
            Assert.Null(saved.UndatedLabelEn);
            Assert.Null(saved.ButtonTextEn);
            Assert.Null(saved.ButtonUrl);
        }

        [Fact]
        public async Task EditListHeader_UnknownBand_RedirectsToIndexWithoutSaving()
        {
            var result = await _controller.EditListHeader("newsletter", _profileId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CmsHomeSectionsController.Index), redirect.ActionName);
            Assert.Empty(await _homeSectionService.GetListSectionHeadersByCompanyIdAsync(_profileId));
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
