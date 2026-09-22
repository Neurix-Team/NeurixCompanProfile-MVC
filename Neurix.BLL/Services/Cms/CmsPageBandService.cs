using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsPageBandService : ICmsPageBandService
    {
        private readonly CmsDbContext _db;
        private readonly ILogger<CmsPageBandService> _logger;

        public CmsPageBandService(CmsDbContext db, ILogger<CmsPageBandService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ════════════════════════════════════════════════════════════════
        // BANDS
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsPageContentDto> GetPageContentByCompanySlugAsync(string slug, string pageKey, bool includeUnpublished = false)
        {
            var key = CmsPageKeys.Normalise(pageKey);

            var bandQuery = _db.PageBands
                .AsNoTracking()
                .Include(b => b.CompanyProfile)
                .Where(b => b.CompanyProfile != null && b.CompanyProfile.Slug == slug && b.PageKey == key);

            var itemQuery = _db.PageBandItems
                .AsNoTracking()
                .Include(i => i.CompanyProfile)
                .Where(i => i.CompanyProfile != null && i.CompanyProfile.Slug == slug && i.PageKey == key);

            if (!includeUnpublished)
            {
                bandQuery = bandQuery.Where(b => b.IsPublished);
                itemQuery = itemQuery.Where(i => i.IsPublished);
            }

            var bands = await bandQuery.OrderBy(b => b.DisplayOrder).ToListAsync();
            var items = await itemQuery.OrderBy(i => i.DisplayOrder).ToListAsync();

            return new CmsPageContentDto
            {
                PageKey = key,
                Bands = bands.Select(MapToBandDto).ToList(),
                Items = items.Select(MapToItemDto).ToList()
            };
        }

        public async Task<CmsPageBandDto?> GetBandByCompanyIdAsync(Guid companyProfileId, string pageKey, string bandKey)
        {
            var page = CmsPageKeys.Normalise(pageKey);
            var band = CmsPageKeys.Normalise(bandKey);

            var entity = await _db.PageBands
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.CompanyProfileId == companyProfileId && b.PageKey == page && b.BandKey == band);

            return entity == null ? null : MapToBandDto(entity);
        }

        public async Task<IReadOnlyList<CmsPageBandDto>> GetBandsByCompanyIdAsync(Guid companyProfileId, string pageKey)
        {
            var page = CmsPageKeys.Normalise(pageKey);

            var entities = await _db.PageBands
                .AsNoTracking()
                .Where(b => b.CompanyProfileId == companyProfileId && b.PageKey == page)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            return entities.Select(MapToBandDto).ToList();
        }

        public async Task<IReadOnlyList<CmsPageBandDto>> GetAllBandsByCompanyIdAsync(Guid companyProfileId)
        {
            var entities = await _db.PageBands
                .AsNoTracking()
                .Where(b => b.CompanyProfileId == companyProfileId)
                .OrderBy(b => b.PageKey).ThenBy(b => b.DisplayOrder)
                .ToListAsync();

            return entities.Select(MapToBandDto).ToList();
        }

        public async Task<ServiceResult> UpsertBandAsync(CmsPageBandUpsertDto dto, Guid? userId = null)
        {
            var page = CmsPageKeys.Normalise(dto.PageKey);
            var band = CmsPageKeys.Normalise(dto.BandKey);

            if (!CmsPageKeys.IsKnown(page, band))
            {
                return ServiceResult.Fail($"{dto.BandKey} is not a section on the {dto.PageKey} page.");
            }

            try
            {
                var entity = await _db.PageBands
                    .FirstOrDefaultAsync(b => b.CompanyProfileId == dto.CompanyProfileId && b.PageKey == page && b.BandKey == band);

                if (entity == null)
                {
                    entity = new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        PageKey = page,
                        BandKey = band,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.PageBands.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = Clean(dto.BadgeEn);
                entity.BadgeAr = Clean(dto.BadgeAr);
                entity.TitlePrefixEn = dto.TitlePrefixEn;
                entity.TitlePrefixAr = dto.TitlePrefixAr;
                entity.TitleHighlightEn = dto.TitleHighlightEn;
                entity.TitleHighlightAr = dto.TitleHighlightAr;
                entity.TitleSuffixEn = dto.TitleSuffixEn;
                entity.TitleSuffixAr = dto.TitleSuffixAr;
                entity.BodyEn = Clean(dto.BodyEn);
                entity.BodyAr = Clean(dto.BodyAr);
                entity.ImagePath = Clean(dto.ImagePath);
                entity.ButtonTextEn = Clean(dto.ButtonTextEn);
                entity.ButtonTextAr = Clean(dto.ButtonTextAr);
                entity.ButtonUrl = Clean(dto.ButtonUrl);
                entity.ItemLinkTextEn = Clean(dto.ItemLinkTextEn);
                entity.ItemLinkTextAr = Clean(dto.ItemLinkTextAr);
                entity.EmptyStateEn = Clean(dto.EmptyStateEn);
                entity.EmptyStateAr = Clean(dto.EmptyStateAr);
                entity.DisplayOrder = dto.DisplayOrder;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert the {BandKey} band on the {PageKey} page.", band, page);
                return ServiceResult.Fail("Could not save this section. Please try again.");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // BAND ITEMS
        // ════════════════════════════════════════════════════════════════

        public async Task<IReadOnlyList<CmsPageBandItemDto>> GetBandItemsByCompanyIdAsync(Guid companyProfileId, string pageKey, string bandKey)
        {
            var page = CmsPageKeys.Normalise(pageKey);
            var band = CmsPageKeys.Normalise(bandKey);

            var entities = await _db.PageBandItems
                .AsNoTracking()
                .Where(i => i.CompanyProfileId == companyProfileId && i.PageKey == page && i.BandKey == band)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return entities.Select(MapToItemDto).ToList();
        }

        public async Task<IReadOnlyList<CmsPageBandItemDto>> GetAllBandItemsByCompanyIdAsync(Guid companyProfileId)
        {
            var entities = await _db.PageBandItems
                .AsNoTracking()
                .Where(i => i.CompanyProfileId == companyProfileId)
                .OrderBy(i => i.PageKey).ThenBy(i => i.BandKey).ThenBy(i => i.DisplayOrder)
                .ToListAsync();

            return entities.Select(MapToItemDto).ToList();
        }

        public async Task<CmsPageBandItemDto?> GetBandItemByIdAsync(Guid id)
        {
            var entity = await _db.PageBandItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            return entity == null ? null : MapToItemDto(entity);
        }

        public async Task<ServiceResult<Guid>> CreateBandItemAsync(CmsPageBandItemUpsertDto dto, Guid? userId = null)
        {
            var page = CmsPageKeys.Normalise(dto.PageKey);
            var band = CmsPageKeys.Normalise(dto.BandKey);

            if (!CmsPageKeys.IsKnown(page, band))
            {
                return ServiceResult<Guid>.Fail($"{dto.BandKey} is not a section on the {dto.PageKey} page.");
            }

            try
            {
                var entity = new CmsPageBandItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = dto.CompanyProfileId,
                    PageKey = page,
                    BandKey = band,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                ApplyItem(entity, dto);

                _db.PageBandItems.Add(entity);
                await _db.SaveChangesAsync();

                return ServiceResult<Guid>.Ok(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create an item in the {BandKey} band on the {PageKey} page.", band, page);
                return ServiceResult<Guid>.Fail("Could not save this item. Please try again.");
            }
        }

        public async Task<ServiceResult> UpdateBandItemAsync(Guid id, CmsPageBandItemUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = await _db.PageBandItems.FirstOrDefaultAsync(i => i.Id == id);
                if (entity == null)
                {
                    return ServiceResult.Fail("This item no longer exists.");
                }

                ApplyItem(entity, dto);
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update page band item {ItemId}.", id);
                return ServiceResult.Fail("Could not save this item. Please try again.");
            }
        }

        public async Task<ServiceResult> DeleteBandItemAsync(Guid id)
        {
            try
            {
                var entity = await _db.PageBandItems.FirstOrDefaultAsync(i => i.Id == id);
                if (entity == null)
                {
                    return ServiceResult.Fail("This item no longer exists.");
                }

                entity.IsDeleted = true;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete page band item {ItemId}.", id);
                return ServiceResult.Fail("Could not delete this item. Please try again.");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Trims and turns an all-whitespace value into null, so a field the editor cleared
        /// reads as "not set" and the public page falls through to its own default copy.
        /// </summary>
        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void ApplyItem(CmsPageBandItem entity, CmsPageBandItemUpsertDto dto)
        {
            entity.IconName = Clean(dto.IconName);
            entity.TitleEn = dto.TitleEn?.Trim() ?? string.Empty;
            entity.TitleAr = dto.TitleAr?.Trim() ?? string.Empty;
            entity.DescriptionEn = Clean(dto.DescriptionEn);
            entity.DescriptionAr = Clean(dto.DescriptionAr);
            entity.ImagePath = Clean(dto.ImagePath);
            entity.LinkUrl = Clean(dto.LinkUrl);
            entity.LinkTextEn = Clean(dto.LinkTextEn);
            entity.LinkTextAr = Clean(dto.LinkTextAr);
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsPublished = dto.IsPublished;
        }

        private static CmsPageBandDto MapToBandDto(CmsPageBand e) => new()
        {
            Id = e.Id,
            CompanyProfileId = e.CompanyProfileId,
            PageKey = e.PageKey,
            BandKey = e.BandKey,
            BadgeEn = e.BadgeEn,
            BadgeAr = e.BadgeAr,
            TitlePrefixEn = e.TitlePrefixEn,
            TitlePrefixAr = e.TitlePrefixAr,
            TitleHighlightEn = e.TitleHighlightEn,
            TitleHighlightAr = e.TitleHighlightAr,
            TitleSuffixEn = e.TitleSuffixEn,
            TitleSuffixAr = e.TitleSuffixAr,
            BodyEn = e.BodyEn,
            BodyAr = e.BodyAr,
            ImagePath = e.ImagePath,
            ButtonTextEn = e.ButtonTextEn,
            ButtonTextAr = e.ButtonTextAr,
            ButtonUrl = e.ButtonUrl,
            ItemLinkTextEn = e.ItemLinkTextEn,
            ItemLinkTextAr = e.ItemLinkTextAr,
            EmptyStateEn = e.EmptyStateEn,
            EmptyStateAr = e.EmptyStateAr,
            DisplayOrder = e.DisplayOrder,
            IsPublished = e.IsPublished
        };

        private static CmsPageBandItemDto MapToItemDto(CmsPageBandItem e) => new()
        {
            Id = e.Id,
            CompanyProfileId = e.CompanyProfileId,
            PageKey = e.PageKey,
            BandKey = e.BandKey,
            IconName = e.IconName,
            TitleEn = e.TitleEn,
            TitleAr = e.TitleAr,
            DescriptionEn = e.DescriptionEn,
            DescriptionAr = e.DescriptionAr,
            ImagePath = e.ImagePath,
            LinkUrl = e.LinkUrl,
            LinkTextEn = e.LinkTextEn,
            LinkTextAr = e.LinkTextAr,
            DisplayOrder = e.DisplayOrder,
            IsPublished = e.IsPublished
        };
    }
}
