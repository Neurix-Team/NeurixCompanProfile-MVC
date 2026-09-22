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
    public class CmsSocialLinkServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsSocialLinkService _service;
        private readonly Guid _companyAId;
        private readonly Guid _companyBId;

        public CmsSocialLinkServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsSocialLinkService(_db, new MemoryCache(new MemoryCacheOptions()));

            _companyAId = Guid.NewGuid();
            _companyBId = Guid.NewGuid();

            _db.CompanyProfiles.AddRange(
                new CmsCompanyProfile { Id = _companyAId, NameEn = "Company A", NameAr = "الشركة أ", Slug = "company-a", IsPublished = true },
                new CmsCompanyProfile { Id = _companyBId, NameEn = "Company B", NameAr = "الشركة ب", Slug = "company-b", IsPublished = true }
            );
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ValidSocialLink_ReturnsSuccessAndPersists()
        {
            var dto = new CmsSocialLinkUpsertDto
            {
                CompanyProfileId = _companyAId,
                Platform = "linkedin",
                Url = "https://www.linkedin.com/company/company-a",
                DisplayName = "Company A LinkedIn",
                IconName = "linkedin",
                DisplayOrder = 1,
                IsPublished = true
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var persisted = await _db.SocialLinks.FindAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("linkedin", persisted.Platform);
            Assert.Equal("https://www.linkedin.com/company/company-a", persisted.Url);
            Assert.Equal(_companyAId, persisted.CompanyProfileId);
            Assert.Equal("linkedin", persisted.IconName);
        }

        [Fact]
        public async Task CreateAsync_InvalidCompanyId_ReturnsFailure()
        {
            var dto = new CmsSocialLinkUpsertDto
            {
                CompanyProfileId = Guid.NewGuid(), // Non-existent
                Platform = "twitter",
                Url = "https://x.com/nonexistent"
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task GetLinksByCompanyAsync_FiltersByCompanyIdCorrectly()
        {
            _db.SocialLinks.AddRange(
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, Platform = "linkedin", Url = "https://linkedin.com/a", DisplayOrder = 1, IsPublished = true },
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, Platform = "twitter", Url = "https://x.com/a", DisplayOrder = 2, IsPublished = true },
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _companyBId, Platform = "github", Url = "https://github.com/b", DisplayOrder = 1, IsPublished = true }
            );
            await _db.SaveChangesAsync();

            var companyALinks = await _service.GetLinksByCompanyAsync(_companyAId);
            var companyBLinks = await _service.GetLinksByCompanyAsync(_companyBId);

            Assert.Equal(2, companyALinks.Count);
            Assert.All(companyALinks, s => Assert.Equal(_companyAId, s.CompanyProfileId));

            Assert.Single(companyBLinks);
            Assert.Equal("github", companyBLinks[0].Platform);
        }

        [Fact]
        public async Task GetPublishedLinksByCompanySlugAsync_ReturnsOnlyPublishedAndOrdered()
        {
            _db.SocialLinks.AddRange(
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, Platform = "youtube", Url = "https://youtube.com/a", DisplayOrder = 2, IsPublished = true },
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, Platform = "linkedin", Url = "https://linkedin.com/a", DisplayOrder = 1, IsPublished = true },
                new CmsSocialLink { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, Platform = "draft-link", Url = "https://draft.com", DisplayOrder = 0, IsPublished = false }
            );
            await _db.SaveChangesAsync();

            var published = await _service.GetPublishedLinksByCompanySlugAsync("company-a");

            Assert.Equal(2, published.Count);
            Assert.Equal("linkedin", published[0].Platform);
            Assert.Equal("youtube", published[1].Platform);
        }

        [Fact]
        public async Task SoftDeleteAsync_ExcludesFromSubsequentQueries()
        {
            var linkId = Guid.NewGuid();
            _db.SocialLinks.Add(new CmsSocialLink
            {
                Id = linkId,
                CompanyProfileId = _companyAId,
                Platform = "facebook",
                Url = "https://facebook.com/a",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var deleteResult = await _service.SoftDeleteAsync(linkId);
            Assert.True(deleteResult.Success);

            var fetched = await _service.GetByIdAsync(linkId);
            Assert.Null(fetched);

            var list = await _service.GetLinksByCompanyAsync(_companyAId, includeUnpublished: true);
            Assert.DoesNotContain(list, s => s.Id == linkId);
        }

        [Fact]
        public async Task SetPublishedStatusAsync_UpdatesStatusSuccessfully()
        {
            var linkId = Guid.NewGuid();
            _db.SocialLinks.Add(new CmsSocialLink
            {
                Id = linkId,
                CompanyProfileId = _companyAId,
                Platform = "discord",
                Url = "https://discord.gg/a",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var unpublishResult = await _service.SetPublishedStatusAsync(linkId, false);
            Assert.True(unpublishResult.Success);

            var updated = await _db.SocialLinks.FindAsync(linkId);
            Assert.NotNull(updated);
            Assert.False(updated.IsPublished);
        }
    }
}
