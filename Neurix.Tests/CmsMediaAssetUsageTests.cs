using System;
using System.Collections.Generic;
using System.IO;
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
    public class CmsMediaAssetUsageTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsMediaAssetService _service;
        private readonly Guid _companyAId;
        private readonly Guid _companyBId;

        public CmsMediaAssetUsageTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsMediaAssetService(_db);

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

        private Guid SeedMediaAsset(
            string filePath,
            Guid? companyProfileId = null,
            string category = "General",
            string originalFileName = "test-asset.png")
        {
            var fileName = Path.GetFileName(filePath.Trim());
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = originalFileName;
            }

            var asset = new CmsMediaAsset
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = companyProfileId ?? _companyAId,
                FileName = fileName,
                OriginalFileName = originalFileName,
                FilePath = filePath,
                FileSizeBytes = 2048,
                ContentType = "image/png",
                Category = category,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.MediaAssets.Add(asset);
            _db.SaveChanges();
            return asset.Id;
        }

        [Fact]
        public async Task GetUsagesAsync_UnknownAssetId_ReturnsEmpty()
        {
            var usages = await _service.GetUsagesAsync(Guid.NewGuid());

            Assert.NotNull(usages);
            Assert.Empty(usages);
        }

        [Fact]
        public async Task GetUsagesAsync_UnreferencedAsset_ReturnsEmpty()
        {
            var assetId = SeedMediaAsset("/uploads/cms/media/general/unreferenced.png");

            var usages = await _service.GetUsagesAsync(assetId);

            Assert.NotNull(usages);
            Assert.Empty(usages);
        }

        [Fact]
        public async Task GetUsagesAsync_ReferencedByServiceImagePath_ReturnsExpectedUsageMetadata()
        {
            var path = "/uploads/cms/services/service-cloud.png";
            var assetId = SeedMediaAsset(path);

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Cloud Solutions",
                NameAr = "حلول سحابية",
                Slug = "cloud-solutions",
                ImagePath = path,
                IsPublished = true
            };
            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            var usage = Assert.Single(usages);
            Assert.Equal("Service", usage.EntityType);
            Assert.Equal(service.Id, usage.EntityId);
            Assert.Equal(nameof(CmsService.ImagePath), usage.PropertyName);
            Assert.Equal("Cloud Solutions", usage.EntityLabel);
            Assert.Equal("CmsServices", usage.ControllerName);
            Assert.Equal("Edit", usage.ActionName);
            Assert.Equal(_companyAId, usage.CompanyProfileId);
        }

        [Fact]
        public async Task GetUsagesAsync_ReferencedByMultipleEntityTypes_ReturnsAllUsagesWithCount()
        {
            var path = "/uploads/cms/media/shared-illustration.png";
            var assetId = SeedMediaAsset(path);

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "AI Consulting",
                NameAr = "استشارات الذكاء الاصطناعي",
                Slug = "ai-consulting",
                ImagePath = path
            };

            var teamMember = new CmsTeamMember
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Alice Smith",
                NameAr = "أليس سميث",
                TitleEn = "Lead Architect",
                TitleAr = "كبير المهندسين",
                PhotoPath = path
            };

            var project = new CmsProject
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                TitleEn = "Smart City Core",
                TitleAr = "منظومة المدينة الذكية",
                Slug = "smart-city-core",
                CoverImagePath = path
            };

            _db.Services.Add(service);
            _db.TeamMembers.Add(teamMember);
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            Assert.Equal(3, usages.Count);
            Assert.Contains(usages, u => u.EntityType == "Service" && u.EntityId == service.Id && u.PropertyName == nameof(CmsService.ImagePath));
            Assert.Contains(usages, u => u.EntityType == "Team Member" && u.EntityId == teamMember.Id && u.PropertyName == nameof(CmsTeamMember.PhotoPath));
            Assert.Contains(usages, u => u.EntityType == "Project" && u.EntityId == project.Id && u.PropertyName == nameof(CmsProject.CoverImagePath));
        }

        [Fact]
        public async Task GetUsagesAsync_ReferencedByBothLogoAndFaviconColumnsOnCompanyProfile_ReturnsBothUsages()
        {
            var path = "/uploads/cms/branding/brand-mark.png";
            var assetId = SeedMediaAsset(path);

            var company = await _db.CompanyProfiles.FindAsync(_companyAId);
            Assert.NotNull(company);
            company.LogoPath = path;
            company.FaviconPath = path;
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            Assert.Equal(2, usages.Count);
            Assert.All(usages, u => Assert.Equal("Company Profile", u.EntityType));
            Assert.All(usages, u => Assert.Equal(_companyAId, u.EntityId));
            Assert.Contains(usages, u => u.PropertyName == nameof(CmsCompanyProfile.LogoPath));
            Assert.Contains(usages, u => u.PropertyName == nameof(CmsCompanyProfile.FaviconPath));
        }

        [Fact]
        public async Task GetUsagesAsync_ReferencedByAllThreeColumnsOfEthicsSection_ReturnsThreeUsages()
        {
            var path = "/uploads/cms/sections/ethics-texture.png";
            var assetId = SeedMediaAsset(path);

            var ethicsSection = new CmsEthicsSection
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                BadgeEn = "Strategy & Ethics",
                TopImagePath = path,
                BottomImagePath = path,
                BackgroundImagePath = path
            };
            _db.EthicsSections.Add(ethicsSection);
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            Assert.Equal(3, usages.Count);
            Assert.All(usages, u => Assert.Equal("Ethics Section", u.EntityType));
            Assert.All(usages, u => Assert.Equal(ethicsSection.Id, u.EntityId));
            Assert.Contains(usages, u => u.PropertyName == nameof(CmsEthicsSection.TopImagePath));
            Assert.Contains(usages, u => u.PropertyName == nameof(CmsEthicsSection.BottomImagePath));
            Assert.Contains(usages, u => u.PropertyName == nameof(CmsEthicsSection.BackgroundImagePath));
        }

        // Regression guard (2026-08-22): SQL pre-filtering in GetUsagesAsync was lossy and dropped paths before
        // PathMatches checked them, risking deletion of referenced assets. The mixed-separator and surrounding-whitespace
        // InlineData rows reproduce the original failure and must not be pruned as duplicate test cases.
        [Theory]
        [InlineData("/uploads/cms/media/general/logo.png")]
        [InlineData("uploads/cms/media/general/logo.png")]
        [InlineData("/UPLOADS/CMS/Media/General/Logo.PNG")]
        [InlineData("\\uploads\\cms\\media\\general\\logo.png")]
        [InlineData("/uploads\\cms/media\\general/logo.png")]
        [InlineData("  /uploads/cms/media/general/logo.png  ")]
        public async Task GetUsagesAsync_PathNormalizationVariants_FindsUsage(string referencingPath)
        {
            var assetPath = "/uploads/cms/media/general/logo.png";
            var assetId = SeedMediaAsset(assetPath);

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Normalized Service Test",
                NameAr = "خدمة اختبار المطابقة",
                Slug = $"norm-{Guid.NewGuid():N}",
                ImagePath = referencingPath
            };
            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            var usage = Assert.Single(usages);
            Assert.Equal("Service", usage.EntityType);
            Assert.Equal(service.Id, usage.EntityId);
            Assert.Equal(nameof(CmsService.ImagePath), usage.PropertyName);
        }

        [Theory]
        [InlineData("/uploads/cms/media/general/logo2.png")]
        [InlineData("/uploads/cms/media/other/logo.png")]
        public async Task GetUsagesAsync_SimilarPaths_DoesNotMatch(string nonMatchingPath)
        {
            var assetPath = "/uploads/cms/media/general/logo.png";
            var assetId = SeedMediaAsset(assetPath);

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Similar Path Service",
                NameAr = "خدمة مسار مشابه",
                Slug = $"similar-{Guid.NewGuid():N}",
                ImagePath = nonMatchingPath
            };
            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            Assert.Empty(usages);
        }

        [Fact]
        public async Task GetUsagesAsync_ReferencingRowIsSoftDeleted_ExcludesFromUsages()
        {
            var path = "/uploads/cms/services/service-retired.png";
            var assetId = SeedMediaAsset(path);

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Retired Service",
                NameAr = "خدمة متقاعدة",
                Slug = "retired-service",
                ImagePath = path,
                IsDeleted = true
            };
            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            var usages = await _service.GetUsagesAsync(assetId);

            Assert.Empty(usages);
        }

        [Fact]
        public async Task SoftDeleteAsync_ReferencedAssetWithoutForce_ReturnsFailureAndDoesNotDelete()
        {
            var path = "/uploads/cms/services/locked-service.png";
            var assetId = SeedMediaAsset(path, originalFileName: "locked-service.png");

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Active Service",
                NameAr = "خدمة نشطة",
                Slug = "active-service",
                ImagePath = path
            };
            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            var result = await _service.SoftDeleteAsync(assetId, force: false);

            Assert.False(result.Success);
            Assert.Contains("1 place", result.ErrorMessage);
            Assert.Contains("locked-service.png", result.ErrorMessage);

            var fetched = await _service.GetByIdAsync(assetId);
            Assert.NotNull(fetched);

            var entity = await _db.MediaAssets.FindAsync(assetId);
            Assert.NotNull(entity);
            Assert.False(entity.IsDeleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_ReferencedAssetWithForceTrue_ReturnsSuccessAndDeletes()
        {
            var path = "/uploads/cms/services/force-deleted-service.png";
            var assetId = SeedMediaAsset(path);

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _companyAId,
                NameEn = "Force Test Service",
                NameAr = "خدمة اختبار الحذف القسري",
                Slug = "force-service",
                ImagePath = path
            };
            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            var result = await _service.SoftDeleteAsync(assetId, force: true);

            Assert.True(result.Success);

            var fetched = await _service.GetByIdAsync(assetId);
            Assert.Null(fetched);

            var raw = await _db.MediaAssets.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == assetId);
            Assert.NotNull(raw);
            Assert.True(raw.IsDeleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_UnreferencedAssetWithoutForce_ReturnsSuccessAndDeletes()
        {
            var assetId = SeedMediaAsset("/uploads/cms/media/unused-standalone.png");

            var result = await _service.SoftDeleteAsync(assetId, force: false);

            Assert.True(result.Success);

            var fetched = await _service.GetByIdAsync(assetId);
            Assert.Null(fetched);

            var raw = await _db.MediaAssets.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == assetId);
            Assert.NotNull(raw);
            Assert.True(raw.IsDeleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_UnknownId_ReturnsFailure()
        {
            var result = await _service.SoftDeleteAsync(Guid.NewGuid(), force: false);

            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetUsageCountsAsync_MixOfReferencedAndUnreferencedAssets_ReturnsCorrectCountsForEachAsset()
        {
            var path1 = "/uploads/cms/media/asset-three-refs.png";
            var path2 = "/uploads/cms/media/asset-one-ref.png";
            var path3 = "/uploads/cms/media/asset-zero-refs.png";

            var asset1Id = SeedMediaAsset(path1, _companyAId);
            var asset2Id = SeedMediaAsset(path2, _companyAId);
            var asset3Id = SeedMediaAsset(path3, _companyAId);

            // Asset 1 referenced 3 times
            _db.Services.Add(new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "S1", NameAr = "خ1", Slug = "s1", ImagePath = path1 });
            _db.TeamMembers.Add(new CmsTeamMember { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "T1", NameAr = "ف1", TitleEn = "Dev", TitleAr = "مطور", PhotoPath = path1 });
            _db.Projects.Add(new CmsProject { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, TitleEn = "P1", TitleAr = "م1", Slug = "p1", CoverImagePath = path1 });

            // Asset 2 referenced 1 time
            _db.BlogPosts.Add(new CmsBlogPost { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, TitleEn = "B1", TitleAr = "ت1", Slug = "b1", CoverImagePath = path2 });

            // Asset 3 unreferenced (0 times)
            await _db.SaveChangesAsync();

            var counts = await _service.GetUsageCountsAsync(_companyAId);

            Assert.Equal(3, counts.Count);

            Assert.True(counts.ContainsKey(asset1Id));
            Assert.Equal(3, counts[asset1Id]);

            Assert.True(counts.ContainsKey(asset2Id));
            Assert.Equal(1, counts[asset2Id]);

            Assert.True(counts.ContainsKey(asset3Id));
            Assert.Equal(0, counts[asset3Id]);
        }

        [Fact]
        public async Task GetUsageCountsAsync_ScopedToCompanyProfileId_ExcludesAssetsFromOtherCompany()
        {
            var pathA = "/uploads/cms/company-a/asset.png";
            var pathB = "/uploads/cms/company-b/asset.png";

            var assetAId = SeedMediaAsset(pathA, _companyAId);
            var assetBId = SeedMediaAsset(pathB, _companyBId);

            _db.Services.Add(new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "SA", NameAr = "خA", Slug = "sa", ImagePath = pathA });
            _db.Services.Add(new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyBId, NameEn = "SB", NameAr = "خB", Slug = "sb", ImagePath = pathB });
            await _db.SaveChangesAsync();

            var countsCompanyA = await _service.GetUsageCountsAsync(_companyAId);

            Assert.Single(countsCompanyA);
            Assert.True(countsCompanyA.ContainsKey(assetAId));
            Assert.Equal(1, countsCompanyA[assetAId]);
            Assert.False(countsCompanyA.ContainsKey(assetBId));
        }

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsSuccessAndPersists()
        {
            var dto = new CmsMediaAssetCreateDto
            {
                CompanyProfileId = _companyAId,
                FileName = "new-upload.png",
                OriginalFileName = "my-upload.png",
                FilePath = "/uploads/cms/media/new-upload.png",
                FileSizeBytes = 4096,
                ContentType = "image/png",
                AltTextEn = "English Alt",
                AltTextAr = "Arabic Alt",
                Category = "Logos",
                Tags = "brand, logo"
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var persisted = await _service.GetByIdAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("new-upload.png", persisted.FileName);
            Assert.Equal("my-upload.png", persisted.OriginalFileName);
            Assert.Equal("/uploads/cms/media/new-upload.png", persisted.FilePath);
            Assert.Equal("Logos", persisted.Category);
            Assert.Equal("English Alt", persisted.AltTextEn);
        }

        [Fact]
        public async Task CreateAsync_NonExistentCompanyProfile_ReturnsFailure()
        {
            var dto = new CmsMediaAssetCreateDto
            {
                CompanyProfileId = Guid.NewGuid(),
                FileName = "missing-comp.png",
                OriginalFileName = "missing-comp.png",
                FilePath = "/uploads/cms/media/missing-comp.png"
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("does not exist", result.ErrorMessage);
        }
    }
}
