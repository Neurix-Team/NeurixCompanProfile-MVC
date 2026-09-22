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
    public class CmsHomeSectionService : ICmsHomeSectionService
    {
        private readonly CmsDbContext _db;
        private readonly ILogger<CmsHomeSectionService> _logger;

        public CmsHomeSectionService(CmsDbContext db, ILogger<CmsHomeSectionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ════════════════════════════════════════════════════════════════
        // 1. HERO SECTION
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsHeroSectionDto?> GetHeroSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.HeroSections
                .AsNoTracking()
                .Include(h => h.CompanyProfile)
                .Where(h => h.CompanyProfile != null && h.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(h => h.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToHeroDto(entity);
        }

        public async Task<CmsHeroSectionDto?> GetHeroSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.HeroSections
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToHeroDto(entity);
        }

        public async Task<CmsHeroSectionDto?> GetHeroSectionByIdAsync(Guid id)
        {
            var entity = await _db.HeroSections
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);

            return entity == null ? null : MapToHeroDto(entity);
        }

        public async Task<ServiceResult> UpsertHeroSectionAsync(CmsHeroSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
                if (!companyExists)
                {
                    return ServiceResult.Fail("Target company profile was not found.");
                }

                var entity = await _db.HeroSections
                    .FirstOrDefaultAsync(h => h.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsHeroSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.HeroSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitlePrefixEn = dto.TitlePrefixEn ?? string.Empty;
                entity.TitleHighlightEn = dto.TitleHighlightEn?.Trim() ?? string.Empty;
                entity.TitleSuffixEn = dto.TitleSuffixEn ?? string.Empty;
                entity.TitlePrefixAr = dto.TitlePrefixAr ?? string.Empty;
                entity.TitleHighlightAr = dto.TitleHighlightAr?.Trim() ?? string.Empty;
                entity.TitleSuffixAr = dto.TitleSuffixAr ?? string.Empty;
                entity.SubtitleEn = dto.SubtitleEn?.Trim() ?? string.Empty;
                entity.SubtitleAr = dto.SubtitleAr?.Trim() ?? string.Empty;
                entity.PrimaryButtonTextEn = dto.PrimaryButtonTextEn?.Trim() ?? string.Empty;
                entity.PrimaryButtonTextAr = dto.PrimaryButtonTextAr?.Trim() ?? string.Empty;
                entity.PrimaryButtonUrl = dto.PrimaryButtonUrl?.Trim() ?? string.Empty;
                entity.SecondaryButtonTextEn = dto.SecondaryButtonTextEn?.Trim() ?? string.Empty;
                entity.SecondaryButtonTextAr = dto.SecondaryButtonTextAr?.Trim() ?? string.Empty;
                entity.SecondaryButtonUrl = dto.SecondaryButtonUrl?.Trim() ?? string.Empty;
                entity.Stat1Value = dto.Stat1Value?.Trim() ?? string.Empty;
                entity.Stat1LabelEn = dto.Stat1LabelEn?.Trim() ?? string.Empty;
                entity.Stat1LabelAr = dto.Stat1LabelAr?.Trim() ?? string.Empty;
                entity.Stat2Value = dto.Stat2Value?.Trim() ?? string.Empty;
                entity.Stat2LabelEn = dto.Stat2LabelEn?.Trim() ?? string.Empty;
                entity.Stat2LabelAr = dto.Stat2LabelAr?.Trim() ?? string.Empty;
                entity.Stat3Value = dto.Stat3Value?.Trim() ?? string.Empty;
                entity.Stat3LabelEn = dto.Stat3LabelEn?.Trim() ?? string.Empty;
                entity.Stat3LabelAr = dto.Stat3LabelAr?.Trim() ?? string.Empty;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Hero section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the Hero section: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 2. HUMAN VISION SECTION
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsHumanVisionSectionDto?> GetHumanVisionSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.HumanVisionSections
                .AsNoTracking()
                .Include(v => v.CompanyProfile)
                .Where(v => v.CompanyProfile != null && v.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(v => v.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToHumanVisionDto(entity);
        }

        public async Task<CmsHumanVisionSectionDto?> GetHumanVisionSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.HumanVisionSections
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToHumanVisionDto(entity);
        }

        public async Task<CmsHumanVisionSectionDto?> GetHumanVisionSectionByIdAsync(Guid id)
        {
            var entity = await _db.HumanVisionSections
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            return entity == null ? null : MapToHumanVisionDto(entity);
        }

        public async Task<ServiceResult> UpsertHumanVisionSectionAsync(CmsHumanVisionSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
                if (!companyExists)
                {
                    return ServiceResult.Fail("Target company profile was not found.");
                }

                var entity = await _db.HumanVisionSections
                    .FirstOrDefaultAsync(v => v.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsHumanVisionSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.HumanVisionSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitlePrefixEn = dto.TitlePrefixEn ?? string.Empty;
                entity.TitleHighlightEn = dto.TitleHighlightEn?.Trim() ?? string.Empty;
                entity.TitlePrefixAr = dto.TitlePrefixAr ?? string.Empty;
                entity.TitleHighlightAr = dto.TitleHighlightAr?.Trim() ?? string.Empty;
                entity.Paragraph1En = dto.Paragraph1En?.Trim() ?? string.Empty;
                entity.Paragraph1Ar = dto.Paragraph1Ar?.Trim() ?? string.Empty;
                entity.Paragraph2En = dto.Paragraph2En?.Trim() ?? string.Empty;
                entity.Paragraph2Ar = dto.Paragraph2Ar?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(dto.ImagePath))
                {
                    entity.ImagePath = dto.ImagePath.Trim();
                }
                entity.ImageAltEn = dto.ImageAltEn?.Trim() ?? string.Empty;
                entity.ImageAltAr = dto.ImageAltAr?.Trim() ?? string.Empty;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Human Vision section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the Human Vision section: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 3. MESSAGE TO PIONEERS SECTION
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsPioneersSectionDto?> GetPioneersSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.PioneersSections
                .AsNoTracking()
                .Include(p => p.CompanyProfile)
                .Where(p => p.CompanyProfile != null && p.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToPioneersDto(entity);
        }

        public async Task<CmsPioneersSectionDto?> GetPioneersSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.PioneersSections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToPioneersDto(entity);
        }

        public async Task<CmsPioneersSectionDto?> GetPioneersSectionByIdAsync(Guid id)
        {
            var entity = await _db.PioneersSections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity == null ? null : MapToPioneersDto(entity);
        }

        public async Task<ServiceResult> UpsertPioneersSectionAsync(CmsPioneersSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
                if (!companyExists)
                {
                    return ServiceResult.Fail("Target company profile was not found.");
                }

                var entity = await _db.PioneersSections
                    .FirstOrDefaultAsync(p => p.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsPioneersSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.PioneersSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitlePrefixEn = dto.TitlePrefixEn ?? string.Empty;
                entity.TitleHighlightEn = dto.TitleHighlightEn?.Trim() ?? string.Empty;
                entity.TitlePrefixAr = dto.TitlePrefixAr ?? string.Empty;
                entity.TitleHighlightAr = dto.TitleHighlightAr?.Trim() ?? string.Empty;
                entity.Paragraph1En = dto.Paragraph1En?.Trim() ?? string.Empty;
                entity.Paragraph1Ar = dto.Paragraph1Ar?.Trim() ?? string.Empty;
                entity.Paragraph2En = dto.Paragraph2En?.Trim() ?? string.Empty;
                entity.Paragraph2Ar = dto.Paragraph2Ar?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(dto.ImagePath))
                {
                    entity.ImagePath = dto.ImagePath.Trim();
                }
                entity.ImageAltEn = dto.ImageAltEn?.Trim() ?? string.Empty;
                entity.ImageAltAr = dto.ImageAltAr?.Trim() ?? string.Empty;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Pioneers section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the Pioneers section: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // MAPPERS
        // ════════════════════════════════════════════════════════════════

        private static CmsHeroSectionDto MapToHeroDto(CmsHeroSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitleSuffixEn = entity.TitleSuffixEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            TitleSuffixAr = entity.TitleSuffixAr,
            SubtitleEn = entity.SubtitleEn,
            SubtitleAr = entity.SubtitleAr,
            PrimaryButtonTextEn = entity.PrimaryButtonTextEn,
            PrimaryButtonTextAr = entity.PrimaryButtonTextAr,
            PrimaryButtonUrl = entity.PrimaryButtonUrl,
            SecondaryButtonTextEn = entity.SecondaryButtonTextEn,
            SecondaryButtonTextAr = entity.SecondaryButtonTextAr,
            SecondaryButtonUrl = entity.SecondaryButtonUrl,
            Stat1Value = entity.Stat1Value,
            Stat1LabelEn = entity.Stat1LabelEn,
            Stat1LabelAr = entity.Stat1LabelAr,
            Stat2Value = entity.Stat2Value,
            Stat2LabelEn = entity.Stat2LabelEn,
            Stat2LabelAr = entity.Stat2LabelAr,
            Stat3Value = entity.Stat3Value,
            Stat3LabelEn = entity.Stat3LabelEn,
            Stat3LabelAr = entity.Stat3LabelAr,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsHumanVisionSectionDto MapToHumanVisionDto(CmsHumanVisionSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            Paragraph1En = entity.Paragraph1En,
            Paragraph1Ar = entity.Paragraph1Ar,
            Paragraph2En = entity.Paragraph2En,
            Paragraph2Ar = entity.Paragraph2Ar,
            ImagePath = entity.ImagePath,
            ImageAltEn = entity.ImageAltEn,
            ImageAltAr = entity.ImageAltAr,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsPioneersSectionDto MapToPioneersDto(CmsPioneersSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            Paragraph1En = entity.Paragraph1En,
            Paragraph1Ar = entity.Paragraph1Ar,
            Paragraph2En = entity.Paragraph2En,
            Paragraph2Ar = entity.Paragraph2Ar,
            ImagePath = entity.ImagePath,
            ImageAltEn = entity.ImageAltEn,
            ImageAltAr = entity.ImageAltAr,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        // ════════════════════════════════════════════════════════════════
        // 4. CORE CAPABILITIES / PILLARS SECTION (#pillars)
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsPillarsSectionDto?> GetPillarsSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.PillarsSections
                .AsNoTracking()
                .Include(p => p.CompanyProfile)
                .Where(p => p.CompanyProfile != null && p.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToPillarsSectionDto(entity);
        }

        public async Task<CmsPillarsSectionDto?> GetPillarsSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.PillarsSections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToPillarsSectionDto(entity);
        }

        public async Task<ServiceResult> UpsertPillarsSectionAsync(CmsPillarsSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = await _db.PillarsSections
                    .FirstOrDefaultAsync(p => p.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsPillarsSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.PillarsSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.TitleEn = dto.TitleEn?.Trim() ?? string.Empty;
                entity.TitleAr = dto.TitleAr?.Trim() ?? string.Empty;
                entity.SubtitleEn = dto.SubtitleEn?.Trim() ?? string.Empty;
                entity.SubtitleAr = dto.SubtitleAr?.Trim() ?? string.Empty;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Pillars section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the Pillars section: {ex.Message}");
            }
        }

        // ── Pillar Items (List) ──

        public async Task<IReadOnlyList<CmsPillarItemDto>> GetPillarItemsByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.PillarItems
                .AsNoTracking()
                .Include(p => p.CompanyProfile)
                .Where(p => p.CompanyProfile != null && p.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            var items = await query.OrderBy(p => p.DisplayOrder).ThenBy(p => p.CreatedAtUtc).ToListAsync();
            return items.Select(MapToPillarItemDto).ToList();
        }

        public async Task<IReadOnlyList<CmsPillarItemDto>> GetPillarItemsByCompanyIdAsync(Guid companyProfileId, bool includeUnpublished = true)
        {
            var query = _db.PillarItems
                .AsNoTracking()
                .Where(p => p.CompanyProfileId == companyProfileId);

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            var items = await query.OrderBy(p => p.DisplayOrder).ThenBy(p => p.CreatedAtUtc).ToListAsync();
            return items.Select(MapToPillarItemDto).ToList();
        }

        public async Task<CmsPillarItemDto?> GetPillarItemByIdAsync(Guid id)
        {
            var entity = await _db.PillarItems
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity == null ? null : MapToPillarItemDto(entity);
        }

        public async Task<ServiceResult<Guid>> CreatePillarItemAsync(CmsPillarItemUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = new CmsPillarItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = dto.CompanyProfileId,
                    IconName = dto.IconName?.Trim() ?? "brain",
                    TitleEn = dto.TitleEn?.Trim() ?? string.Empty,
                    TitleAr = dto.TitleAr?.Trim() ?? string.Empty,
                    DescriptionEn = dto.DescriptionEn?.Trim() ?? string.Empty,
                    DescriptionAr = dto.DescriptionAr?.Trim() ?? string.Empty,
                    DisplayOrder = dto.DisplayOrder,
                    IsPublished = dto.IsPublished,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _db.PillarItems.Add(entity);
                await _db.SaveChangesAsync();

                return ServiceResult<Guid>.Ok(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Pillar item for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult<Guid>.Fail($"An error occurred while creating the pillar item: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdatePillarItemAsync(CmsPillarItemUpsertDto dto, Guid? userId = null)
        {
            if (!dto.Id.HasValue)
                return ServiceResult.Fail("Pillar item ID is required for update.");

            try
            {
                var entity = await _db.PillarItems.FirstOrDefaultAsync(p => p.Id == dto.Id.Value);
                if (entity == null)
                    return ServiceResult.Fail("Pillar item not found.");

                entity.IconName = dto.IconName?.Trim() ?? "brain";
                entity.TitleEn = dto.TitleEn?.Trim() ?? string.Empty;
                entity.TitleAr = dto.TitleAr?.Trim() ?? string.Empty;
                entity.DescriptionEn = dto.DescriptionEn?.Trim() ?? string.Empty;
                entity.DescriptionAr = dto.DescriptionAr?.Trim() ?? string.Empty;
                entity.DisplayOrder = dto.DisplayOrder;
                entity.IsPublished = dto.IsPublished;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Pillar item {Id}", dto.Id);
                return ServiceResult.Fail($"An error occurred while updating the pillar item: {ex.Message}");
            }
        }

        public async Task<ServiceResult> DeletePillarItemAsync(Guid id, Guid? userId = null)
        {
            try
            {
                var entity = await _db.PillarItems.FirstOrDefaultAsync(p => p.Id == id);
                if (entity == null)
                    return ServiceResult.Fail("Pillar item not found.");

                entity.IsDeleted = true;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Pillar item {Id}", id);
                return ServiceResult.Fail($"An error occurred while deleting the pillar item: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 4b. OUR ECOSYSTEM / DIVISIONS SECTION (#divisions)
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsDivisionsSectionDto?> GetDivisionsSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.DivisionsSections
                .AsNoTracking()
                .Include(d => d.CompanyProfile)
                .Where(d => d.CompanyProfile != null && d.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(d => d.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToDivisionsSectionDto(entity);
        }

        public async Task<CmsDivisionsSectionDto?> GetDivisionsSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.DivisionsSections
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToDivisionsSectionDto(entity);
        }

        public async Task<ServiceResult> UpsertDivisionsSectionAsync(CmsDivisionsSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = await _db.DivisionsSections
                    .FirstOrDefaultAsync(d => d.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsDivisionsSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.DivisionsSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitlePrefixEn = dto.TitlePrefixEn ?? string.Empty;
                entity.TitleHighlightEn = dto.TitleHighlightEn ?? string.Empty;
                entity.TitlePrefixAr = dto.TitlePrefixAr ?? string.Empty;
                entity.TitleHighlightAr = dto.TitleHighlightAr ?? string.Empty;
                entity.DescriptionEn = dto.DescriptionEn?.Trim() ?? string.Empty;
                entity.DescriptionAr = dto.DescriptionAr?.Trim() ?? string.Empty;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Divisions section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the Divisions section: {ex.Message}");
            }
        }

        // ── Division Cards (List) ──

        public async Task<IReadOnlyList<CmsDivisionItemDto>> GetDivisionItemsByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.DivisionItems
                .AsNoTracking()
                .Include(d => d.CompanyProfile)
                .Where(d => d.CompanyProfile != null && d.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(d => d.IsPublished);
            }

            var items = await query.OrderBy(d => d.DisplayOrder).ThenBy(d => d.CreatedAtUtc).ToListAsync();
            return items.Select(MapToDivisionItemDto).ToList();
        }

        public async Task<IReadOnlyList<CmsDivisionItemDto>> GetDivisionItemsByCompanyIdAsync(Guid companyProfileId, bool includeUnpublished = true)
        {
            var query = _db.DivisionItems
                .AsNoTracking()
                .Where(d => d.CompanyProfileId == companyProfileId);

            if (!includeUnpublished)
            {
                query = query.Where(d => d.IsPublished);
            }

            var items = await query.OrderBy(d => d.DisplayOrder).ThenBy(d => d.CreatedAtUtc).ToListAsync();
            return items.Select(MapToDivisionItemDto).ToList();
        }

        public async Task<CmsDivisionItemDto?> GetDivisionItemByIdAsync(Guid id)
        {
            var entity = await _db.DivisionItems
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            return entity == null ? null : MapToDivisionItemDto(entity);
        }

        public async Task<ServiceResult<Guid>> CreateDivisionItemAsync(CmsDivisionItemUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = new CmsDivisionItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = dto.CompanyProfileId,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                ApplyDivisionItem(entity, dto);

                _db.DivisionItems.Add(entity);
                await _db.SaveChangesAsync();

                return ServiceResult<Guid>.Ok(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Division card for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult<Guid>.Fail($"An error occurred while creating the division card: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateDivisionItemAsync(CmsDivisionItemUpsertDto dto, Guid? userId = null)
        {
            if (!dto.Id.HasValue)
                return ServiceResult.Fail("Division card ID is required for update.");

            try
            {
                var entity = await _db.DivisionItems.FirstOrDefaultAsync(d => d.Id == dto.Id.Value);
                if (entity == null)
                    return ServiceResult.Fail("Division card not found.");

                ApplyDivisionItem(entity, dto);
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Division card {Id}", dto.Id);
                return ServiceResult.Fail($"An error occurred while updating the division card: {ex.Message}");
            }
        }

        public async Task<ServiceResult> DeleteDivisionItemAsync(Guid id, Guid? userId = null)
        {
            try
            {
                var entity = await _db.DivisionItems.FirstOrDefaultAsync(d => d.Id == id);
                if (entity == null)
                    return ServiceResult.Fail("Division card not found.");

                entity.IsDeleted = true;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Division card {Id}", id);
                return ServiceResult.Fail($"An error occurred while deleting the division card: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 4c. AI & ENGINEERING SECTION (#services)
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsAiEngineeringSectionDto?> GetAiEngineeringSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.AiEngineeringSections
                .AsNoTracking()
                .Include(a => a.CompanyProfile)
                .Where(a => a.CompanyProfile != null && a.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(a => a.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToAiEngineeringSectionDto(entity);
        }

        public async Task<CmsAiEngineeringSectionDto?> GetAiEngineeringSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.AiEngineeringSections
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToAiEngineeringSectionDto(entity);
        }

        public async Task<ServiceResult> UpsertAiEngineeringSectionAsync(CmsAiEngineeringSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = await _db.AiEngineeringSections
                    .FirstOrDefaultAsync(a => a.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsAiEngineeringSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.AiEngineeringSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitleEn = dto.TitleEn?.Trim() ?? string.Empty;
                entity.TitleAr = dto.TitleAr?.Trim() ?? string.Empty;
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert AI & Engineering section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the AI & Engineering section: {ex.Message}");
            }
        }

        // ── AI & Engineering Cards (List) ──

        public async Task<IReadOnlyList<CmsAiEngineeringItemDto>> GetAiEngineeringItemsByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.AiEngineeringItems
                .AsNoTracking()
                .Include(a => a.CompanyProfile)
                .Where(a => a.CompanyProfile != null && a.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(a => a.IsPublished);
            }

            var items = await query.OrderBy(a => a.DisplayOrder).ThenBy(a => a.CreatedAtUtc).ToListAsync();
            return items.Select(MapToAiEngineeringItemDto).ToList();
        }

        public async Task<IReadOnlyList<CmsAiEngineeringItemDto>> GetAiEngineeringItemsByCompanyIdAsync(Guid companyProfileId, bool includeUnpublished = true)
        {
            var query = _db.AiEngineeringItems
                .AsNoTracking()
                .Where(a => a.CompanyProfileId == companyProfileId);

            if (!includeUnpublished)
            {
                query = query.Where(a => a.IsPublished);
            }

            var items = await query.OrderBy(a => a.DisplayOrder).ThenBy(a => a.CreatedAtUtc).ToListAsync();
            return items.Select(MapToAiEngineeringItemDto).ToList();
        }

        public async Task<CmsAiEngineeringItemDto?> GetAiEngineeringItemByIdAsync(Guid id)
        {
            var entity = await _db.AiEngineeringItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return entity == null ? null : MapToAiEngineeringItemDto(entity);
        }

        public async Task<ServiceResult<Guid>> CreateAiEngineeringItemAsync(CmsAiEngineeringItemUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = new CmsAiEngineeringItem
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = dto.CompanyProfileId,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                ApplyAiEngineeringItem(entity, dto);

                _db.AiEngineeringItems.Add(entity);
                await _db.SaveChangesAsync();

                return ServiceResult<Guid>.Ok(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create AI & Engineering card for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult<Guid>.Fail($"An error occurred while creating the service card: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateAiEngineeringItemAsync(CmsAiEngineeringItemUpsertDto dto, Guid? userId = null)
        {
            if (!dto.Id.HasValue)
                return ServiceResult.Fail("Service card ID is required for update.");

            try
            {
                var entity = await _db.AiEngineeringItems.FirstOrDefaultAsync(a => a.Id == dto.Id.Value);
                if (entity == null)
                    return ServiceResult.Fail("Service card not found.");

                ApplyAiEngineeringItem(entity, dto);
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update AI & Engineering card {Id}", dto.Id);
                return ServiceResult.Fail($"An error occurred while updating the service card: {ex.Message}");
            }
        }

        public async Task<ServiceResult> DeleteAiEngineeringItemAsync(Guid id, Guid? userId = null)
        {
            try
            {
                var entity = await _db.AiEngineeringItems.FirstOrDefaultAsync(a => a.Id == id);
                if (entity == null)
                    return ServiceResult.Fail("Service card not found.");

                entity.IsDeleted = true;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete AI & Engineering card {Id}", id);
                return ServiceResult.Fail($"An error occurred while deleting the service card: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 4d. LIST-SECTION HEADERS (#portfolio, #insights, #testimonials)
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsListSectionHeaderDto?> GetListSectionHeaderByCompanySlugAsync(string slug, string sectionKey, bool includeUnpublished = false)
        {
            var key = NormaliseSectionKey(sectionKey);

            var query = _db.ListSectionHeaders
                .AsNoTracking()
                .Include(h => h.CompanyProfile)
                .Where(h => h.CompanyProfile != null && h.CompanyProfile.Slug == slug && h.SectionKey == key);

            if (!includeUnpublished)
            {
                query = query.Where(h => h.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToListSectionHeaderDto(entity);
        }

        public async Task<CmsListSectionHeaderDto?> GetListSectionHeaderByCompanyIdAsync(Guid companyProfileId, string sectionKey)
        {
            var key = NormaliseSectionKey(sectionKey);

            var entity = await _db.ListSectionHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.CompanyProfileId == companyProfileId && h.SectionKey == key);

            return entity == null ? null : MapToListSectionHeaderDto(entity);
        }

        public async Task<IReadOnlyList<CmsListSectionHeaderDto>> GetListSectionHeadersByCompanyIdAsync(Guid companyProfileId)
        {
            var items = await _db.ListSectionHeaders
                .AsNoTracking()
                .Where(h => h.CompanyProfileId == companyProfileId)
                .OrderBy(h => h.SectionKey)
                .ToListAsync();

            return items.Select(MapToListSectionHeaderDto).ToList();
        }

        public async Task<ServiceResult> UpsertListSectionHeaderAsync(CmsListSectionHeaderUpsertDto dto, Guid? userId = null)
        {
            var key = NormaliseSectionKey(dto.SectionKey);

            if (!CmsListSectionKeys.All.Contains(key))
            {
                return ServiceResult.Fail($"'{dto.SectionKey}' is not a known homepage list section.");
            }

            try
            {
                var entity = await _db.ListSectionHeaders
                    .FirstOrDefaultAsync(h => h.CompanyProfileId == dto.CompanyProfileId && h.SectionKey == key);

                if (entity == null)
                {
                    entity = new CmsListSectionHeader
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        SectionKey = key,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.ListSectionHeaders.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitlePrefixEn = dto.TitlePrefixEn ?? string.Empty;
                entity.TitleHighlightEn = dto.TitleHighlightEn ?? string.Empty;
                entity.TitlePrefixAr = dto.TitlePrefixAr ?? string.Empty;
                entity.TitleHighlightAr = dto.TitleHighlightAr ?? string.Empty;
                entity.SubtitleEn = dto.SubtitleEn?.Trim();
                entity.SubtitleAr = dto.SubtitleAr?.Trim();
                entity.ItemLinkTextEn = dto.ItemLinkTextEn?.Trim();
                entity.ItemLinkTextAr = dto.ItemLinkTextAr?.Trim();
                entity.DefaultCategoryLabelEn = dto.DefaultCategoryLabelEn?.Trim();
                entity.DefaultCategoryLabelAr = dto.DefaultCategoryLabelAr?.Trim();
                entity.ReadTimeSuffixEn = dto.ReadTimeSuffixEn?.Trim();
                entity.ReadTimeSuffixAr = dto.ReadTimeSuffixAr?.Trim();
                entity.UndatedLabelEn = dto.UndatedLabelEn?.Trim();
                entity.UndatedLabelAr = dto.UndatedLabelAr?.Trim();
                entity.ButtonTextEn = dto.ButtonTextEn?.Trim();
                entity.ButtonTextAr = dto.ButtonTextAr?.Trim();
                entity.ButtonUrl = dto.ButtonUrl?.Trim();
                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert list section header {Key} for company {CompanyId}", key, dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the section header: {ex.Message}");
            }
        }

        private static string NormaliseSectionKey(string? sectionKey) =>
            (sectionKey ?? string.Empty).Trim().ToLowerInvariant();

        private static void ApplyDivisionItem(CmsDivisionItem entity, CmsDivisionItemUpsertDto dto)
        {
            entity.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? "layers" : dto.IconName.Trim();
            entity.TitleEn = dto.TitleEn?.Trim() ?? string.Empty;
            entity.TitleAr = dto.TitleAr?.Trim() ?? string.Empty;
            entity.DescriptionEn = dto.DescriptionEn?.Trim() ?? string.Empty;
            entity.DescriptionAr = dto.DescriptionAr?.Trim() ?? string.Empty;
            entity.LinkUrl = string.IsNullOrWhiteSpace(dto.LinkUrl) ? "/" : dto.LinkUrl.Trim();
            entity.LinkTextEn = string.IsNullOrWhiteSpace(dto.LinkTextEn) ? "Explore" : dto.LinkTextEn.Trim();
            entity.LinkTextAr = string.IsNullOrWhiteSpace(dto.LinkTextAr) ? "المزيد" : dto.LinkTextAr.Trim();
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsPublished = dto.IsPublished;
        }

        private static void ApplyAiEngineeringItem(CmsAiEngineeringItem entity, CmsAiEngineeringItemUpsertDto dto)
        {
            entity.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? "code" : dto.IconName.Trim();
            entity.TitleEn = dto.TitleEn?.Trim() ?? string.Empty;
            entity.TitleAr = dto.TitleAr?.Trim() ?? string.Empty;
            entity.DescriptionEn = dto.DescriptionEn?.Trim() ?? string.Empty;
            entity.DescriptionAr = dto.DescriptionAr?.Trim() ?? string.Empty;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsPublished = dto.IsPublished;
        }

        private static CmsDivisionsSectionDto MapToDivisionsSectionDto(CmsDivisionsSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            DescriptionEn = entity.DescriptionEn,
            DescriptionAr = entity.DescriptionAr,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsDivisionItemDto MapToDivisionItemDto(CmsDivisionItem entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            IconName = entity.IconName,
            TitleEn = entity.TitleEn,
            TitleAr = entity.TitleAr,
            DescriptionEn = entity.DescriptionEn,
            DescriptionAr = entity.DescriptionAr,
            LinkUrl = entity.LinkUrl,
            LinkTextEn = entity.LinkTextEn,
            LinkTextAr = entity.LinkTextAr,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsAiEngineeringSectionDto MapToAiEngineeringSectionDto(CmsAiEngineeringSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitleEn = entity.TitleEn,
            TitleAr = entity.TitleAr,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsAiEngineeringItemDto MapToAiEngineeringItemDto(CmsAiEngineeringItem entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            IconName = entity.IconName,
            TitleEn = entity.TitleEn,
            TitleAr = entity.TitleAr,
            DescriptionEn = entity.DescriptionEn,
            DescriptionAr = entity.DescriptionAr,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsListSectionHeaderDto MapToListSectionHeaderDto(CmsListSectionHeader entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            SectionKey = entity.SectionKey,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            SubtitleEn = entity.SubtitleEn,
            SubtitleAr = entity.SubtitleAr,
            ItemLinkTextEn = entity.ItemLinkTextEn,
            ItemLinkTextAr = entity.ItemLinkTextAr,
            DefaultCategoryLabelEn = entity.DefaultCategoryLabelEn,
            DefaultCategoryLabelAr = entity.DefaultCategoryLabelAr,
            ReadTimeSuffixEn = entity.ReadTimeSuffixEn,
            ReadTimeSuffixAr = entity.ReadTimeSuffixAr,
            UndatedLabelEn = entity.UndatedLabelEn,
            UndatedLabelAr = entity.UndatedLabelAr,
            ButtonTextEn = entity.ButtonTextEn,
            ButtonTextAr = entity.ButtonTextAr,
            ButtonUrl = entity.ButtonUrl,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        // ════════════════════════════════════════════════════════════════
        // 5. VISION & IMPLEMENTATION FRAMEWORK SECTION (#ethics)
        // ════════════════════════════════════════════════════════════════

        public async Task<CmsEthicsSectionDto?> GetEthicsSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.EthicsSections
                .AsNoTracking()
                .Include(p => p.CompanyProfile)
                .Where(p => p.CompanyProfile != null && p.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToEthicsDto(entity);
        }

        public async Task<CmsEthicsSectionDto?> GetEthicsSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.EthicsSections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToEthicsDto(entity);
        }

        public async Task<CmsEthicsSectionDto?> GetEthicsSectionByIdAsync(Guid id)
        {
            var entity = await _db.EthicsSections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity == null ? null : MapToEthicsDto(entity);
        }

        public async Task<ServiceResult> UpsertEthicsSectionAsync(CmsEthicsSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var entity = await _db.EthicsSections
                    .FirstOrDefaultAsync(p => p.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsEthicsSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.EthicsSections.Add(entity);
                }
                else
                {
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                entity.BadgeEn = dto.BadgeEn?.Trim() ?? string.Empty;
                entity.BadgeAr = dto.BadgeAr?.Trim() ?? string.Empty;
                entity.TitlePrefixEn = dto.TitlePrefixEn ?? string.Empty;
                entity.TitleHighlightEn = dto.TitleHighlightEn?.Trim() ?? string.Empty;
                entity.TitlePrefixAr = dto.TitlePrefixAr ?? string.Empty;
                entity.TitleHighlightAr = dto.TitleHighlightAr?.Trim() ?? string.Empty;
                entity.DescriptionEn = dto.DescriptionEn?.Trim() ?? string.Empty;
                entity.DescriptionAr = dto.DescriptionAr?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(dto.TopImagePath))
                    entity.TopImagePath = dto.TopImagePath.Trim();
                entity.TopImageAltEn = dto.TopImageAltEn?.Trim() ?? string.Empty;
                entity.TopImageAltAr = dto.TopImageAltAr?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(dto.BottomImagePath))
                    entity.BottomImagePath = dto.BottomImagePath.Trim();
                entity.BottomImageAltEn = dto.BottomImageAltEn?.Trim() ?? string.Empty;
                entity.BottomImageAltAr = dto.BottomImageAltAr?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(dto.BackgroundImagePath))
                    entity.BackgroundImagePath = dto.BackgroundImagePath.Trim();

                entity.IsPublished = dto.IsPublished;

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Ethics section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the Ethics section: {ex.Message}");
            }
        }

        // ── Call To Action Section (#cta) ──

        public async Task<CmsCtaSectionDto?> GetCtaSectionByCompanySlugAsync(string slug, bool includeUnpublished = false)
        {
            var query = _db.CtaSections
                .AsNoTracking()
                .Include(c => c.CompanyProfile)
                .Where(c => c.CompanyProfile != null && c.CompanyProfile.Slug == slug);

            if (!includeUnpublished)
            {
                query = query.Where(c => c.IsPublished);
            }

            var entity = await query.FirstOrDefaultAsync();
            return entity == null ? null : MapToCtaDto(entity);
        }

        public async Task<CmsCtaSectionDto?> GetCtaSectionByCompanyIdAsync(Guid companyProfileId)
        {
            var entity = await _db.CtaSections
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyProfileId == companyProfileId);

            return entity == null ? null : MapToCtaDto(entity);
        }

        public async Task<CmsCtaSectionDto?> GetCtaSectionByIdAsync(Guid id)
        {
            var entity = await _db.CtaSections
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return entity == null ? null : MapToCtaDto(entity);
        }

        public async Task<ServiceResult> UpsertCtaSectionAsync(CmsCtaSectionUpsertDto dto, Guid? userId = null)
        {
            try
            {
                var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
                if (!companyExists)
                {
                    return ServiceResult.Fail("The specified company profile does not exist.");
                }

                var entity = await _db.CtaSections
                    .FirstOrDefaultAsync(c => c.CompanyProfileId == dto.CompanyProfileId);

                if (entity == null)
                {
                    entity = new CmsCtaSection
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = dto.CompanyProfileId,
                        BadgeEn = dto.BadgeEn?.Trim() ?? "Let's Build Together",
                        BadgeAr = dto.BadgeAr?.Trim() ?? "لنبني معاً",
                        TitlePrefixEn = dto.TitlePrefixEn ?? "Ready to engineer",
                        TitleHighlightEn = dto.TitleHighlightEn ?? " your future?",
                        TitlePrefixAr = dto.TitlePrefixAr ?? "مستعد لهندسة",
                        TitleHighlightAr = dto.TitleHighlightAr ?? " مستقبلك؟",
                        ButtonTextEn = dto.ButtonTextEn?.Trim() ?? "Contact Us",
                        ButtonTextAr = dto.ButtonTextAr?.Trim() ?? "تواصل معنا",
                        ButtonUrl = dto.ButtonUrl?.Trim() ?? "/Home/Contact",
                        ContactEmail = dto.ContactEmail?.Trim() ?? "neurix@aidaleel.com",
                        BackgroundImagePath = dto.BackgroundImagePath?.Trim(),
                        IsPublished = dto.IsPublished,
                        CreatedByUserId = userId,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _db.CtaSections.Add(entity);
                }
                else
                {
                    entity.BadgeEn = dto.BadgeEn?.Trim() ?? "Let's Build Together";
                    entity.BadgeAr = dto.BadgeAr?.Trim() ?? "لنبني معاً";
                    entity.TitlePrefixEn = dto.TitlePrefixEn ?? "Ready to engineer";
                    entity.TitleHighlightEn = dto.TitleHighlightEn ?? " your future?";
                    entity.TitlePrefixAr = dto.TitlePrefixAr ?? "مستعد لهندسة";
                    entity.TitleHighlightAr = dto.TitleHighlightAr ?? " مستقبلك؟";
                    entity.ButtonTextEn = dto.ButtonTextEn?.Trim() ?? "Contact Us";
                    entity.ButtonTextAr = dto.ButtonTextAr?.Trim() ?? "تواصل معنا";
                    entity.ButtonUrl = dto.ButtonUrl?.Trim() ?? "/Home/Contact";
                    entity.ContactEmail = dto.ContactEmail?.Trim() ?? "neurix@aidaleel.com";
                    
                    if (!string.IsNullOrWhiteSpace(dto.BackgroundImagePath))
                    {
                        entity.BackgroundImagePath = dto.BackgroundImagePath.Trim();
                    }

                    entity.IsPublished = dto.IsPublished;
                    entity.UpdatedAtUtc = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert CTA section for company {CompanyId}", dto.CompanyProfileId);
                return ServiceResult.Fail($"An error occurred while saving the CTA section: {ex.Message}");
            }
        }

        private static CmsPillarsSectionDto MapToPillarsSectionDto(CmsPillarsSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            TitleEn = entity.TitleEn,
            TitleAr = entity.TitleAr,
            SubtitleEn = entity.SubtitleEn,
            SubtitleAr = entity.SubtitleAr,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsPillarItemDto MapToPillarItemDto(CmsPillarItem entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            IconName = entity.IconName,
            TitleEn = entity.TitleEn,
            TitleAr = entity.TitleAr,
            DescriptionEn = entity.DescriptionEn,
            DescriptionAr = entity.DescriptionAr,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsEthicsSectionDto MapToEthicsDto(CmsEthicsSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            DescriptionEn = entity.DescriptionEn,
            DescriptionAr = entity.DescriptionAr,
            TopImagePath = entity.TopImagePath,
            TopImageAltEn = entity.TopImageAltEn,
            TopImageAltAr = entity.TopImageAltAr,
            BottomImagePath = entity.BottomImagePath,
            BottomImageAltEn = entity.BottomImageAltEn,
            BottomImageAltAr = entity.BottomImageAltAr,
            BackgroundImagePath = entity.BackgroundImagePath,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        private static CmsCtaSectionDto MapToCtaDto(CmsCtaSection entity) => new()
        {
            Id = entity.Id,
            CompanyProfileId = entity.CompanyProfileId,
            BadgeEn = entity.BadgeEn,
            BadgeAr = entity.BadgeAr,
            TitlePrefixEn = entity.TitlePrefixEn,
            TitleHighlightEn = entity.TitleHighlightEn,
            TitlePrefixAr = entity.TitlePrefixAr,
            TitleHighlightAr = entity.TitleHighlightAr,
            ButtonTextEn = entity.ButtonTextEn,
            ButtonTextAr = entity.ButtonTextAr,
            ButtonUrl = entity.ButtonUrl,
            ContactEmail = entity.ContactEmail,
            BackgroundImagePath = entity.BackgroundImagePath,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
