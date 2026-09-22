using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    public class CmsMenuItemServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsMenuItemService _service;
        private readonly Guid _profileId;

        public CmsMenuItemServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: $"CmsMenuItemsTestDb_{Guid.NewGuid()}")
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsMenuItemService(_db, NullLogger<CmsMenuItemService>.Instance, new MemoryCache(new MemoryCacheOptions()));

            _profileId = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = _profileId,
                Slug = "neurix",
                NameEn = "Neurix AI",
                NameAr = "نيوركس AI",
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow
            });
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task FullWorkflow_CreateReadUpdateReorderDelete()
        {
            // 1. Create items
            var item1Dto = new CmsMenuItemUpsertDto
            {
                CompanyProfileId = _profileId,
                LabelEn = "Research Labs",
                LabelAr = "مختبرات الأبحاث",
                Url = "/Home/Labs",
                IconName = "flask-conical",
                Placement = CmsMenuItemPlacement.Header,
                DisplayOrder = 1,
                IsPublished = true
            };
            var createRes1 = await _service.CreateMenuItemAsync(item1Dto);
            Assert.True(createRes1.Success);
            var item1Id = createRes1.Value;

            var item2Dto = new CmsMenuItemUpsertDto
            {
                CompanyProfileId = _profileId,
                LabelEn = "Global Ethics",
                LabelAr = "الأخلاقيات العالمية",
                Url = "/Home/About#ethics",
                IconName = "shield",
                Placement = CmsMenuItemPlacement.Footer,
                DisplayOrder = 2,
                IsPublished = true
            };
            var createRes2 = await _service.CreateMenuItemAsync(item2Dto);
            Assert.True(createRes2.Success);
            var item2Id = createRes2.Value;

            // 2. Read by Placement
            var headerItems = await _service.GetMenuItemsByCompanySlugAsync("neurix", CmsMenuItemPlacement.Header);
            Assert.Single(headerItems);
            Assert.Equal("Research Labs", headerItems[0].LabelEn);

            var footerItems = await _service.GetMenuItemsByCompanySlugAsync("neurix", CmsMenuItemPlacement.Footer);
            Assert.Single(footerItems);
            Assert.Equal("Global Ethics", footerItems[0].LabelEn);

            // 3. Update
            var updateDto = new CmsMenuItemUpsertDto
            {
                Id = item1Id,
                CompanyProfileId = _profileId,
                LabelEn = "Frontier Research Labs",
                LabelAr = "مختبرات الأبحاث المتقدمة",
                Url = "/Home/Labs",
                IconName = "sparkles",
                Placement = CmsMenuItemPlacement.Both,
                DisplayOrder = 5,
                IsPublished = true
            };
            var updateRes = await _service.UpdateMenuItemAsync(updateDto);
            Assert.True(updateRes.Success);

            var updatedItem = await _service.GetMenuItemByIdAsync(item1Id);
            Assert.NotNull(updatedItem);
            Assert.Equal("Frontier Research Labs", updatedItem.LabelEn);
            Assert.Equal("مختبرات الأبحاث المتقدمة", updatedItem.LabelAr);
            Assert.Equal(CmsMenuItemPlacement.Both, updatedItem.Placement);

            // 4. Reorder
            var reorderRes = await _service.ReorderMenuItemsAsync(new List<Guid> { item2Id, item1Id });
            Assert.True(reorderRes.Success);

            var allItems = await _service.GetMenuItemsByCompanyIdAsync(_profileId);
            Assert.Equal(2, allItems.Count);
            Assert.Equal(item2Id, allItems[0].Id);
            Assert.Equal(1, allItems[0].DisplayOrder);
            Assert.Equal(item1Id, allItems[1].Id);
            Assert.Equal(2, allItems[1].DisplayOrder);

            // 5. Delete
            var deleteRes = await _service.DeleteMenuItemAsync(item2Id);
            Assert.True(deleteRes.Success);

            var remaining = await _service.GetMenuItemsByCompanySlugAsync("neurix");
            Assert.Single(remaining);
            Assert.Equal(item1Id, remaining[0].Id);
        }
    }
}
