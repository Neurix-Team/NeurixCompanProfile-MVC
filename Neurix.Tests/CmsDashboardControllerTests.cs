using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Services.Cms;
using Neurix.Controllers;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Neurix.Models;
using Xunit;

namespace Neurix.Tests
{
    public class CmsDashboardControllerTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsCompanyProfileService _profileService;
        private readonly CmsServiceService _serviceService;
        private readonly CmsSocialLinkService _socialLinkService;
        private readonly CmsTeamMemberService _teamService;
        private readonly CmsBlogPostService _blogService;
        private readonly CmsProjectService _projectService;
        private readonly CmsTestimonialService _testimonialService;
        private readonly CmsMediaAssetService _mediaService;
        private readonly CmsDashboardController _controller;
        private readonly Guid _neurixProfileId;
        private readonly Guid _daleelProfileId;

        public CmsDashboardControllerTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _profileService = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));
            _serviceService = new CmsServiceService(_db);
            _socialLinkService = new CmsSocialLinkService(_db, new MemoryCache(new MemoryCacheOptions()));
            _teamService = new CmsTeamMemberService(_db);
            _blogService = new CmsBlogPostService(_db);
            _projectService = new CmsProjectService(_db);
            _testimonialService = new CmsTestimonialService(_db);
            _mediaService = new CmsMediaAssetService(_db);

            _controller = new CmsDashboardController(
                _profileService,
                _serviceService,
                _socialLinkService,
                _teamService,
                _blogService,
                _projectService,
                _testimonialService,
                _mediaService,
                NullLogger<CmsDashboardController>.Instance
            );

            _neurixProfileId = Guid.NewGuid();
            _daleelProfileId = Guid.NewGuid();

            _db.CompanyProfiles.AddRange(
                new CmsCompanyProfile { Id = _neurixProfileId, NameEn = "Neurix AI", NameAr = "نيوركس AI", Slug = "neurix", IsPublished = true },
                new CmsCompanyProfile { Id = _daleelProfileId, NameEn = "Daleel", NameAr = "دليل", Slug = "daleel", IsPublished = false } // 1 published, 1 draft
            );

            _db.Services.AddRange(
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _neurixProfileId, NameEn = "R&D", NameAr = "بحث", Slug = "rd", IsPublished = true },
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _neurixProfileId, NameEn = "Software", NameAr = "برمجيات", Slug = "software", IsPublished = true },
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _daleelProfileId, NameEn = "Guidance", NameAr = "إرشاد", Slug = "guidance", IsPublished = false }
            );

            _db.SocialLinks.AddRange(
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _neurixProfileId, Platform = "linkedin", Url = "https://linkedin.com/neurix", IsPublished = true },
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _neurixProfileId, Platform = "twitter", Url = "https://x.com/neurix", IsPublished = true },
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _daleelProfileId, Platform = "globe", Url = "https://aidaleel.com", IsPublished = true }
            );

            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task Index_ReturnsViewWithCmsDashboardViewModelAggregatingStats()
        {
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsDashboardViewModel>(viewResult.Model);

            Assert.Equal(2, model.TotalProfilesCount);
            Assert.Equal(1, model.PublishedProfilesCount);

            Assert.Equal(3, model.TotalServicesCount);
            Assert.Equal(2, model.PublishedServicesCount);

            Assert.Equal(3, model.TotalSocialLinksCount);
            Assert.Equal(3, model.PublishedSocialLinksCount);
        }

        [Fact]
        public async Task Index_CalculatesPerBrandServiceAndSocialLinkCountsCorrectly()
        {
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsDashboardViewModel>(viewResult.Model);

            var neurixSummary = model.BrandSummaries.FirstOrDefault(b => b.Id == _neurixProfileId);
            var daleelSummary = model.BrandSummaries.FirstOrDefault(b => b.Id == _daleelProfileId);

            Assert.NotNull(neurixSummary);
            Assert.True(neurixSummary.IsPublished);
            Assert.Equal(2, neurixSummary.ServicesCount);
            Assert.Equal(2, neurixSummary.SocialLinksCount);

            Assert.NotNull(daleelSummary);
            Assert.False(daleelSummary.IsPublished);
            Assert.Equal(1, daleelSummary.ServicesCount);
            Assert.Equal(1, daleelSummary.SocialLinksCount);
        }

        [Fact]
        public async Task Index_PopulatesRecentServicesAndSocialLinks()
        {
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CmsDashboardViewModel>(viewResult.Model);

            Assert.NotEmpty(model.RecentServices);
            Assert.NotEmpty(model.RecentSocialLinks);
            Assert.True(model.RecentServices.Count <= 5);
            Assert.True(model.RecentSocialLinks.Count <= 5);
        }
    }
}
