using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services;
using Neurix.BLL.Services.Cms;
using Neurix.Controllers;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;
using Neurix.Models;
using Xunit;

namespace Neurix.Tests
{
    public class HomeControllerTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsServiceService _cmsServiceService;
        private readonly CmsCompanyProfileService _cmsCompanyProfileService;
        private readonly CmsTestimonialService _cmsTestimonialService;
        private readonly CmsBlogPostService _cmsBlogPostService;
        private readonly CmsProjectService _cmsProjectService;
        private readonly CmsTeamMemberService _cmsTeamMemberService;
        private readonly CmsDivisionPageService _cmsDivisionPageService;
        private readonly CmsSiteSettingService _cmsSiteSettingService;
        private readonly CmsHomeSectionService _cmsHomeSectionService;
        private readonly CmsContentPageService _cmsContentPageService;
        private readonly CmsPageBandService _cmsPageBandService;
        private readonly FakeLeadService _fakeLeadService;
        private readonly HomeController _controller;
        private readonly Guid _neurixProfileId;

        public HomeControllerTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _cmsServiceService = new CmsServiceService(_db);
            _cmsCompanyProfileService = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));
            _cmsTestimonialService = new CmsTestimonialService(_db);
            _cmsBlogPostService = new CmsBlogPostService(_db);
            _cmsProjectService = new CmsProjectService(_db);
            _cmsTeamMemberService = new CmsTeamMemberService(_db);
            _cmsDivisionPageService = new CmsDivisionPageService(_db);
            _cmsSiteSettingService = new CmsSiteSettingService(_db);
            _cmsHomeSectionService = new CmsHomeSectionService(_db, NullLogger<CmsHomeSectionService>.Instance);
            _cmsContentPageService = new CmsContentPageService(_db);
            _cmsPageBandService = new CmsPageBandService(_db, NullLogger<CmsPageBandService>.Instance);
            _fakeLeadService = new FakeLeadService();

            _controller = new HomeController(
                _fakeLeadService,
                _cmsServiceService,
                _cmsCompanyProfileService,
                _cmsTestimonialService,
                _cmsBlogPostService,
                _cmsProjectService,
                _cmsTeamMemberService,
                _cmsDivisionPageService,
                _cmsSiteSettingService,
                _cmsHomeSectionService,
                _cmsContentPageService,
                _cmsPageBandService,
                new MemoryCache(new MemoryCacheOptions()),
                NullLogger<HomeController>.Instance
            );

            _neurixProfileId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = _neurixProfileId,
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
        public async Task Index_ReturnsViewWithHomePageViewModelContainingPublishedServices()
        {
            _db.Services.AddRange(
                new CmsService
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "rd",
                    NameEn = "Research & Development",
                    NameAr = "البحث والتطوير",
                    ShortDescriptionEn = "R&D Short desc",
                    ShortDescriptionAr = "وصف مختصر",
                    IconName = "microscope",
                    DisplayOrder = 1,
                    IsPublished = true
                },
                new CmsService
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "software",
                    NameEn = "Software Implementation",
                    NameAr = "تنفيذ البرمجيات",
                    ShortDescriptionEn = "Software Short desc",
                    ShortDescriptionAr = "وصف مختصر برمجيات",
                    IconName = "code",
                    DisplayOrder = 2,
                    IsPublished = true
                },
                new CmsService
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "draft-service",
                    NameEn = "Draft Service",
                    NameAr = "خدمة مسودة",
                    DisplayOrder = 3,
                    IsPublished = false // should NOT appear on public homepage
                }
            );
            await _db.SaveChangesAsync();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomePageViewModel>(viewResult.Model);

            Assert.NotNull(model.CompanyProfile);
            Assert.Equal("neurix", model.CompanyProfile.Slug);

            Assert.Equal(2, model.Services.Count);
            Assert.Equal("rd", model.Services[0].Slug);
            Assert.Equal("software", model.Services[1].Slug);
            Assert.DoesNotContain(model.Services, s => s.Slug == "draft-service");
        }

        [Fact]
        public async Task Index_WhenNoServicesExist_ReturnsEmptyServicesListWithoutThrowing()
        {
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomePageViewModel>(viewResult.Model);

            Assert.NotNull(model.CompanyProfile);
            Assert.Empty(model.Services);
        }

        [Fact]
        public async Task Index_SecondCallWithinCacheWindow_ReturnsCachedModel_WithoutRequeryingChangedData()
        {
            var first = await _controller.Index();
            var firstModel = Assert.IsType<HomePageViewModel>(Assert.IsType<ViewResult>(first).Model);
            Assert.Empty(firstModel.Services);

            _db.Services.Add(new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _neurixProfileId,
                Slug = "added-after-first-load",
                NameEn = "Added After First Load",
                NameAr = "أضيف بعد التحميل الأول",
                DisplayOrder = 1,
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var second = await _controller.Index();
            var secondModel = Assert.IsType<HomePageViewModel>(Assert.IsType<ViewResult>(second).Model);

            Assert.Empty(secondModel.Services);
            Assert.Same(firstModel, secondModel);
        }

        [Fact]
        public async Task About_ReturnsViewWithCompanyProfile()
        {
            var result = await _controller.About();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsCompanyProfileDetailDto>(viewResult.Model);

            Assert.NotNull(model);
            Assert.Equal("neurix", model.Slug);
            Assert.Equal("Neurix AI", model.NameEn);
        }

        [Fact]
        public async Task Contact_PopulatesCompanyProfileInViewData()
        {
            var result = await _controller.Contact();

            var viewResult = Assert.IsType<ViewResult>(result);
            var profile = viewResult.ViewData["CompanyProfile"] as CmsCompanyProfileDetailDto;

            Assert.NotNull(profile);
            Assert.Equal("neurix", profile.Slug);
        }

        [Fact]
        public async Task Terms_ReturnsNullModel_WhenNoContentPagePublishedYet()
        {
            var result = await _controller.Terms();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task Terms_ReturnsPublishedContentPage_WhenOneExists()
        {
            _db.ContentPages.Add(new CmsContentPage
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _neurixProfileId,
                Slug = "terms",
                TitleEn = "Terms of Service",
                TitleAr = "شروط الخدمة",
                BodyEn = "<p>Terms body</p>",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var result = await _controller.Terms();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsContentPageDetailDto>(viewResult.Model);
            Assert.Equal("terms", model.Slug);
            Assert.Equal("<p>Terms body</p>", model.BodyEn);
        }

        [Fact]
        public async Task Security_ReturnsNullModel_WhenNoContentPagePublishedYet()
        {
            var result = await _controller.Security();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task Insights_ReturnsViewWithPublishedBlogPosts()
        {
            _db.BlogPosts.AddRange(
                new CmsBlogPost
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "human-centric-ai",
                    TitleEn = "Human-Centric AI",
                    TitleAr = "الذكاء الاصطناعي الإنساني",
                    IsPublished = true
                },
                new CmsBlogPost
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "draft-post",
                    TitleEn = "Draft Post",
                    TitleAr = "مسودة",
                    IsPublished = false
                }
            );
            await _db.SaveChangesAsync();

            var result = await _controller.Insights();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IReadOnlyList<CmsBlogPostSummaryDto>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("human-centric-ai", model[0].Slug);
        }

        [Fact]
        public async Task Portfolio_ReturnsViewWithPublishedProjects()
        {
            _db.Projects.AddRange(
                new CmsProject
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "sovereign-cloud",
                    TitleEn = "Sovereign Cloud",
                    TitleAr = "السحابة السيادية",
                    IsPublished = true
                },
                new CmsProject
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = _neurixProfileId,
                    Slug = "internal-draft",
                    TitleEn = "Draft Project",
                    TitleAr = "مشروع مسودة",
                    IsPublished = false
                }
            );
            await _db.SaveChangesAsync();

            var result = await _controller.Portfolio();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IReadOnlyList<CmsProjectSummaryDto>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("sovereign-cloud", model[0].Slug);
        }

        [Fact]
        public async Task Labs_ReturnsViewWithDivisionPage()
        {
            _db.DivisionPages.Add(new CmsDivisionPage
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _neurixProfileId,
                Slug = "labs",
                HeroTitleEn = "Neurix AI Labs — Shaping Tomorrow",
                HeroTitleAr = "مختبرات نيوركس AI — صناعة الغد",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var result = await _controller.Labs();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsDivisionPageDetailDto>(viewResult.Model);

            Assert.NotNull(model);
            Assert.Equal("labs", model.Slug);
            Assert.Equal("Neurix AI Labs — Shaping Tomorrow", model.HeroTitleEn);
        }

        [Fact]
        public async Task Index_PopulatesDynamicHomeSections()
        {
            _db.HeroSections.Add(new CmsHeroSection
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _neurixProfileId,
                BadgeEn = "Dynamic Hero Test",
                BadgeAr = "اختبار البانر الديناميكي",
                TitlePrefixEn = "Prefix ",
                TitleHighlightEn = "Highlight",
                TitleSuffixEn = " Suffix",
                TitlePrefixAr = "بادئة ",
                TitleHighlightAr = "تمييز",
                TitleSuffixAr = " لاحقة",
                SubtitleEn = "Sub EN",
                SubtitleAr = "Sub AR",
                PrimaryButtonTextEn = "Btn1",
                PrimaryButtonTextAr = "زر1",
                PrimaryButtonUrl = "#btn1",
                SecondaryButtonTextEn = "Btn2",
                SecondaryButtonTextAr = "زر2",
                SecondaryButtonUrl = "#btn2",
                Stat1Value = "77+",
                Stat1LabelEn = "Stat1 EN",
                Stat1LabelAr = "Stat1 AR",
                Stat2Value = "88+",
                Stat2LabelEn = "Stat2 EN",
                Stat2LabelAr = "Stat2 AR",
                Stat3Value = "99+",
                Stat3LabelEn = "Stat3 EN",
                Stat3LabelAr = "Stat3 AR",
                IsPublished = true
            });

            _db.HumanVisionSections.Add(new CmsHumanVisionSection
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _neurixProfileId,
                BadgeEn = "Dynamic Vision Test",
                BadgeAr = "اختبار الرؤية الديناميكي",
                TitlePrefixEn = "Prefix ",
                TitleHighlightEn = "Highlight",
                TitlePrefixAr = "بادئة ",
                TitleHighlightAr = "تمييز",
                Paragraph1En = "P1 EN",
                Paragraph1Ar = "P1 AR",
                Paragraph2En = "P2 EN",
                Paragraph2Ar = "P2 AR",
                ImagePath = "/images/test.png",
                ImageAltEn = "Alt EN",
                ImageAltAr = "Alt AR",
                IsPublished = true
            });

            _db.PioneersSections.Add(new CmsPioneersSection
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _neurixProfileId,
                BadgeEn = "Dynamic Pioneers Test",
                BadgeAr = "اختبار الرواد الديناميكي",
                TitlePrefixEn = "Prefix ",
                TitleHighlightEn = "Highlight",
                TitlePrefixAr = "بادئة ",
                TitleHighlightAr = "تمييز",
                Paragraph1En = "P1 EN",
                Paragraph1Ar = "P1 AR",
                Paragraph2En = "P2 EN",
                Paragraph2Ar = "P2 AR",
                ImagePath = "/images/test2.png",
                ImageAltEn = "Alt EN",
                ImageAltAr = "Alt AR",
                IsPublished = true
            });

            await _db.SaveChangesAsync();

            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomePageViewModel>(viewResult.Model);

            Assert.NotNull(model.HeroSection);
            Assert.Equal("Dynamic Hero Test", model.HeroSection.BadgeEn);
            Assert.Equal("77+", model.HeroSection.Stat1Value);

            Assert.NotNull(model.HumanVisionSection);
            Assert.Equal("Dynamic Vision Test", model.HumanVisionSection.BadgeEn);

            Assert.NotNull(model.PioneersSection);
            Assert.Equal("Dynamic Pioneers Test", model.PioneersSection.BadgeEn);
        }

        // Test double for ILeadService
        private class FakeLeadService : ILeadService
        {
            public Task<Guid> CaptureWebsiteEnquiryAsync(WebsiteEnquiry enquiry)
            {
                return Task.FromResult(Guid.NewGuid());
            }

            public Task<PagedResult<LeadListRow>> GetListAsync(string? search, LeadStatus? status, Guid? assignedToUserId, int page, int pageSize = PagingDefaults.DefaultPageSize, bool unassignedOnly = false)
                => throw new NotImplementedException();
            public Task<LeadDetail?> GetDetailAsync(Guid id)
                => throw new NotImplementedException();
            public Task<LeadRequest?> GetForEditAsync(Guid id)
                => throw new NotImplementedException();
            public Task<Guid> CreateAsync(LeadRequest request, Guid currentUserId)
                => throw new NotImplementedException();
            public Task<LeadUpdateOutcome> UpdateAsync(Guid id, LeadRequest request)
                => throw new NotImplementedException();
            public Task<ServiceResult> SoftDeleteAsync(Guid id)
                => throw new NotImplementedException();
            public Task<IReadOnlyList<AssignableUser>> GetAssignableUsersAsync()
                => throw new NotImplementedException();
        }
    }
}
