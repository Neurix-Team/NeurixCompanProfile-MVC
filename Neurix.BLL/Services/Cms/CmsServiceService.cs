using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsServiceService : ICmsServiceService
    {
        private readonly CmsDbContext _db;

        public CmsServiceService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsServiceSummaryDto>> GetServicesByCompanyAsync(Guid? companyProfileId = null, bool includeUnpublished = false, CancellationToken cancellationToken = default)
        {
            var query = _db.Services
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
                .ThenBy(s => s.NameEn)
                .Select(s => new CmsServiceSummaryDto
                {
                    Id = s.Id,
                    CompanyProfileId = s.CompanyProfileId,
                    CompanyNameEn = s.CompanyProfile != null ? s.CompanyProfile.NameEn : string.Empty,
                    Slug = s.Slug,
                    NameEn = s.NameEn,
                    NameAr = s.NameAr,
                    ShortDescriptionEn = s.ShortDescriptionEn,
                    ShortDescriptionAr = s.ShortDescriptionAr,
                    IconName = s.IconName,
                    ImagePath = s.ImagePath,
                    DisplayOrder = s.DisplayOrder,
                    IsPublished = s.IsPublished,
                    CreatedAtUtc = s.CreatedAtUtc,
                    UpdatedAtUtc = s.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CmsServiceSummaryDto>> GetPublishedServicesByCompanySlugAsync(string companySlug, CancellationToken cancellationToken = default)
        {
            var normalizedCompanySlug = NormalizeSlug(companySlug);

            return await _db.Services
                .AsNoTracking()
                .Include(s => s.CompanyProfile)
                .Where(s => s.IsPublished && s.CompanyProfile != null && s.CompanyProfile.IsPublished && s.CompanyProfile.Slug == normalizedCompanySlug)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.NameEn)
                .Select(s => new CmsServiceSummaryDto
                {
                    Id = s.Id,
                    CompanyProfileId = s.CompanyProfileId,
                    CompanyNameEn = s.CompanyProfile != null ? s.CompanyProfile.NameEn : string.Empty,
                    Slug = s.Slug,
                    NameEn = s.NameEn,
                    NameAr = s.NameAr,
                    ShortDescriptionEn = s.ShortDescriptionEn,
                    ShortDescriptionAr = s.ShortDescriptionAr,
                    IconName = s.IconName,
                    ImagePath = s.ImagePath,
                    DisplayOrder = s.DisplayOrder,
                    IsPublished = s.IsPublished,
                    CreatedAtUtc = s.CreatedAtUtc,
                    UpdatedAtUtc = s.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<CmsServiceDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Services
                .AsNoTracking()
                .Include(s => s.CompanyProfile)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            return entity == null ? null : MapToDetail(entity);
        }

        public async Task<CmsServiceDetailDto?> GetBySlugAsync(Guid companyProfileId, string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = NormalizeSlug(slug);
            var entity = await _db.Services
                .AsNoTracking()
                .Include(s => s.CompanyProfile)
                .FirstOrDefaultAsync(s => s.CompanyProfileId == companyProfileId && s.Slug == normalizedSlug, cancellationToken);

            return entity == null ? null : MapToDetail(entity);
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsServiceUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var companyExists = await _db.CompanyProfiles
                .AnyAsync(p => p.Id == dto.CompanyProfileId, cancellationToken);

            if (!companyExists)
            {
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");
            }

            var slug = NormalizeSlug(dto.Slug);

            var slugExists = await _db.Services
                .AnyAsync(s => s.CompanyProfileId == dto.CompanyProfileId && s.Slug == slug, cancellationToken);

            if (slugExists)
            {
                return ServiceResult<Guid>.Fail($"A service with the slug '{slug}' already exists for this company profile.");
            }

            var service = new CmsService
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Slug = slug,
                NameEn = dto.NameEn.Trim(),
                NameAr = dto.NameAr.Trim(),
                ShortDescriptionEn = dto.ShortDescriptionEn?.Trim(),
                ShortDescriptionAr = dto.ShortDescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                DescriptionAr = dto.DescriptionAr?.Trim(),
                IconName = dto.IconName?.Trim(),
                ImagePath = dto.ImagePath?.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.Services.Add(service);
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Ok(service.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsServiceUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (service == null)
            {
                return ServiceResult.Fail("Service not found.");
            }

            var companyExists = await _db.CompanyProfiles
                .AnyAsync(p => p.Id == dto.CompanyProfileId, cancellationToken);

            if (!companyExists)
            {
                return ServiceResult.Fail("Selected company profile does not exist.");
            }

            var slug = NormalizeSlug(dto.Slug);

            var slugConflict = await _db.Services
                .AnyAsync(s => s.Id != id && s.CompanyProfileId == dto.CompanyProfileId && s.Slug == slug, cancellationToken);

            if (slugConflict)
            {
                return ServiceResult.Fail($"A service with the slug '{slug}' already exists for this company profile.");
            }

            service.CompanyProfileId = dto.CompanyProfileId;
            service.Slug = slug;
            service.NameEn = dto.NameEn.Trim();
            service.NameAr = dto.NameAr.Trim();
            service.ShortDescriptionEn = dto.ShortDescriptionEn?.Trim();
            service.ShortDescriptionAr = dto.ShortDescriptionAr?.Trim();
            service.DescriptionEn = dto.DescriptionEn?.Trim();
            service.DescriptionAr = dto.DescriptionAr?.Trim();
            service.IconName = dto.IconName?.Trim();
            service.ImagePath = dto.ImagePath?.Trim();
            service.DisplayOrder = dto.DisplayOrder;
            service.IsPublished = dto.IsPublished;

            await _db.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (service == null)
            {
                return ServiceResult.Fail("Service not found.");
            }

            service.IsPublished = isPublished;
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateDisplayOrderAsync(Guid id, int displayOrder, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (service == null)
            {
                return ServiceResult.Fail("Service not found.");
            }

            service.DisplayOrder = displayOrder;
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (service == null)
            {
                return ServiceResult.Fail("Service not found.");
            }

            service.IsDeleted = true;
            await _db.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        }

        private static string NormalizeSlug(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            return raw.Trim().ToLowerInvariant().Replace(" ", "-");
        }

        private static CmsServiceDetailDto MapToDetail(CmsService s)
        {
            return new CmsServiceDetailDto
            {
                Id = s.Id,
                CompanyProfileId = s.CompanyProfileId,
                CompanyNameEn = s.CompanyProfile?.NameEn ?? string.Empty,
                CompanySlug = s.CompanyProfile?.Slug ?? string.Empty,
                Slug = s.Slug,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                ShortDescriptionEn = s.ShortDescriptionEn,
                ShortDescriptionAr = s.ShortDescriptionAr,
                DescriptionEn = s.DescriptionEn,
                DescriptionAr = s.DescriptionAr,
                IconName = s.IconName,
                ImagePath = s.ImagePath,
                DisplayOrder = s.DisplayOrder,
                IsPublished = s.IsPublished,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            };
        }
    }
}
