using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsMenuItemService : ICmsMenuItemService
    {
        // The navbar and footer each ask for a different placement of the same
        // company's menu on every single page, so this is 2-3 identical-shaped
        // queries per request without a cache. Menu links change rarely, so a short
        // TTL (no explicit invalidation) is a reasonable trade for staying simple.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        private readonly CmsDbContext _db;
        private readonly ILogger<CmsMenuItemService> _logger;
        private readonly IMemoryCache _cache;

        public CmsMenuItemService(CmsDbContext db, ILogger<CmsMenuItemService> logger, IMemoryCache cache)
        {
            _db = db;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IReadOnlyList<CmsMenuItemSummaryDto>> GetMenuItemsByCompanySlugAsync(
            string slug,
            CmsMenuItemPlacement? placement = null,
            bool includeUnpublished = false)
        {
            var cacheKey = $"cms:menu-items:{slug}:{placement}:{includeUnpublished}";
            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CmsMenuItemSummaryDto>? cached) && cached is not null)
            {
                return cached;
            }

            var query = _db.MenuItems
                .AsNoTracking()
                .Include(m => m.CompanyProfile)
                .Where(m => m.CompanyProfile != null && m.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(m => m.IsPublished);
            }

            if (placement.HasValue)
            {
                var p = placement.Value;
                if (p == CmsMenuItemPlacement.Header)
                {
                    query = query.Where(m => m.Placement == CmsMenuItemPlacement.Header || m.Placement == CmsMenuItemPlacement.Both);
                }
                else if (p == CmsMenuItemPlacement.Footer)
                {
                    query = query.Where(m => m.Placement == CmsMenuItemPlacement.Footer || m.Placement == CmsMenuItemPlacement.Both);
                }
                else
                {
                    query = query.Where(m => m.Placement == p);
                }
            }

            var items = await query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.CreatedAtUtc).ToListAsync();
            var result = items.Select(MapToSummaryDto).ToList();
            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }

        public async Task<IReadOnlyList<CmsMenuItemSummaryDto>> GetMenuItemsByCompanyIdAsync(
            Guid companyProfileId,
            CmsMenuItemPlacement? placement = null,
            bool includeUnpublished = true)
        {
            var query = _db.MenuItems
                .AsNoTracking()
                .Include(m => m.CompanyProfile)
                .Where(m => m.CompanyProfileId == companyProfileId);

            if (!includeUnpublished)
            {
                query = query.Where(m => m.IsPublished);
            }

            if (placement.HasValue)
            {
                var p = placement.Value;
                if (p == CmsMenuItemPlacement.Header)
                {
                    query = query.Where(m => m.Placement == CmsMenuItemPlacement.Header || m.Placement == CmsMenuItemPlacement.Both);
                }
                else if (p == CmsMenuItemPlacement.Footer)
                {
                    query = query.Where(m => m.Placement == CmsMenuItemPlacement.Footer || m.Placement == CmsMenuItemPlacement.Both);
                }
                else
                {
                    query = query.Where(m => m.Placement == p);
                }
            }

            var items = await query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.CreatedAtUtc).ToListAsync();
            return items.Select(MapToSummaryDto).ToList();
        }

        public async Task<CmsMenuItemDetailDto?> GetMenuItemByIdAsync(Guid id)
        {
            var entity = await _db.MenuItems
                .AsNoTracking()
                .Include(m => m.CompanyProfile)
                .FirstOrDefaultAsync(m => m.Id == id);

            return entity == null ? null : MapToDetailDto(entity);
        }

        public async Task<ServiceResult<Guid>> CreateMenuItemAsync(CmsMenuItemUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
                if (!companyExists)
                {
                    return ServiceResult<Guid>.Fail("The specified company profile does not exist.");
                }

                var entity = new CmsMenuItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = dto.CompanyProfileId,
                    LabelEn = dto.LabelEn.Trim(),
                    LabelAr = dto.LabelAr.Trim(),
                    Url = dto.Url.Trim(),
                    IconName = string.IsNullOrWhiteSpace(dto.IconName) ? null : dto.IconName.Trim(),
                    Placement = dto.Placement,
                    DisplayOrder = dto.DisplayOrder,
                    OpenInNewTab = dto.OpenInNewTab,
                    IsPublished = dto.IsPublished,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _db.MenuItems.Add(entity);
                await _db.SaveChangesAsync();

                return ServiceResult<Guid>.Ok(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create menu item for company profile {CompanyId}", dto.CompanyProfileId);
                return ServiceResult<Guid>.Fail($"An error occurred while creating the menu item: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateMenuItemAsync(CmsMenuItemUpsertDto dto, Guid? userId = null)
        {
            try
            {
                if (!dto.Id.HasValue)
                {
                    return ServiceResult.Fail("Menu item ID is required for updates.");
                }

                var entity = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == dto.Id.Value);
                if (entity == null)
                {
                    return ServiceResult.Fail("The menu item was not found.");
                }

                var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
                if (!companyExists)
                {
                    return ServiceResult.Fail("The specified company profile does not exist.");
                }

                entity.CompanyProfileId = dto.CompanyProfileId;
                entity.LabelEn = dto.LabelEn.Trim();
                entity.LabelAr = dto.LabelAr.Trim();
                entity.Url = dto.Url.Trim();
                entity.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? null : dto.IconName.Trim();
                entity.Placement = dto.Placement;
                entity.DisplayOrder = dto.DisplayOrder;
                entity.OpenInNewTab = dto.OpenInNewTab;
                entity.IsPublished = dto.IsPublished;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update menu item {Id}", dto.Id);
                return ServiceResult.Fail($"An error occurred while updating the menu item: {ex.Message}");
            }
        }

        public async Task<ServiceResult> DeleteMenuItemAsync(Guid id, Guid? userId = null)
        {
            try
            {
                var entity = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
                if (entity == null)
                {
                    return ServiceResult.Fail("The menu item was not found.");
                }

                entity.IsDeleted = true;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete menu item {Id}", id);
                return ServiceResult.Fail($"An error occurred while deleting the menu item: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ReorderMenuItemsAsync(IReadOnlyList<Guid> orderedIds, Guid? userId = null)
        {
            try
            {
                if (orderedIds == null || orderedIds.Count == 0)
                {
                    return ServiceResult.Ok();
                }

                var items = await _db.MenuItems.Where(m => orderedIds.Contains(m.Id)).ToListAsync();
                var index = 1;

                foreach (var id in orderedIds)
                {
                    var item = items.FirstOrDefault(m => m.Id == id);
                    if (item != null)
                    {
                        item.DisplayOrder = index++;
                        item.UpdatedAtUtc = DateTime.UtcNow;
                    }
                }

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reorder menu items.");
                return ServiceResult.Fail($"An error occurred while reordering menu items: {ex.Message}");
            }
        }

        private static CmsMenuItemSummaryDto MapToSummaryDto(CmsMenuItem entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            CompanyNameEn = entity.CompanyProfile?.NameEn ?? string.Empty,
            CompanySlug = entity.CompanyProfile?.Slug ?? string.Empty,
            LabelEn = entity.LabelEn,
            LabelAr = entity.LabelAr,
            Url = entity.Url,
            IconName = entity.IconName,
            Placement = entity.Placement,
            DisplayOrder = entity.DisplayOrder,
            OpenInNewTab = entity.OpenInNewTab,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsMenuItemDetailDto MapToDetailDto(CmsMenuItem entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            CompanyNameEn = entity.CompanyProfile?.NameEn ?? string.Empty,
            CompanySlug = entity.CompanyProfile?.Slug ?? string.Empty,
            LabelEn = entity.LabelEn,
            LabelAr = entity.LabelAr,
            Url = entity.Url,
            IconName = entity.IconName,
            Placement = entity.Placement,
            DisplayOrder = entity.DisplayOrder,
            OpenInNewTab = entity.OpenInNewTab,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
