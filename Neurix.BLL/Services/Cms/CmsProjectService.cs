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
    public class CmsProjectService : ICmsProjectService
    {
        private readonly CmsDbContext _db;

        public CmsProjectService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsProjectSummaryDto>> GetProjectsByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false)
        {
            var query = _db.Projects
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
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CompletedAtUtc ?? p.CreatedAtUtc)
                .Select(p => new CmsProjectSummaryDto
                {
                    Id = p.Id,
                    CompanyProfileId = p.CompanyProfileId,
                    CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                    Slug = p.Slug,
                    TitleEn = p.TitleEn,
                    TitleAr = p.TitleAr,
                    SummaryEn = p.SummaryEn,
                    SummaryAr = p.SummaryAr,
                    CoverImagePath = p.CoverImagePath,
                    Category = p.Category,
                    ClientName = p.ClientName,
                    TechnologiesUsed = p.TechnologiesUsed,
                    DisplayOrder = p.DisplayOrder,
                    IsFeatured = p.IsFeatured,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CmsProjectSummaryDto>> GetPublishedProjectsByCompanySlugAsync(string companySlug, int? limit = null)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsProjectSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            var query = _db.Projects
                .Include(p => p.CompanyProfile)
                .AsNoTracking()
                .Where(p => p.IsPublished && p.CompanyProfile != null && p.CompanyProfile.IsPublished && p.CompanyProfile.Slug == normalized)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CompletedAtUtc ?? p.CreatedAtUtc);

            if (limit.HasValue && limit.Value > 0)
            {
                return await query.Take(limit.Value).Select(p => new CmsProjectSummaryDto
                {
                    Id = p.Id,
                    CompanyProfileId = p.CompanyProfileId,
                    CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                    Slug = p.Slug,
                    TitleEn = p.TitleEn,
                    TitleAr = p.TitleAr,
                    SummaryEn = p.SummaryEn,
                    SummaryAr = p.SummaryAr,
                    CoverImagePath = p.CoverImagePath,
                    Category = p.Category,
                    ClientName = p.ClientName,
                    TechnologiesUsed = p.TechnologiesUsed,
                    DisplayOrder = p.DisplayOrder,
                    IsFeatured = p.IsFeatured,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc
                }).ToListAsync();
            }

            return await query.Select(p => new CmsProjectSummaryDto
            {
                Id = p.Id,
                CompanyProfileId = p.CompanyProfileId,
                CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                Slug = p.Slug,
                TitleEn = p.TitleEn,
                TitleAr = p.TitleAr,
                SummaryEn = p.SummaryEn,
                SummaryAr = p.SummaryAr,
                CoverImagePath = p.CoverImagePath,
                Category = p.Category,
                ClientName = p.ClientName,
                TechnologiesUsed = p.TechnologiesUsed,
                DisplayOrder = p.DisplayOrder,
                IsFeatured = p.IsFeatured,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc
            }).ToListAsync();
        }

        public async Task<IReadOnlyList<CmsProjectSummaryDto>> GetFeaturedProjectsByCompanySlugAsync(string companySlug, int? limit = null)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsProjectSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            var query = _db.Projects
                .Include(p => p.CompanyProfile)
                .AsNoTracking()
                .Where(p => p.IsPublished && p.IsFeatured && p.CompanyProfile != null && p.CompanyProfile.IsPublished && p.CompanyProfile.Slug == normalized)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CompletedAtUtc ?? p.CreatedAtUtc);

            if (limit.HasValue && limit.Value > 0)
            {
                return await query.Take(limit.Value).Select(p => new CmsProjectSummaryDto
                {
                    Id = p.Id,
                    CompanyProfileId = p.CompanyProfileId,
                    CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                    Slug = p.Slug,
                    TitleEn = p.TitleEn,
                    TitleAr = p.TitleAr,
                    SummaryEn = p.SummaryEn,
                    SummaryAr = p.SummaryAr,
                    CoverImagePath = p.CoverImagePath,
                    Category = p.Category,
                    ClientName = p.ClientName,
                    TechnologiesUsed = p.TechnologiesUsed,
                    DisplayOrder = p.DisplayOrder,
                    IsFeatured = p.IsFeatured,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc
                }).ToListAsync();
            }

            return await query.Select(p => new CmsProjectSummaryDto
            {
                Id = p.Id,
                CompanyProfileId = p.CompanyProfileId,
                CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                Slug = p.Slug,
                TitleEn = p.TitleEn,
                TitleAr = p.TitleAr,
                SummaryEn = p.SummaryEn,
                SummaryAr = p.SummaryAr,
                CoverImagePath = p.CoverImagePath,
                Category = p.Category,
                ClientName = p.ClientName,
                TechnologiesUsed = p.TechnologiesUsed,
                DisplayOrder = p.DisplayOrder,
                IsFeatured = p.IsFeatured,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc
            }).ToListAsync();
        }

        public async Task<CmsProjectDetailDto?> GetByIdAsync(Guid id)
        {
            var p = await _db.Projects
                .Include(item => item.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (p == null) return null;

            return new CmsProjectDetailDto
            {
                Id = p.Id,
                CompanyProfileId = p.CompanyProfileId,
                CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                CompanySlug = p.CompanyProfile != null ? p.CompanyProfile.Slug : string.Empty,
                Slug = p.Slug,
                TitleEn = p.TitleEn,
                TitleAr = p.TitleAr,
                SummaryEn = p.SummaryEn,
                SummaryAr = p.SummaryAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                CoverImagePath = p.CoverImagePath,
                Category = p.Category,
                ClientName = p.ClientName,
                TechnologiesUsed = p.TechnologiesUsed,
                ProjectUrl = p.ProjectUrl,
                GithubUrl = p.GithubUrl,
                CompletedAtUtc = p.CompletedAtUtc,
                DisplayOrder = p.DisplayOrder,
                IsFeatured = p.IsFeatured,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc
            };
        }

        public async Task<CmsProjectDetailDto?> GetBySlugAsync(string companySlug, string projectSlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug) || string.IsNullOrWhiteSpace(projectSlug))
                return null;

            var normalizedCompany = companySlug.Trim().ToLowerInvariant();
            var normalizedSlug = projectSlug.Trim().ToLowerInvariant();

            var p = await _db.Projects
                .Include(item => item.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IsPublished &&
                                             item.CompanyProfile != null &&
                                             item.CompanyProfile.IsPublished &&
                                             item.CompanyProfile.Slug == normalizedCompany &&
                                             item.Slug == normalizedSlug);

            if (p == null) return null;

            return new CmsProjectDetailDto
            {
                Id = p.Id,
                CompanyProfileId = p.CompanyProfileId,
                CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                CompanySlug = p.CompanyProfile != null ? p.CompanyProfile.Slug : string.Empty,
                Slug = p.Slug,
                TitleEn = p.TitleEn,
                TitleAr = p.TitleAr,
                SummaryEn = p.SummaryEn,
                SummaryAr = p.SummaryAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                CoverImagePath = p.CoverImagePath,
                Category = p.Category,
                ClientName = p.ClientName,
                TechnologiesUsed = p.TechnologiesUsed,
                ProjectUrl = p.ProjectUrl,
                GithubUrl = p.GithubUrl,
                CompletedAtUtc = p.CompletedAtUtc,
                DisplayOrder = p.DisplayOrder,
                IsFeatured = p.IsFeatured,
                IsPublished = p.IsPublished,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc
            };
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsProjectUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult<Guid>.Fail("Project data is required.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");

            var slug = dto.Slug.Trim().ToLowerInvariant();
            var slugExists = await _db.Projects.AnyAsync(p => p.CompanyProfileId == dto.CompanyProfileId && p.Slug == slug);
            if (slugExists)
                return ServiceResult<Guid>.Fail($"A project with slug '{slug}' already exists for this brand.");

            var entity = new CmsProject
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Slug = slug,
                TitleEn = dto.TitleEn.Trim(),
                TitleAr = dto.TitleAr.Trim(),
                SummaryEn = dto.SummaryEn?.Trim(),
                SummaryAr = dto.SummaryAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                DescriptionAr = dto.DescriptionAr?.Trim(),
                CoverImagePath = dto.CoverImagePath?.Trim(),
                Category = dto.Category?.Trim(),
                ClientName = dto.ClientName?.Trim(),
                TechnologiesUsed = dto.TechnologiesUsed?.Trim(),
                ProjectUrl = dto.ProjectUrl?.Trim(),
                GithubUrl = dto.GithubUrl?.Trim(),
                CompletedAtUtc = dto.CompletedAtUtc,
                DisplayOrder = dto.DisplayOrder,
                IsFeatured = dto.IsFeatured,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.Projects.Add(entity);
            await _db.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(entity.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsProjectUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult.Fail("Project data is required.");

            var entity = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Project not found.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult.Fail("Selected company profile does not exist.");

            var slug = dto.Slug.Trim().ToLowerInvariant();
            var slugExists = await _db.Projects.AnyAsync(p => p.Id != id && p.CompanyProfileId == dto.CompanyProfileId && p.Slug == slug);
            if (slugExists)
                return ServiceResult.Fail($"A project with slug '{slug}' already exists for this brand.");

            entity.CompanyProfileId = dto.CompanyProfileId;
            entity.Slug = slug;
            entity.TitleEn = dto.TitleEn.Trim();
            entity.TitleAr = dto.TitleAr.Trim();
            entity.SummaryEn = dto.SummaryEn?.Trim();
            entity.SummaryAr = dto.SummaryAr?.Trim();
            entity.DescriptionEn = dto.DescriptionEn?.Trim();
            entity.DescriptionAr = dto.DescriptionAr?.Trim();
            entity.CoverImagePath = dto.CoverImagePath?.Trim();
            entity.Category = dto.Category?.Trim();
            entity.ClientName = dto.ClientName?.Trim();
            entity.TechnologiesUsed = dto.TechnologiesUsed?.Trim();
            entity.ProjectUrl = dto.ProjectUrl?.Trim();
            entity.GithubUrl = dto.GithubUrl?.Trim();
            entity.CompletedAtUtc = dto.CompletedAtUtc;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsFeatured = dto.IsFeatured;
            entity.IsPublished = dto.IsPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null)
        {
            var entity = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Project not found.");

            entity.IsDeleted = true;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null)
        {
            var entity = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Project not found.");

            entity.IsPublished = isPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
