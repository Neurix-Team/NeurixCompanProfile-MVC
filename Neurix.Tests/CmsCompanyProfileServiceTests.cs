using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    public class CmsCompanyProfileServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsCompanyProfileService _service;

        public CmsCompanyProfileServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsCompanyProfileService(_db, new MemoryCache(new MemoryCacheOptions()));
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ValidProfile_ReturnsSuccessAndPersistsData()
        {
            var dto = new CmsCompanyProfileUpsertDto
            {
                NameEn = "Test Brand",
                NameAr = "علامة تجارية",
                Slug = "test-brand",
                TaglineEn = "Innovation First",
                TaglineAr = "الابتكار أولاً",
                Email = "test@brand.com",
                IsPublished = true
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var persisted = await _db.CompanyProfiles.FindAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("test-brand", persisted.Slug);
            Assert.Equal("Test Brand", persisted.NameEn);
            Assert.Equal("علامة تجارية", persisted.NameAr);
            Assert.True(persisted.IsPublished);
            Assert.False(persisted.IsDeleted);
        }

        [Fact]
        public async Task CreateAsync_DuplicateSlug_ReturnsFailure()
        {
            var dto1 = new CmsCompanyProfileUpsertDto
            {
                NameEn = "Brand One",
                NameAr = "علامة 1",
                Slug = "duplicate-slug"
            };

            var dto2 = new CmsCompanyProfileUpsertDto
            {
                NameEn = "Brand Two",
                NameAr = "علامة 2",
                Slug = "duplicate-slug"
            };

            var result1 = await _service.CreateAsync(dto1);
            var result2 = await _service.CreateAsync(dto2);

            Assert.True(result1.Success);
            Assert.False(result2.Success);
            Assert.Contains("already exists", result2.ErrorMessage);
        }

        [Fact]
        public async Task GetAllProfilesAsync_WhenIncludeUnpublishedIsFalse_ReturnsOnlyPublished()
        {
            _db.CompanyProfiles.AddRange(
                new CmsCompanyProfile { Id = Guid.NewGuid(), NameEn = "Pub", NameAr = "منشور", Slug = "pub", IsPublished = true },
                new CmsCompanyProfile { Id = Guid.NewGuid(), NameEn = "Draft", NameAr = "مسودة", Slug = "draft", IsPublished = false }
            );
            await _db.SaveChangesAsync();

            var publishedOnly = await _service.GetAllProfilesAsync(includeUnpublished: false);
            var allProfiles = await _service.GetAllProfilesAsync(includeUnpublished: true);

            Assert.Single(publishedOnly);
            Assert.Equal("pub", publishedOnly[0].Slug);
            Assert.Equal(2, allProfiles.Count);
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsCorrectProfile()
        {
            var id = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = id,
                NameEn = "Neurix AI",
                NameAr = "نيوركس AI",
                Slug = "neurix",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var profile = await _service.GetBySlugAsync("neurix");

            Assert.NotNull(profile);
            Assert.Equal(id, profile.Id);
            Assert.Equal("Neurix AI", profile.NameEn);
        }

        [Fact]
        public async Task GetDefaultPublishedProfileAsync_PrefersNeurixSlug()
        {
            _db.CompanyProfiles.AddRange(
                new CmsCompanyProfile { Id = Guid.NewGuid(), NameEn = "Alpha Brand", NameAr = "ألفا", Slug = "alpha", IsPublished = true },
                new CmsCompanyProfile { Id = Guid.NewGuid(), NameEn = "Neurix AI", NameAr = "نيوركس", Slug = "neurix", IsPublished = true },
                new CmsCompanyProfile { Id = Guid.NewGuid(), NameEn = "Beta Brand", NameAr = "بيتا", Slug = "beta", IsPublished = true }
            );
            await _db.SaveChangesAsync();

            var defaultProfile = await _service.GetDefaultPublishedProfileAsync();

            Assert.NotNull(defaultProfile);
            Assert.Equal("neurix", defaultProfile.Slug);
        }

        [Fact]
        public async Task UpdateAsync_ValidChanges_UpdatesDatabase()
        {
            var id = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = id,
                NameEn = "Old Name",
                NameAr = "اسم قديم",
                Slug = "old-slug",
                IsPublished = false
            });
            await _db.SaveChangesAsync();

            var updateDto = new CmsCompanyProfileUpsertDto
            {
                NameEn = "New Name",
                NameAr = "اسم جديد",
                Slug = "new-slug",
                IsPublished = true,
                TaglineEn = "New Tagline"
            };

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.True(result.Success);

            var updated = await _db.CompanyProfiles.FindAsync(id);
            Assert.NotNull(updated);
            Assert.Equal("New Name", updated.NameEn);
            Assert.Equal("new-slug", updated.Slug);
            Assert.True(updated.IsPublished);
            Assert.Equal("New Tagline", updated.TaglineEn);
        }

        [Fact]
        public async Task SoftDeleteAsync_SetsIsDeleted_HidesFromQueryFilter()
        {
            var id = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = id,
                NameEn = "To Delete",
                NameAr = "للحذف",
                Slug = "to-delete",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var deleteResult = await _service.SoftDeleteAsync(id);
            Assert.True(deleteResult.Success);

            // Verify via service GetById
            var fetched = await _service.GetByIdAsync(id);
            Assert.Null(fetched);

            // Verify via GetAllProfiles
            var all = await _service.GetAllProfilesAsync(includeUnpublished: true);
            Assert.DoesNotContain(all, p => p.Id == id);
        }

        [Fact]
        public async Task CreateAndGetBySlug_WithBrandColorsAndFavicon_PersistsAndMapsCorrectly()
        {
            var dto = new CmsCompanyProfileUpsertDto
            {
                NameEn = "Neurix Labs",
                NameAr = "مختبرات نيوركس",
                Slug = "neurix-labs",
                LogoPath = "/uploads/cms/logos/brand-logo.png",
                FaviconPath = "/uploads/cms/favicons/brand-fav.svg",
                PrimaryColor = "#00C2D4",
                AccentColor = "#5B5FEF",
                Website = "https://neurix.uk",
                IsPublished = true
            };

            var createResult = await _service.CreateAsync(dto);
            Assert.True(createResult.Success);

            var detail = await _service.GetBySlugAsync("neurix-labs");
            Assert.NotNull(detail);
            Assert.Equal("#00C2D4", detail.PrimaryColor);
            Assert.Equal("#5B5FEF", detail.AccentColor);
            Assert.Equal("/uploads/cms/logos/brand-logo.png", detail.LogoPath);
            Assert.Equal("/uploads/cms/favicons/brand-fav.svg", detail.FaviconPath);
            Assert.Equal("https://neurix.uk", detail.Website);

            var summaries = await _service.GetAllProfilesAsync(includeUnpublished: true);
            var summary = summaries.FirstOrDefault(s => s.Slug == "neurix-labs");
            Assert.NotNull(summary);
            Assert.Equal("#00C2D4", summary.PrimaryColor);
            Assert.Equal("#5B5FEF", summary.AccentColor);
            Assert.Equal("/uploads/cms/favicons/brand-fav.svg", summary.FaviconPath);
            Assert.Equal("/uploads/cms/logos/brand-logo.png", summary.LogoPath);
        }

        [Fact]
        public async Task UpdateAsync_BrandColorsAndFavicon_UpdatesPersistedRecord()
        {
            var id = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = id,
                NameEn = "Brand",
                NameAr = "علامة",
                Slug = "brand",
                PrimaryColor = "#000000",
                AccentColor = "#111111",
                LogoPath = "/old-logo.png",
                FaviconPath = "/old-fav.ico",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var updateDto = new CmsCompanyProfileUpsertDto
            {
                NameEn = "Brand Updated",
                NameAr = "علامة محدثة",
                Slug = "brand",
                PrimaryColor = "#00C2D4",
                AccentColor = "#5B5FEF",
                LogoPath = "/new-logo.png",
                FaviconPath = "/new-fav.svg",
                Website = "https://example.com",
                IsPublished = true
            };

            var updateResult = await _service.UpdateAsync(id, updateDto);
            Assert.True(updateResult.Success);

            var updated = await _service.GetByIdAsync(id);
            Assert.NotNull(updated);
            Assert.Equal("#00C2D4", updated.PrimaryColor);
            Assert.Equal("#5B5FEF", updated.AccentColor);
            Assert.Equal("/new-logo.png", updated.LogoPath);
            Assert.Equal("/new-fav.svg", updated.FaviconPath);
            Assert.Equal("https://example.com", updated.Website);
        }
    }
}
