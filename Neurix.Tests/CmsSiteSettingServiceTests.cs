using System;
using System.Collections.Generic;
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
    public class CmsSiteSettingServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsSiteSettingService _service;
        private readonly Guid _companyAId;
        private readonly Guid _companyBId;

        public CmsSiteSettingServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsSiteSettingService(_db);

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
        public async Task CreateAsync_ValidDto_ReturnsSuccessAndPersists()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "site.title",
                ValueEn = "Neurix",
                ValueAr = "نيوركس",
                SettingType = "text",
                GroupName = "General",
                Label = "Site Title"
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var settings = await _service.GetSettingsByCompanyAsync(_companyAId);
            var persisted = Assert.Single(settings);
            Assert.Equal("site.title", persisted.Key);
            Assert.Equal("Neurix", persisted.ValueEn);
            Assert.Equal("نيوركس", persisted.ValueAr);
            Assert.Equal("text", persisted.SettingType);
            Assert.Equal("General", persisted.GroupName);
            Assert.Equal("Site Title", persisted.Label);
            Assert.Equal(_companyAId, persisted.CompanyProfileId);
        }

        [Fact]
        public async Task GetSettingsByCompanySlugAsync_ReflectsEditsAfterRevisionAdvances()
        {
            var revision = new CmsContentRevision();
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new CmsDbContext(options, revision);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CmsSiteSettingService(db, cache, revision);
            db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = Guid.NewGuid(), Slug = "neurix", NameEn = "Neurix", NameAr = "نيوركس", IsPublished = true
            });
            await db.SaveChangesAsync();
            var companyId = (await db.CompanyProfiles.SingleAsync()).Id;
            await service.CreateAsync(new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = companyId, Key = "navbar.cta", ValueEn = "Start a Project",
                ValueAr = "ابدأ مشروعك", GroupName = "General", Label = "Navigation button"
            });

            var first = await service.GetSettingsByCompanySlugAsync("neurix");
            Assert.Equal("Start a Project", Assert.Single(first).ValueEn);
            await service.UpsertAsync(new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = companyId, Key = "navbar.cta", ValueEn = "Contact our team",
                ValueAr = "تواصل مع الفريق", GroupName = "General", Label = "Navigation button"
            });

            var updated = await service.GetSettingsByCompanySlugAsync("neurix");
            Assert.Equal("Contact our team", Assert.Single(updated).ValueEn);
        }

        [Fact]
        public async Task CreateAsync_DuplicateKeyForSameCompany_ReturnsFailureAndDoesNotCreateSecondRow()
        {
            var dto1 = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "seo.title",
                ValueEn = "First Title",
                SettingType = "text",
                GroupName = "SEO",
                Label = "Page Title"
            };

            var dto2 = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "seo.title",
                ValueEn = "Duplicate Title",
                SettingType = "text",
                GroupName = "SEO",
                Label = "Duplicate Page Title"
            };

            var result1 = await _service.CreateAsync(dto1);
            var result2 = await _service.CreateAsync(dto2);

            Assert.True(result1.Success);
            Assert.False(result2.Success);
            Assert.Contains("seo.title", result2.ErrorMessage);

            var settings = await _service.GetSettingsByCompanyAsync(_companyAId);
            Assert.Single(settings);
            Assert.Equal("First Title", settings[0].ValueEn);
        }

        [Fact]
        public async Task CreateAsync_SameKeyDifferentCompany_ReturnsSuccessForBoth()
        {
            var dtoCompanyA = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "seo.title",
                ValueEn = "Company A SEO Title",
                SettingType = "text",
                GroupName = "SEO",
                Label = "SEO Title"
            };

            var dtoCompanyB = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyBId,
                Key = "seo.title",
                ValueEn = "Company B SEO Title",
                SettingType = "text",
                GroupName = "SEO",
                Label = "SEO Title"
            };

            var resultA = await _service.CreateAsync(dtoCompanyA);
            var resultB = await _service.CreateAsync(dtoCompanyB);

            Assert.True(resultA.Success);
            Assert.True(resultB.Success);

            var settingsA = await _service.GetSettingsByCompanyAsync(_companyAId);
            var settingsB = await _service.GetSettingsByCompanyAsync(_companyBId);

            Assert.Single(settingsA);
            Assert.Single(settingsB);
            Assert.Equal("Company A SEO Title", settingsA[0].ValueEn);
            Assert.Equal("Company B SEO Title", settingsB[0].ValueEn);
        }

        [Fact]
        public async Task CreateAsync_UnnormalizedKey_NormalizesToLowercaseAndTrims()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "  SEO.OG.Title  ",
                ValueEn = "OG Title",
                SettingType = "text",
                GroupName = "SEO",
                Label = "Open Graph Title"
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);

            var persisted = await _db.SiteSettings.FindAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("seo.og.title", persisted.Key);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("seo..title")]
        [InlineData(".seo")]
        [InlineData("seo.")]
        [InlineData("seo og")]
        [InlineData("seo_og")]
        [InlineData("seo-og")]
        [InlineData("seo/og")]
        public async Task CreateAsync_MalformedKey_ReturnsFailureAndPersistsNothing(string invalidKey)
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = invalidKey,
                ValueEn = "Test Value",
                SettingType = "text",
                GroupName = "SEO",
                Label = "Invalid Key Test"
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Success);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
            Assert.Empty(_db.SiteSettings);
        }

        [Fact]
        public async Task CreateAsync_UnknownSettingType_ReturnsFailureNamingType()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "general.richtext",
                ValueEn = "<p>rich</p>",
                SettingType = "richtext",
                GroupName = "General",
                Label = "Rich Text"
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("richtext", result.ErrorMessage);
            Assert.Empty(_db.SiteSettings);
        }

        [Fact]
        public async Task CreateAsync_UnknownGroupName_ReturnsFailureNamingGroup()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "marketing.campaign",
                ValueEn = "Summer",
                SettingType = "text",
                GroupName = "Marketing",
                Label = "Campaign"
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("Marketing", result.ErrorMessage);
            Assert.Empty(_db.SiteSettings);
        }

        [Fact]
        public async Task CreateAsync_AllowedSettingTypeAndGroupWithDifferentCasing_ReturnsSuccess()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "seo.meta.title",
                ValueEn = "Case Insensitive Allowed Values",
                SettingType = "TEXT",
                GroupName = "seo",
                Label = "SEO Title"
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);

            var persisted = await _db.SiteSettings.FindAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("TEXT", persisted.SettingType);
            Assert.Equal("seo", persisted.GroupName);
        }

        [Fact]
        public async Task UpsertAsync_ExistingKey_UpdatesInPlaceWithoutDuplicate()
        {
            var createDto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "site.tagline",
                ValueEn = "Initial Tagline",
                ValueAr = "شعار أولي",
                SettingType = "text",
                GroupName = "General",
                Label = "Tagline"
            };
            var createResult = await _service.CreateAsync(createDto);
            Assert.True(createResult.Success);
            var initialId = createResult.Value;

            var upsertDto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "site.tagline",
                ValueEn = "Updated Tagline",
                ValueAr = "شعار محدث",
                SettingType = "text",
                GroupName = "General",
                Label = "Tagline Updated"
            };
            var upsertResult = await _service.UpsertAsync(upsertDto);

            Assert.True(upsertResult.Success);
            Assert.Equal(initialId, upsertResult.Value);

            var settings = await _service.GetSettingsByCompanyAsync(_companyAId);
            var updated = Assert.Single(settings);
            Assert.Equal(initialId, updated.Id);
            Assert.Equal("Updated Tagline", updated.ValueEn);
            Assert.Equal("شعار محدث", updated.ValueAr);
            Assert.Equal("Tagline Updated", updated.Label);
        }

        [Fact]
        public async Task UpsertAsync_NewKey_InsertsNewSetting()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "contact.support.email",
                ValueEn = "support@neurix.com",
                SettingType = "text",
                GroupName = "Contact",
                Label = "Support Email"
            };

            var result = await _service.UpsertAsync(dto);

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var persisted = await _db.SiteSettings.FindAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("contact.support.email", persisted.Key);
            Assert.Equal("support@neurix.com", persisted.ValueEn);
        }

        [Fact]
        public async Task UpsertAsync_NormalizesKey_CausesSubsequentCreateToDetectDuplicate()
        {
            var upsertDto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "SEO.Site.Title",
                ValueEn = "Original Title",
                SettingType = "text",
                GroupName = "SEO",
                Label = "SEO Site Title"
            };

            var upsertResult = await _service.UpsertAsync(upsertDto);
            Assert.True(upsertResult.Success);

            var createDto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "seo.site.title",
                ValueEn = "Duplicate Create Attempt",
                SettingType = "text",
                GroupName = "SEO",
                Label = "SEO Site Title"
            };

            var createResult = await _service.CreateAsync(createDto);

            Assert.False(createResult.Success);
            Assert.Contains("already exists", createResult.ErrorMessage);
        }

        [Fact]
        public async Task SaveBatchAsync_AllValidItems_ReturnsSuccessAndPersistsAll()
        {
            var items = new List<CmsSiteSettingUpsertDto>
            {
                new()
                {
                    CompanyProfileId = _companyAId,
                    Key = "general.brandname",
                    ValueEn = "Neurix Corp",
                    SettingType = "text",
                    GroupName = "General",
                    Label = "Brand Name"
                },
                new()
                {
                    CompanyProfileId = _companyAId,
                    Key = "seo.metadescription",
                    ValueEn = "Neurix official website meta description",
                    SettingType = "textarea",
                    GroupName = "SEO",
                    Label = "Meta Description"
                },
                new()
                {
                    CompanyProfileId = _companyAId,
                    Key = "footer.copyright",
                    ValueEn = "© 2026 Neurix. All rights reserved.",
                    SettingType = "text",
                    GroupName = "Footer",
                    Label = "Copyright"
                }
            };

            var result = await _service.SaveBatchAsync(_companyAId, items);

            Assert.True(result.Success);

            var settings = await _service.GetSettingsByCompanyAsync(_companyAId);
            Assert.Equal(3, settings.Count);
            Assert.Contains(settings, s => s.Key == "general.brandname" && s.ValueEn == "Neurix Corp");
            Assert.Contains(settings, s => s.Key == "seo.metadescription" && s.ValueEn == "Neurix official website meta description");
            Assert.Contains(settings, s => s.Key == "footer.copyright" && s.ValueEn == "© 2026 Neurix. All rights reserved.");
        }

        // Regression guard (2026-08-22): SaveBatchAsync previously discarded the ServiceResult returned
        // by UpsertAsync and unconditionally returned Ok(), causing invalid rows to silently fail while
        // reporting success. This test ensures per-item validation failures are properly surfaced.
        [Fact]
        public async Task SaveBatchAsync_WithOneInvalidItemAmongValidOnes_ReturnsFailureNamingOffendingKey()
        {
            var items = new List<CmsSiteSettingUpsertDto>
            {
                new()
                {
                    CompanyProfileId = _companyAId,
                    Key = "general.validone",
                    ValueEn = "Valid 1",
                    SettingType = "text",
                    GroupName = "General",
                    Label = "Valid 1"
                },
                new()
                {
                    CompanyProfileId = _companyAId,
                    Key = "invalid_key_name",
                    ValueEn = "Invalid Key",
                    SettingType = "text",
                    GroupName = "General",
                    Label = "Invalid"
                },
                new()
                {
                    CompanyProfileId = _companyAId,
                    Key = "general.validtwo",
                    ValueEn = "Valid 2",
                    SettingType = "text",
                    GroupName = "General",
                    Label = "Valid 2"
                }
            };

            var result = await _service.SaveBatchAsync(_companyAId, items);

            Assert.False(result.Success);
            Assert.Contains("invalid_key_name", result.ErrorMessage);
        }

        [Fact]
        public async Task SaveBatchAsync_ItemCarriesDifferentCompanyProfileId_ForcesToBatchCompanyProfileId()
        {
            var items = new List<CmsSiteSettingUpsertDto>
            {
                new()
                {
                    CompanyProfileId = _companyBId, // Intentionally different
                    Key = "general.overriddenbrand",
                    ValueEn = "Should belong to Company A",
                    SettingType = "text",
                    GroupName = "General",
                    Label = "Overridden Brand"
                }
            };

            var result = await _service.SaveBatchAsync(_companyAId, items);

            Assert.True(result.Success);

            var companyASettings = await _service.GetSettingsByCompanyAsync(_companyAId);
            var companyBSettings = await _service.GetSettingsByCompanyAsync(_companyBId);

            var itemA = Assert.Single(companyASettings);
            Assert.Equal("general.overriddenbrand", itemA.Key);
            Assert.Equal(_companyAId, itemA.CompanyProfileId);

            Assert.Empty(companyBSettings);
        }

        [Fact]
        public async Task SoftDeleteAsync_ExistingSetting_ReturnsSuccessAndHidesFromSubsequentQueries()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "footer.disclaimer",
                ValueEn = "Disclaimer text",
                SettingType = "text",
                GroupName = "Footer",
                Label = "Disclaimer"
            };
            var createResult = await _service.CreateAsync(dto);
            Assert.True(createResult.Success);
            var settingId = createResult.Value;

            var deleteResult = await _service.SoftDeleteAsync(settingId);

            Assert.True(deleteResult.Success);

            var activeSettings = await _service.GetSettingsByCompanyAsync(_companyAId);
            Assert.DoesNotContain(activeSettings, s => s.Id == settingId);

            var rawEntity = await _db.SiteSettings.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == settingId);
            Assert.NotNull(rawEntity);
            Assert.True(rawEntity.IsDeleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_UnknownId_ReturnsFailure()
        {
            var result = await _service.SoftDeleteAsync(Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage);
        }

        // Regression guard (2026-08-22): SoftDeleteAsync must use FirstOrDefaultAsync rather than FindAsync.
        // FindAsync consults EF Core's change tracker first and returns tracked soft-deleted entities,
        // bypassing HasQueryFilter(!e.IsDeleted) and causing repeated deletions to falsely report success.
        [Fact]
        public async Task SoftDeleteAsync_CalledTwiceOnSameId_SecondCallReturnsFailure()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "general.todeletetwice",
                ValueEn = "Delete Me",
                SettingType = "text",
                GroupName = "General",
                Label = "Delete Twice"
            };
            var createResult = await _service.CreateAsync(dto);
            Assert.True(createResult.Success);
            var settingId = createResult.Value;

            var firstDeleteResult = await _service.SoftDeleteAsync(settingId);
            var secondDeleteResult = await _service.SoftDeleteAsync(settingId);

            Assert.True(firstDeleteResult.Success);
            Assert.False(secondDeleteResult.Success);
            Assert.Contains("not found", secondDeleteResult.ErrorMessage);
        }

        [Fact]
        public async Task GetSettingAsync_ValidSlugAndKey_ReturnsSettingDto()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "seo.keywords",
                ValueEn = "ai, tech, cloud",
                ValueAr = "ذكاء، تقنية، سحاب",
                SettingType = "text",
                GroupName = "SEO",
                Label = "Keywords"
            };
            await _service.CreateAsync(dto);

            var setting = await _service.GetSettingAsync("company-a", "seo.keywords");

            Assert.NotNull(setting);
            Assert.Equal("seo.keywords", setting.Key);
            Assert.Equal("ai, tech, cloud", setting.ValueEn);
            Assert.Equal("ذكاء، تقنية، سحاب", setting.ValueAr);
        }

        [Fact]
        public async Task GetSettingValueEnAsync_ReturnsValueWhenFound_OrFallbackDefault()
        {
            var dto = new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _companyAId,
                Key = "contact.hotline",
                ValueEn = "+1-800-NEURIX",
                SettingType = "text",
                GroupName = "Contact",
                Label = "Hotline"
            };
            await _service.CreateAsync(dto);

            var foundValue = await _service.GetSettingValueEnAsync("company-a", "contact.hotline", "default-hotline");
            var missingValue = await _service.GetSettingValueEnAsync("company-a", "nonexistent.key", "default-fallback");

            Assert.Equal("+1-800-NEURIX", foundValue);
            Assert.Equal("default-fallback", missingValue);
        }
    }
}
