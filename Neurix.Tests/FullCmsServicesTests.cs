using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    public class FullCmsServicesTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsSiteSettingService _settingService;
        private readonly CmsTeamMemberService _teamService;
        private readonly CmsBlogPostService _blogService;
        private readonly CmsTestimonialService _testimonialService;
        private readonly CmsProjectService _projectService;
        private readonly CmsDivisionPageService _divisionService;
        private readonly CmsMediaAssetService _mediaService;
        private readonly Guid _companyId;

        public FullCmsServicesTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _settingService = new CmsSiteSettingService(_db);
            _teamService = new CmsTeamMemberService(_db);
            _blogService = new CmsBlogPostService(_db);
            _testimonialService = new CmsTestimonialService(_db);
            _projectService = new CmsProjectService(_db);
            _divisionService = new CmsDivisionPageService(_db);
            _mediaService = new CmsMediaAssetService(_db);

            _companyId = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = _companyId,
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
        public async Task SiteSettingService_SaveBatchAndQuery_WorksSuccessfully()
        {
            var items = new List<CmsSiteSettingUpsertDto>
            {
                new() { CompanyProfileId = _companyId, Key = "contact.phone", ValueEn = "+20 100 000 0000", ValueAr = "+20 100 000 0000", SettingType = "text", GroupName = "Contact", Label = "Phone" },
                new() { CompanyProfileId = _companyId, Key = "contact.email", ValueEn = "contact@neurix.ai", ValueAr = "contact@neurix.ai", SettingType = "text", GroupName = "Contact", Label = "Email" }
            };

            var saveRes = await _settingService.SaveBatchAsync(_companyId, items);
            Assert.True(saveRes.Success);

            var phone = await _settingService.GetSettingValueEnAsync("neurix", "contact.phone");
            Assert.Equal("+20 100 000 0000", phone);

            var list = await _settingService.GetSettingsByCompanyAsync(_companyId);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task TeamMemberService_CRUD_OperatesCorrectly()
        {
            var dto = new CmsTeamMemberUpsertDto
            {
                CompanyProfileId = _companyId,
                NameEn = "Dr. Jane Doe",
                NameAr = "د. جين دو",
                TitleEn = "Principal AI Scientist",
                TitleAr = "كبير باحثي الذكاء الاصطناعي",
                BioEn = "Expert in foundation models.",
                BioAr = "خبيرة في النماذج التأسيسية.",
                DisplayOrder = 1,
                IsPublished = true
            };

            var createRes = await _teamService.CreateAsync(dto);
            Assert.True(createRes.Success);
            Assert.NotEqual(Guid.Empty, createRes.Value);

            var member = await _teamService.GetByIdAsync(createRes.Value);
            Assert.NotNull(member);
            Assert.Equal("Dr. Jane Doe", member.NameEn);

            var published = await _teamService.GetPublishedMembersByCompanySlugAsync("neurix");
            Assert.Single(published);

            // Toggle publish
            var toggleRes = await _teamService.SetPublishedStatusAsync(createRes.Value, false);
            Assert.True(toggleRes.Success);

            var publishedAfter = await _teamService.GetPublishedMembersByCompanySlugAsync("neurix");
            Assert.Empty(publishedAfter);
        }

        [Fact]
        public async Task BlogPostService_CreateAndGetBySlug_EnforcesUniqueness()
        {
            var dto = new CmsBlogPostUpsertDto
            {
                CompanyProfileId = _companyId,
                Slug = "quantum-ai-horizons",
                TitleEn = "Quantum AI Horizons",
                TitleAr = "آفاق الذكاء الاصطناعي الكمومي",
                SummaryEn = "Research synopsis...",
                BodyEn = "Full article content...",
                IsPublished = true,
                IsFeatured = true
            };

            var createRes = await _blogService.CreateAsync(dto);
            Assert.True(createRes.Success);

            // Duplicate slug should fail
            var dupRes = await _blogService.CreateAsync(dto);
            Assert.False(dupRes.Success);

            var fetched = await _blogService.GetBySlugAsync("neurix", "quantum-ai-horizons");
            Assert.NotNull(fetched);
            Assert.Equal("Quantum AI Horizons", fetched.TitleEn);

            var featured = await _blogService.GetFeaturedPostsByCompanySlugAsync("neurix", 5);
            Assert.Single(featured);
        }

        [Fact]
        public async Task TestimonialService_RatingAndFeatured_QueriesProperly()
        {
            var dto = new CmsTestimonialUpsertDto
            {
                CompanyProfileId = _companyId,
                NameEn = "Prof. John Smith",
                NameAr = "أ.د. جون سميث",
                CompanyName = "AI Research Institute",
                QuoteEn = "Exceptional engineering excellence.",
                QuoteAr = "تميز هندسي استثنائي.",
                Rating = 5,
                IsFeatured = true,
                IsPublished = true
            };

            var createRes = await _testimonialService.CreateAsync(dto);
            Assert.True(createRes.Success);

            var featured = await _testimonialService.GetFeaturedTestimonialsByCompanySlugAsync("neurix");
            Assert.Single(featured);
            Assert.Equal(5, featured[0].Rating);
        }

        [Fact]
        public async Task ProjectService_CreateAndGetBySlug_WorksCorrectly()
        {
            var dto = new CmsProjectUpsertDto
            {
                CompanyProfileId = _companyId,
                Slug = "sovereign-ai-os",
                TitleEn = "Sovereign AI Operating Engine",
                TitleAr = "محرك تشغيل الذكاء الاصطناعي السيادي",
                SummaryEn = "A high-performance Linux kernel AI subsystem.",
                SummaryAr = "نظام فرعي عالي الأداء لنواة لينكس.",
                TechnologiesUsed = "C++, CUDA, PyTorch",
                IsFeatured = true,
                IsPublished = true
            };

            var createRes = await _projectService.CreateAsync(dto);
            Assert.True(createRes.Success);

            var project = await _projectService.GetBySlugAsync("neurix", "sovereign-ai-os");
            Assert.NotNull(project);
            Assert.Equal("Sovereign AI Operating Engine", project.TitleEn);
            Assert.Contains("CUDA", project.TechnologiesUsed);
        }

        [Fact]
        public async Task DivisionPageService_UpsertAndGetBySlug_OperatesCorrectly()
        {
            var dto = new CmsDivisionPageUpsertDto
            {
                CompanyProfileId = _companyId,
                Slug = "labs",
                HeroTitleEn = "Neurix AI Labs — The Crucible of Discovery",
                HeroTitleAr = "مختبرات نيوركس AI — مصنع الاستكشاف",
                MissionEn = "Transforming mathematical models into production systems.",
                MissionAr = "تحويل النماذج الرياضية إلى أنظمة إنتاجية.",
                IsPublished = true
            };

            var upsertRes = await _divisionService.UpsertAsync(dto);
            Assert.True(upsertRes.Success);

            var page = await _divisionService.GetBySlugAsync("neurix", "labs");
            Assert.NotNull(page);
            Assert.Equal("Neurix AI Labs — The Crucible of Discovery", page.HeroTitleEn);
        }

        [Fact]
        public async Task MediaAssetService_CreateAndCategoryFilter_OperatesCorrectly()
        {
            var dto = new CmsMediaAssetCreateDto
            {
                CompanyProfileId = _companyId,
                FileName = "logo.svg",
                OriginalFileName = "neurix-logo.svg",
                FilePath = "/uploads/media/logos/logo.svg",
                FileSizeBytes = 15200,
                ContentType = "image/svg+xml",
                Category = "Logos",
                AltTextEn = "Neurix Brand Logo"
            };

            var createRes = await _mediaService.CreateAsync(dto);
            Assert.True(createRes.Success);

            var logos = await _mediaService.GetAssetsByCompanyAsync(_companyId, "Logos");
            Assert.Single(logos);
            Assert.Equal("Logos", logos[0].Category);

            var banners = await _mediaService.GetAssetsByCompanyAsync(_companyId, "Banners");
            Assert.Empty(banners);
        }
    }
}
