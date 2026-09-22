using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsSocialLinkService : ICmsSocialLinkService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        private readonly CmsDbContext _db;
        private readonly IMemoryCache _cache;

        public CmsSocialLinkService(CmsDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<IReadOnlyList<CmsSocialLinkSummaryDto>> GetLinksByCompanyAsync(Guid? companyProfileId = null, bool includeUnpublished = false, CancellationToken cancellationToken = default)
        {
            var query = _db.SocialLinks
                .AsNoTracking()
                .Include(s => s.CompanyProfile)
                .AsQueryable();

            if (companyProfileId.HasValue)
            {
                query = query.Where(s => s.CompanyProfileId == companyProfileId.Value);
            }

            if (!includeUnpublished)
            {
                query = query.Where(s => s.IsPublished);
            }

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Platform)
                .Select(s => new CmsSocialLinkSummaryDto
                {
                    Id = s.Id,
                    CompanyProfileId = s.CompanyProfileId,
                    CompanyNameEn = s.CompanyProfile != null ? s.CompanyProfile.NameEn : string.Empty,
                    Platform = s.Platform,
                    Url = s.Url,
                    DisplayName = s.DisplayName,
                    IconName = s.IconName,
                    DisplayOrder = s.DisplayOrder,
                    IsPublished = s.IsPublished,
                    CreatedAtUtc = s.CreatedAtUtc,
                    UpdatedAtUtc = s.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CmsSocialLinkSummaryDto>> GetPublishedLinksByCompanySlugAsync(string companySlug, CancellationToken cancellationToken = default)
        {
            var normalizedCompanySlug = (companySlug ?? string.Empty).Trim().ToLowerInvariant();
            var cacheKey = $"cms:social-links:{normalizedCompanySlug}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CmsSocialLinkSummaryDto>? cached) && cached is not null)
            {
                return cached;
            }

            var result = await _db.SocialLinks
                .AsNoTracking()
                .Include(s => s.CompanyProfile)
                .Where(s => s.IsPublished && s.CompanyProfile != null && s.CompanyProfile.IsPublished && s.CompanyProfile.Slug == normalizedCompanySlug)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Platform)
                .Select(s => new CmsSocialLinkSummaryDto
                {
                    Id = s.Id,
                    CompanyProfileId = s.CompanyProfileId,
                    CompanyNameEn = s.CompanyProfile != null ? s.CompanyProfile.NameEn : string.Empty,
                    Platform = s.Platform,
                    Url = s.Url,
                    DisplayName = s.DisplayName,
                    IconName = s.IconName,
                    DisplayOrder = s.DisplayOrder,
                    IsPublished = s.IsPublished,
                    CreatedAtUtc = s.CreatedAtUtc,
                    UpdatedAtUtc = s.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }

        public async Task<CmsSocialLinkDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.SocialLinks
                .AsNoTracking()
                .Include(s => s.CompanyProfile)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            return entity == null ? null : MapToDetail(entity);
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsSocialLinkUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var companyExists = await _db.CompanyProfiles
                .AnyAsync(p => p.Id == dto.CompanyProfileId, cancellationToken);

            if (!companyExists)
            {
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");
            }

            var platform = (dto.Platform ?? string.Empty).Trim().ToLowerInvariant();
            var iconName = string.IsNullOrWhiteSpace(dto.IconName) ? platform : dto.IconName.Trim().ToLowerInvariant();
            var displayName = string.IsNullOrWhiteSpace(dto.DisplayName)
                ? GetDefaultDisplayName(platform)
                : dto.DisplayName.Trim();

            var link = new CmsSocialLink
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Platform = platform,
                Url = dto.Url.Trim(),
                DisplayName = displayName,
                IconName = iconName,
                DisplayOrder = dto.DisplayOrder,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.SocialLinks.Add(link);
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Ok(link.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsSocialLinkUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var link = await _db.SocialLinks.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (link == null)
            {
                return ServiceResult.Fail("Social link not found.");
            }

            var companyExists = await _db.CompanyProfiles
                .AnyAsync(p => p.Id == dto.CompanyProfileId, cancellationToken);

            if (!companyExists)
            {
                return ServiceResult.Fail("Selected company profile does not exist.");
            }

            var platform = (dto.Platform ?? string.Empty).Trim().ToLowerInvariant();
            var iconName = string.IsNullOrWhiteSpace(dto.IconName) ? platform : dto.IconName.Trim().ToLowerInvariant();
            var displayName = string.IsNullOrWhiteSpace(dto.DisplayName)
                ? GetDefaultDisplayName(platform)
                : dto.DisplayName.Trim();

            link.CompanyProfileId = dto.CompanyProfileId;
            link.Platform = platform;
            link.Url = dto.Url.Trim();
            link.DisplayName = displayName;
            link.IconName = iconName;
            link.DisplayOrder = dto.DisplayOrder;
            link.IsPublished = dto.IsPublished;

            await _db.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var link = await _db.SocialLinks.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (link == null)
            {
                return ServiceResult.Fail("Social link not found.");
            }

            link.IsPublished = isPublished;
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateDisplayOrderAsync(Guid id, int displayOrder, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var link = await _db.SocialLinks.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (link == null)
            {
                return ServiceResult.Fail("Social link not found.");
            }

            link.DisplayOrder = displayOrder;
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var link = await _db.SocialLinks.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (link == null)
            {
                return ServiceResult.Fail("Social link not found.");
            }

            link.IsDeleted = true;
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        }

        private static string GetDefaultDisplayName(string platform)
        {
            return platform switch
            {
                "linkedin" => "LinkedIn",
                "twitter" or "x" => "Twitter / X",
                "facebook" => "Facebook",
                "instagram" => "Instagram",
                "youtube" => "YouTube",
                "github" => "GitHub",
                "discord" => "Discord",
                _ => char.ToUpperInvariant(platform[0]) + platform[1..]
            };
        }

        private static CmsSocialLinkDetailDto MapToDetail(CmsSocialLink s)
        {
            return new CmsSocialLinkDetailDto
            {
                Id = s.Id,
                CompanyProfileId = s.CompanyProfileId,
                CompanyNameEn = s.CompanyProfile?.NameEn ?? string.Empty,
                CompanySlug = s.CompanyProfile?.Slug ?? string.Empty,
                Platform = s.Platform,
                Url = s.Url,
                DisplayName = s.DisplayName,
                IconName = s.IconName,
                DisplayOrder = s.DisplayOrder,
                IsPublished = s.IsPublished,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            };
        }
    }
}
