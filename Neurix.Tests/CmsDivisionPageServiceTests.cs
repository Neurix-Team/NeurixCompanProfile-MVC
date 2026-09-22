using System;
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
    public class CmsDivisionPageServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsDivisionPageService _service;
        private readonly Guid _profileId;

        public CmsDivisionPageServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsDivisionPageService(_db);

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

        private CmsDivisionPageUpsertDto NewDto(string slug = "labs") => new()
        {
            CompanyProfileId = _profileId,
            Slug = slug,
            HeroTitleEn = "Neurix AI Labs",
            HeroTitleAr = "مختبرات نيوركس AI",
            HeroSubtitleEn = "R&D center",
            HeroSubtitleAr = "مركز البحث والتطوير",
            MissionEn = "Applied research mandate",
            MissionAr = "رسالة البحث التطبيقي",
            CoverImagePath = "/uploads/cms/divisions/labs.png",
            IsPublished = true
        };

        [Fact]
        public async Task UpsertAsync_CreatesNewDivisionPage_WhenNoneExists()
        {
            var result = await _service.UpsertAsync(NewDto());

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var saved = await _db.DivisionPages.SingleAsync();
            Assert.Equal("labs", saved.Slug);
            Assert.Equal("Neurix AI Labs", saved.HeroTitleEn);
            Assert.Equal("/uploads/cms/divisions/labs.png", saved.CoverImagePath);
        }

        [Fact]
        public async Task UpsertAsync_UpdatesInPlace_ForSameCompanyAndSlug()
        {
            var first = await _service.UpsertAsync(NewDto());

            var edit = NewDto();
            edit.HeroTitleEn = "Updated Labs Title";
            edit.CoverImagePath = "/uploads/cms/divisions/labs-updated.png";
            var second = await _service.UpsertAsync(edit);

            Assert.True(second.Success);
            Assert.Equal(first.Value, second.Value);
            Assert.Equal(1, await _db.DivisionPages.CountAsync());

            var updated = await _db.DivisionPages.SingleAsync();
            Assert.Equal("Updated Labs Title", updated.HeroTitleEn);
            Assert.Equal("/uploads/cms/divisions/labs-updated.png", updated.CoverImagePath);
        }

        [Fact]
        public async Task UpsertAsync_Fails_WhenCompanyProfileMissing()
        {
            var dto = NewDto();
            dto.CompanyProfileId = Guid.NewGuid();

            var result = await _service.UpsertAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("company profile", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsPublishedDivisionPage()
        {
            await _service.UpsertAsync(NewDto("technology"));

            var page = await _service.GetBySlugAsync("neurix", "technology");

            Assert.NotNull(page);
            Assert.Equal("technology", page!.Slug);
            Assert.Equal("Neurix AI Labs", page.HeroTitleEn);
            Assert.Equal("/uploads/cms/divisions/labs.png", page.CoverImagePath);
        }

        [Fact]
        public async Task GetBySlugAsync_IsCaseInsensitive()
        {
            await _service.UpsertAsync(NewDto("hq"));

            var page = await _service.GetBySlugAsync("NEURIX", "HQ");

            Assert.NotNull(page);
            Assert.Equal("hq", page!.Slug);
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsNull_WhenUnpublished()
        {
            var dto = NewDto("plus");
            dto.IsPublished = false;
            await _service.UpsertAsync(dto);

            Assert.Null(await _service.GetBySlugAsync("neurix", "plus"));
        }

        [Fact]
        public async Task GetDivisionPagesByCompanyAsync_ReturnsAllDivisions()
        {
            await _service.UpsertAsync(NewDto("labs"));
            await _service.UpsertAsync(NewDto("technology"));
            await _service.UpsertAsync(NewDto("hq"));

            var list = await _service.GetDivisionPagesByCompanyAsync(_profileId, includeUnpublished: true);

            Assert.Equal(3, list.Count);
            Assert.Contains(list, d => d.Slug == "labs");
            Assert.Contains(list, d => d.Slug == "technology");
            Assert.Contains(list, d => d.Slug == "hq");
        }

        [Fact]
        public async Task SetPublishedStatusAsync_TogglesPublish()
        {
            var created = await _service.UpsertAsync(NewDto("club"));

            var toggleResult = await _service.SetPublishedStatusAsync(created.Value, false);
            Assert.True(toggleResult.Success);

            var page = await _service.GetByIdAsync(created.Value);
            Assert.NotNull(page);
            Assert.False(page!.IsPublished);
        }
    }
}
