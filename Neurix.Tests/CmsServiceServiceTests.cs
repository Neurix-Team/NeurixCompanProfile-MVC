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
    public class CmsServiceServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsServiceService _service;
        private readonly Guid _companyAId;
        private readonly Guid _companyBId;

        public CmsServiceServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsServiceService(_db);

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
        public async Task CreateAsync_ValidService_ReturnsSuccessAndPersists()
        {
            var dto = new CmsServiceUpsertDto
            {
                CompanyProfileId = _companyAId,
                Slug = "ai-engineering",
                NameEn = "AI Engineering",
                NameAr = "هندسة الذكاء الاصطناعي",
                IconName = "cpu",
                DisplayOrder = 1,
                IsPublished = true
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var persisted = await _db.Services.FindAsync(result.Value);
            Assert.NotNull(persisted);
            Assert.Equal("ai-engineering", persisted.Slug);
            Assert.Equal("AI Engineering", persisted.NameEn);
            Assert.Equal(_companyAId, persisted.CompanyProfileId);
            Assert.Equal("cpu", persisted.IconName);
        }

        [Fact]
        public async Task CreateAsync_DuplicateSlugInSameCompany_ReturnsFailure()
        {
            var dto1 = new CmsServiceUpsertDto
            {
                CompanyProfileId = _companyAId,
                Slug = "cloud-solutions",
                NameEn = "Cloud Solutions 1",
                NameAr = "حلول سحابية 1"
            };

            var dto2 = new CmsServiceUpsertDto
            {
                CompanyProfileId = _companyAId,
                Slug = "cloud-solutions",
                NameEn = "Cloud Solutions 2",
                NameAr = "حلول سحابية 2"
            };

            var result1 = await _service.CreateAsync(dto1);
            var result2 = await _service.CreateAsync(dto2);

            Assert.True(result1.Success);
            Assert.False(result2.Success);
            Assert.Contains("already exists", result2.ErrorMessage);
        }

        [Fact]
        public async Task CreateAsync_SameSlugInDifferentCompanies_SucceedsDueToBrandIsolation()
        {
            var dtoCompanyA = new CmsServiceUpsertDto
            {
                CompanyProfileId = _companyAId,
                Slug = "consulting",
                NameEn = "Consulting for A",
                NameAr = "استشارات أ"
            };

            var dtoCompanyB = new CmsServiceUpsertDto
            {
                CompanyProfileId = _companyBId,
                Slug = "consulting",
                NameEn = "Consulting for B",
                NameAr = "استشارات ب"
            };

            var resultA = await _service.CreateAsync(dtoCompanyA);
            var resultB = await _service.CreateAsync(dtoCompanyB);

            Assert.True(resultA.Success);
            Assert.True(resultB.Success);
        }

        [Fact]
        public async Task GetServicesByCompanyAsync_FiltersByCompanyIdCorrectly()
        {
            _db.Services.AddRange(
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "Service A1", NameAr = "خدمة أ1", Slug = "s-a1", IsPublished = true },
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "Service A2", NameAr = "خدمة أ2", Slug = "s-a2", IsPublished = true },
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyBId, NameEn = "Service B1", NameAr = "خدمة ب1", Slug = "s-b1", IsPublished = true }
            );
            await _db.SaveChangesAsync();

            var companyAServices = await _service.GetServicesByCompanyAsync(_companyAId);
            var companyBServices = await _service.GetServicesByCompanyAsync(_companyBId);

            Assert.Equal(2, companyAServices.Count);
            Assert.All(companyAServices, s => Assert.Equal(_companyAId, s.CompanyProfileId));

            Assert.Single(companyBServices);
            Assert.Equal("s-b1", companyBServices[0].Slug);
        }

        [Fact]
        public async Task GetPublishedServicesByCompanySlugAsync_ReturnsOnlyPublishedAndOrdered()
        {
            _db.Services.AddRange(
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "Second", NameAr = "ثانياً", Slug = "second", DisplayOrder = 2, IsPublished = true },
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "First", NameAr = "أولاً", Slug = "first", DisplayOrder = 1, IsPublished = true },
                new CmsService { Id = Guid.NewGuid(), CompanyProfileId = _companyAId, NameEn = "Draft", NameAr = "مسودة", Slug = "draft", DisplayOrder = 0, IsPublished = false }
            );
            await _db.SaveChangesAsync();

            var published = await _service.GetPublishedServicesByCompanySlugAsync("company-a");

            Assert.Equal(2, published.Count);
            Assert.Equal("first", published[0].Slug);
            Assert.Equal("second", published[1].Slug);
        }

        [Fact]
        public async Task SoftDeleteAsync_ExcludesFromSubsequentQueries()
        {
            var serviceId = Guid.NewGuid();
            _db.Services.Add(new CmsService
            {
                Id = serviceId,
                CompanyProfileId = _companyAId,
                NameEn = "To Delete",
                NameAr = "للحذف",
                Slug = "to-delete",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var deleteResult = await _service.SoftDeleteAsync(serviceId);
            Assert.True(deleteResult.Success);

            var fetched = await _service.GetByIdAsync(serviceId);
            Assert.Null(fetched);

            var list = await _service.GetServicesByCompanyAsync(_companyAId, includeUnpublished: true);
            Assert.DoesNotContain(list, s => s.Id == serviceId);
        }
    }
}
