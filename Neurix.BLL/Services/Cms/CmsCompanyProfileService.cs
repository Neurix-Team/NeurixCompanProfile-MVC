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
    public class CmsCompanyProfileService : ICmsCompanyProfileService
    {
        // Every page reads the company profile at least once (navbar, footer, and the
        // page action all ask for the same "neurix" row independently), so a short cache
        // turns 2-3 redundant round trips per request into one DB read per cache window.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        private readonly CmsDbContext _db;
        private readonly IMemoryCache _cache;

        public CmsCompanyProfileService(CmsDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<IReadOnlyList<CmsCompanyProfileSummaryDto>> GetAllProfilesAsync(bool includeUnpublished = false, CancellationToken cancellationToken = default)
        {
            var query = _db.CompanyProfiles.AsNoTracking();

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            return await query
                .OrderBy(p => p.NameEn)
                .Select(p => new CmsCompanyProfileSummaryDto
                {
                    Id = p.Id,
                    Slug = p.Slug,
                    NameEn = p.NameEn,
                    NameAr = p.NameAr,
                    LogoPath = p.LogoPath,
                    FaviconPath = p.FaviconPath,
                    PrimaryColor = p.PrimaryColor,
                    AccentColor = p.AccentColor,
                    TaglineEn = p.TaglineEn,
                    TaglineAr = p.TaglineAr,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<CmsCompanyProfileDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CompanyProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return entity == null ? null : MapToDetail(entity);
        }

        public async Task<CmsCompanyProfileDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = NormalizeSlug(slug);
            var cacheKey = CacheKeyForSlug(normalizedSlug);

            if (_cache.TryGetValue(cacheKey, out CmsCompanyProfileDetailDto? cached))
            {
                return cached;
            }

            var entity = await _db.CompanyProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == normalizedSlug && p.IsPublished, cancellationToken);

            var dto = entity == null ? null : MapToDetail(entity);
            _cache.Set(cacheKey, dto, CacheDuration);
            return dto;
        }

        private static string CacheKeyForSlug(string normalizedSlug) => $"cms:company-profile:{normalizedSlug}";

        public async Task<CmsCompanyProfileDetailDto?> GetDefaultPublishedProfileAsync(CancellationToken cancellationToken = default)
        {
            // Default to "neurix" if published, otherwise first published profile
            var entity = await _db.CompanyProfiles
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.Slug == "neurix")
                .ThenBy(p => p.NameEn)
                .FirstOrDefaultAsync(cancellationToken);

            return entity == null ? null : MapToDetail(entity);
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsCompanyProfileUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var slug = NormalizeSlug(dto.Slug);

            var slugExists = await _db.CompanyProfiles
                .AnyAsync(p => p.Slug == slug, cancellationToken);

            if (slugExists)
            {
                return ServiceResult<Guid>.Fail($"A company profile with the slug '{slug}' already exists.");
            }

            var profile = new CmsCompanyProfile
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                NameEn = dto.NameEn.Trim(),
                NameAr = dto.NameAr.Trim(),
                LogoPath = dto.LogoPath?.Trim(),
                FaviconPath = dto.FaviconPath?.Trim(),
                PrimaryColor = dto.PrimaryColor?.Trim(),
                AccentColor = dto.AccentColor?.Trim(),
                TaglineEn = dto.TaglineEn?.Trim(),
                TaglineAr = dto.TaglineAr?.Trim(),
                ShortDescriptionEn = dto.ShortDescriptionEn?.Trim(),
                ShortDescriptionAr = dto.ShortDescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                DescriptionAr = dto.DescriptionAr?.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                AddressEn = dto.AddressEn?.Trim(),
                AddressAr = dto.AddressAr?.Trim(),
                Website = dto.Website?.Trim(),
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.CompanyProfiles.Add(profile);
            await _db.SaveChangesAsync(cancellationToken);
            _cache.Remove(CacheKeyForSlug(slug));

            return ServiceResult<Guid>.Ok(profile.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsCompanyProfileUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var profile = await _db.CompanyProfiles.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (profile == null)
            {
                return ServiceResult.Fail("Company profile not found.");
            }

            var previousSlug = profile.Slug;
            var slug = NormalizeSlug(dto.Slug);

            var slugConflict = await _db.CompanyProfiles
                .AnyAsync(p => p.Id != id && p.Slug == slug, cancellationToken);

            if (slugConflict)
            {
                return ServiceResult.Fail($"A company profile with the slug '{slug}' already exists.");
            }

            profile.Slug = slug;
            profile.NameEn = dto.NameEn.Trim();
            profile.NameAr = dto.NameAr.Trim();
            profile.LogoPath = dto.LogoPath?.Trim();
            profile.FaviconPath = dto.FaviconPath?.Trim();
            profile.PrimaryColor = dto.PrimaryColor?.Trim();
            profile.AccentColor = dto.AccentColor?.Trim();
            profile.TaglineEn = dto.TaglineEn?.Trim();
            profile.TaglineAr = dto.TaglineAr?.Trim();
            profile.ShortDescriptionEn = dto.ShortDescriptionEn?.Trim();
            profile.ShortDescriptionAr = dto.ShortDescriptionAr?.Trim();
            profile.DescriptionEn = dto.DescriptionEn?.Trim();
            profile.DescriptionAr = dto.DescriptionAr?.Trim();
            profile.Email = dto.Email?.Trim();
            profile.Phone = dto.Phone?.Trim();
            profile.AddressEn = dto.AddressEn?.Trim();
            profile.AddressAr = dto.AddressAr?.Trim();
            profile.Website = dto.Website?.Trim();
            profile.IsPublished = dto.IsPublished;

            await _db.SaveChangesAsync(cancellationToken);
            _cache.Remove(CacheKeyForSlug(previousSlug));
            _cache.Remove(CacheKeyForSlug(slug));
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var profile = await _db.CompanyProfiles.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (profile == null)
            {
                return ServiceResult.Fail("Company profile not found.");
            }

            profile.IsPublished = isPublished;
            await _db.SaveChangesAsync(cancellationToken);
            _cache.Remove(CacheKeyForSlug(profile.Slug));

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var profile = await _db.CompanyProfiles.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (profile == null)
            {
                return ServiceResult.Fail("Company profile not found.");
            }

            profile.IsDeleted = true;
            await _db.SaveChangesAsync(cancellationToken);
            _cache.Remove(CacheKeyForSlug(profile.Slug));

            return ServiceResult.Ok();
        }

        private static string NormalizeSlug(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            return raw.Trim().ToLowerInvariant().Replace(" ", "-");
        }

        private static CmsCompanyProfileDetailDto MapToDetail(CmsCompanyProfile p)
        {
            return new CmsCompanyProfileDetailDto
            {
                Id = p.Id,
                Slug = p.Slug,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                LogoPath = p.LogoPath,
                FaviconPath = p.FaviconPath,
                PrimaryColor = p.PrimaryColor,
                AccentColor = p.AccentColor,
                TaglineEn = p.TaglineEn,
                TaglineAr = p.TaglineAr,
                ShortDescriptionEn = p.ShortDescriptionEn,
                ShortDescriptionAr = p.ShortDescriptionAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                Email = p.Email,
                Phone = p.Phone,
                AddressEn = p.AddressEn,
                AddressAr = p.AddressAr,
                Website = p.Website,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc
            };
        }
    }
}
