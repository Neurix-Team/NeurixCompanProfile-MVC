using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsDivisionPageService : ICmsDivisionPageService
    {
        private readonly CmsDbContext _db;

        public CmsDivisionPageService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsDivisionPageDetailDto>> GetDivisionPagesByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false)
        {
            var query = _db.DivisionPages
                .Include(p => p.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(p => p.CompanyProfileId == companyProfileId.Value);
            }

            if (!includeUnpublished)
            {
                query = query.Where(p => p.IsPublished && p.CompanyProfile != null && p.CompanyProfile.IsPublished);
            }

            return await query
                .OrderBy(p => p.Slug)
                .Select(p => new CmsDivisionPageDetailDto
                {
                    Id = p.Id,
                    CompanyProfileId = p.CompanyProfileId,
                    CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                    CompanySlug = p.CompanyProfile != null ? p.CompanyProfile.Slug : string.Empty,
                    Slug = p.Slug,
                    HeroTitleEn = p.HeroTitleEn,
                    HeroTitleAr = p.HeroTitleAr,
                    HeroSubtitleEn = p.HeroSubtitleEn,
                    HeroSubtitleAr = p.HeroSubtitleAr,
                    MissionEn = p.MissionEn,
                    MissionAr = p.MissionAr,
                    ContentJson = p.ContentJson,
                    CoverImagePath = p.CoverImagePath,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<CmsDivisionPageDetailDto?> GetByIdAsync(Guid id)
        {
            var p = await _db.DivisionPages
                .Include(item => item.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (p == null) return null;

            return new CmsDivisionPageDetailDto
            {
                Id = p.Id,
                CompanyProfileId = p.CompanyProfileId,
                CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                CompanySlug = p.CompanyProfile != null ? p.CompanyProfile.Slug : string.Empty,
                Slug = p.Slug,
                HeroTitleEn = p.HeroTitleEn,
                HeroTitleAr = p.HeroTitleAr,
                HeroSubtitleEn = p.HeroSubtitleEn,
                HeroSubtitleAr = p.HeroSubtitleAr,
                MissionEn = p.MissionEn,
                MissionAr = p.MissionAr,
                ContentJson = p.ContentJson,
                CoverImagePath = p.CoverImagePath,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc
            };
        }

        public async Task<CmsDivisionPageDetailDto?> GetBySlugAsync(string companySlug, string divisionSlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug) || string.IsNullOrWhiteSpace(divisionSlug))
                return null;

            var normalizedCompany = companySlug.Trim().ToLowerInvariant();
            var normalizedSlug = divisionSlug.Trim().ToLowerInvariant();

            var p = await _db.DivisionPages
                .Include(item => item.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IsPublished &&
                                             item.CompanyProfile != null &&
                                             item.CompanyProfile.IsPublished &&
                                             item.CompanyProfile.Slug == normalizedCompany &&
                                             item.Slug == normalizedSlug);

            if (p == null) return null;

            return new CmsDivisionPageDetailDto
            {
                Id = p.Id,
                CompanyProfileId = p.CompanyProfileId,
                CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                CompanySlug = p.CompanyProfile != null ? p.CompanyProfile.Slug : string.Empty,
                Slug = p.Slug,
                HeroTitleEn = p.HeroTitleEn,
                HeroTitleAr = p.HeroTitleAr,
                HeroSubtitleEn = p.HeroSubtitleEn,
                HeroSubtitleAr = p.HeroSubtitleAr,
                MissionEn = p.MissionEn,
                MissionAr = p.MissionAr,
                ContentJson = p.ContentJson,
                CoverImagePath = p.CoverImagePath,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc
            };
        }

        public async Task<ServiceResult<Guid>> UpsertAsync(CmsDivisionPageUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult<Guid>.Fail("Division page data is required.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");

            var slug = dto.Slug.Trim().ToLowerInvariant();

            var existing = await _db.DivisionPages
                .FirstOrDefaultAsync(p => p.CompanyProfileId == dto.CompanyProfileId && p.Slug == slug);

            if (existing != null)
            {
                existing.HeroTitleEn = dto.HeroTitleEn.Trim();
                existing.HeroTitleAr = dto.HeroTitleAr.Trim();
                existing.HeroSubtitleEn = dto.HeroSubtitleEn?.Trim();
                existing.HeroSubtitleAr = dto.HeroSubtitleAr?.Trim();
                existing.MissionEn = dto.MissionEn?.Trim();
                existing.MissionAr = dto.MissionAr?.Trim();
                existing.ContentJson = dto.ContentJson?.Trim();
                existing.CoverImagePath = dto.CoverImagePath?.Trim();
                existing.IsPublished = dto.IsPublished;
                existing.CreatedByUserId = userId ?? existing.CreatedByUserId;

                await _db.SaveChangesAsync();
                return ServiceResult<Guid>.Ok(existing.Id);
            }

            var entity = new CmsDivisionPage
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Slug = slug,
                HeroTitleEn = dto.HeroTitleEn.Trim(),
                HeroTitleAr = dto.HeroTitleAr.Trim(),
                HeroSubtitleEn = dto.HeroSubtitleEn?.Trim(),
                HeroSubtitleAr = dto.HeroSubtitleAr?.Trim(),
                MissionEn = dto.MissionEn?.Trim(),
                MissionAr = dto.MissionAr?.Trim(),
                ContentJson = dto.ContentJson?.Trim(),
                CoverImagePath = dto.CoverImagePath?.Trim(),
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.DivisionPages.Add(entity);
            await _db.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(entity.Id);
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null)
        {
            var entity = await _db.DivisionPages.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Division page not found.");

            entity.IsDeleted = true;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null)
        {
            var entity = await _db.DivisionPages.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Division page not found.");

            entity.IsPublished = isPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
