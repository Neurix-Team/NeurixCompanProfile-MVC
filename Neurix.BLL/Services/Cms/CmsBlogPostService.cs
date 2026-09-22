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
    public class CmsBlogPostService : ICmsBlogPostService
    {
        private readonly CmsDbContext _db;

        public CmsBlogPostService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsBlogPostSummaryDto>> GetBlogPostsByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false)
        {
            var query = _db.BlogPosts
                .Include(b => b.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(b => b.CompanyProfileId == companyProfileId.Value);
            }

            if (!includeUnpublished)
            {
                query = query.Where(b => b.IsPublished && b.CompanyProfile != null && b.CompanyProfile.IsPublished);
            }

            return await query
                .OrderByDescending(b => b.PublishedAtUtc ?? b.CreatedAtUtc)
                .Select(b => new CmsBlogPostSummaryDto
                {
                    Id = b.Id,
                    CompanyProfileId = b.CompanyProfileId,
                    CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                    Slug = b.Slug,
                    TitleEn = b.TitleEn,
                    TitleAr = b.TitleAr,
                    SummaryEn = b.SummaryEn,
                    SummaryAr = b.SummaryAr,
                    CoverImagePath = b.CoverImagePath,
                    Category = b.Category,
                    AuthorNameEn = b.AuthorNameEn,
                    ReadTimeMinutes = b.ReadTimeMinutes,
                    PublishedAtUtc = b.PublishedAtUtc,
                    IsFeatured = b.IsFeatured,
                    IsPublished = b.IsPublished,
                    CreatedAtUtc = b.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CmsBlogPostSummaryDto>> GetPublishedPostsByCompanySlugAsync(string companySlug, int? limit = null)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsBlogPostSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            var query = _db.BlogPosts
                .Include(b => b.CompanyProfile)
                .AsNoTracking()
                .Where(b => b.IsPublished && b.CompanyProfile != null && b.CompanyProfile.IsPublished && b.CompanyProfile.Slug == normalized)
                .OrderByDescending(b => b.PublishedAtUtc ?? b.CreatedAtUtc);

            if (limit.HasValue && limit.Value > 0)
            {
                return await query.Take(limit.Value).Select(b => new CmsBlogPostSummaryDto
                {
                    Id = b.Id,
                    CompanyProfileId = b.CompanyProfileId,
                    CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                    Slug = b.Slug,
                    TitleEn = b.TitleEn,
                    TitleAr = b.TitleAr,
                    SummaryEn = b.SummaryEn,
                    SummaryAr = b.SummaryAr,
                    CoverImagePath = b.CoverImagePath,
                    Category = b.Category,
                    AuthorNameEn = b.AuthorNameEn,
                    ReadTimeMinutes = b.ReadTimeMinutes,
                    PublishedAtUtc = b.PublishedAtUtc,
                    IsFeatured = b.IsFeatured,
                    IsPublished = b.IsPublished,
                    CreatedAtUtc = b.CreatedAtUtc
                }).ToListAsync();
            }

            return await query.Select(b => new CmsBlogPostSummaryDto
            {
                Id = b.Id,
                CompanyProfileId = b.CompanyProfileId,
                CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                Slug = b.Slug,
                TitleEn = b.TitleEn,
                TitleAr = b.TitleAr,
                SummaryEn = b.SummaryEn,
                SummaryAr = b.SummaryAr,
                CoverImagePath = b.CoverImagePath,
                Category = b.Category,
                AuthorNameEn = b.AuthorNameEn,
                ReadTimeMinutes = b.ReadTimeMinutes,
                PublishedAtUtc = b.PublishedAtUtc,
                IsFeatured = b.IsFeatured,
                IsPublished = b.IsPublished,
                CreatedAtUtc = b.CreatedAtUtc
            }).ToListAsync();
        }

        public async Task<IReadOnlyList<CmsBlogPostSummaryDto>> GetFeaturedPostsByCompanySlugAsync(string companySlug, int? limit = null)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsBlogPostSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            var query = _db.BlogPosts
                .Include(b => b.CompanyProfile)
                .AsNoTracking()
                .Where(b => b.IsPublished && b.IsFeatured && b.CompanyProfile != null && b.CompanyProfile.IsPublished && b.CompanyProfile.Slug == normalized)
                .OrderByDescending(b => b.PublishedAtUtc ?? b.CreatedAtUtc);

            if (limit.HasValue && limit.Value > 0)
            {
                return await query.Take(limit.Value).Select(b => new CmsBlogPostSummaryDto
                {
                    Id = b.Id,
                    CompanyProfileId = b.CompanyProfileId,
                    CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                    Slug = b.Slug,
                    TitleEn = b.TitleEn,
                    TitleAr = b.TitleAr,
                    SummaryEn = b.SummaryEn,
                    SummaryAr = b.SummaryAr,
                    CoverImagePath = b.CoverImagePath,
                    Category = b.Category,
                    AuthorNameEn = b.AuthorNameEn,
                    ReadTimeMinutes = b.ReadTimeMinutes,
                    PublishedAtUtc = b.PublishedAtUtc,
                    IsFeatured = b.IsFeatured,
                    IsPublished = b.IsPublished,
                    CreatedAtUtc = b.CreatedAtUtc
                }).ToListAsync();
            }

            return await query.Select(b => new CmsBlogPostSummaryDto
            {
                Id = b.Id,
                CompanyProfileId = b.CompanyProfileId,
                CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                Slug = b.Slug,
                TitleEn = b.TitleEn,
                TitleAr = b.TitleAr,
                SummaryEn = b.SummaryEn,
                SummaryAr = b.SummaryAr,
                CoverImagePath = b.CoverImagePath,
                Category = b.Category,
                AuthorNameEn = b.AuthorNameEn,
                ReadTimeMinutes = b.ReadTimeMinutes,
                PublishedAtUtc = b.PublishedAtUtc,
                IsFeatured = b.IsFeatured,
                IsPublished = b.IsPublished,
                CreatedAtUtc = b.CreatedAtUtc
            }).ToListAsync();
        }

        public async Task<CmsBlogPostDetailDto?> GetByIdAsync(Guid id)
        {
            var b = await _db.BlogPosts
                .Include(p => p.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (b == null) return null;

            return new CmsBlogPostDetailDto
            {
                Id = b.Id,
                CompanyProfileId = b.CompanyProfileId,
                CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                CompanySlug = b.CompanyProfile != null ? b.CompanyProfile.Slug : string.Empty,
                Slug = b.Slug,
                TitleEn = b.TitleEn,
                TitleAr = b.TitleAr,
                SummaryEn = b.SummaryEn,
                SummaryAr = b.SummaryAr,
                BodyEn = b.BodyEn,
                BodyAr = b.BodyAr,
                CoverImagePath = b.CoverImagePath,
                Category = b.Category,
                AuthorNameEn = b.AuthorNameEn,
                AuthorNameAr = b.AuthorNameAr,
                Tags = b.Tags,
                ReadTimeMinutes = b.ReadTimeMinutes,
                PublishedAtUtc = b.PublishedAtUtc,
                IsFeatured = b.IsFeatured,
                IsPublished = b.IsPublished,
                CreatedAtUtc = b.CreatedAtUtc,
                UpdatedAtUtc = b.UpdatedAtUtc
            };
        }

        public async Task<CmsBlogPostDetailDto?> GetBySlugAsync(string companySlug, string postSlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug) || string.IsNullOrWhiteSpace(postSlug))
                return null;

            var normalizedCompany = companySlug.Trim().ToLowerInvariant();
            var normalizedPost = postSlug.Trim().ToLowerInvariant();

            var b = await _db.BlogPosts
                .Include(p => p.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IsPublished &&
                                          p.CompanyProfile != null &&
                                          p.CompanyProfile.IsPublished &&
                                          p.CompanyProfile.Slug == normalizedCompany &&
                                          p.Slug == normalizedPost);

            if (b == null) return null;

            return new CmsBlogPostDetailDto
            {
                Id = b.Id,
                CompanyProfileId = b.CompanyProfileId,
                CompanyNameEn = b.CompanyProfile != null ? b.CompanyProfile.NameEn : string.Empty,
                CompanySlug = b.CompanyProfile != null ? b.CompanyProfile.Slug : string.Empty,
                Slug = b.Slug,
                TitleEn = b.TitleEn,
                TitleAr = b.TitleAr,
                SummaryEn = b.SummaryEn,
                SummaryAr = b.SummaryAr,
                BodyEn = b.BodyEn,
                BodyAr = b.BodyAr,
                CoverImagePath = b.CoverImagePath,
                Category = b.Category,
                AuthorNameEn = b.AuthorNameEn,
                AuthorNameAr = b.AuthorNameAr,
                Tags = b.Tags,
                ReadTimeMinutes = b.ReadTimeMinutes,
                PublishedAtUtc = b.PublishedAtUtc,
                IsFeatured = b.IsFeatured,
                IsPublished = b.IsPublished,
                CreatedAtUtc = b.CreatedAtUtc,
                UpdatedAtUtc = b.UpdatedAtUtc
            };
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsBlogPostUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult<Guid>.Fail("Blog post data is required.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");

            var slug = dto.Slug.Trim().ToLowerInvariant();
            var slugExists = await _db.BlogPosts.AnyAsync(b => b.CompanyProfileId == dto.CompanyProfileId && b.Slug == slug);
            if (slugExists)
                return ServiceResult<Guid>.Fail($"A post with slug '{slug}' already exists for this brand.");

            var entity = new CmsBlogPost
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Slug = slug,
                TitleEn = dto.TitleEn.Trim(),
                TitleAr = dto.TitleAr.Trim(),
                SummaryEn = dto.SummaryEn?.Trim(),
                SummaryAr = dto.SummaryAr?.Trim(),
                BodyEn = dto.BodyEn?.Trim(),
                BodyAr = dto.BodyAr?.Trim(),
                CoverImagePath = dto.CoverImagePath?.Trim(),
                Category = dto.Category?.Trim(),
                AuthorNameEn = dto.AuthorNameEn?.Trim(),
                AuthorNameAr = dto.AuthorNameAr?.Trim(),
                Tags = dto.Tags?.Trim(),
                ReadTimeMinutes = dto.ReadTimeMinutes > 0 ? dto.ReadTimeMinutes : 5,
                PublishedAtUtc = dto.PublishedAtUtc ?? (dto.IsPublished ? DateTime.UtcNow : null),
                IsFeatured = dto.IsFeatured,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.BlogPosts.Add(entity);
            await _db.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(entity.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsBlogPostUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult.Fail("Blog post data is required.");

            var entity = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Blog post not found.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult.Fail("Selected company profile does not exist.");

            var slug = dto.Slug.Trim().ToLowerInvariant();
            var slugExists = await _db.BlogPosts.AnyAsync(b => b.Id != id && b.CompanyProfileId == dto.CompanyProfileId && b.Slug == slug);
            if (slugExists)
                return ServiceResult.Fail($"A post with slug '{slug}' already exists for this brand.");

            entity.CompanyProfileId = dto.CompanyProfileId;
            entity.Slug = slug;
            entity.TitleEn = dto.TitleEn.Trim();
            entity.TitleAr = dto.TitleAr.Trim();
            entity.SummaryEn = dto.SummaryEn?.Trim();
            entity.SummaryAr = dto.SummaryAr?.Trim();
            entity.BodyEn = dto.BodyEn?.Trim();
            entity.BodyAr = dto.BodyAr?.Trim();
            entity.CoverImagePath = dto.CoverImagePath?.Trim();
            entity.Category = dto.Category?.Trim();
            entity.AuthorNameEn = dto.AuthorNameEn?.Trim();
            entity.AuthorNameAr = dto.AuthorNameAr?.Trim();
            entity.Tags = dto.Tags?.Trim();
            entity.ReadTimeMinutes = dto.ReadTimeMinutes > 0 ? dto.ReadTimeMinutes : 5;
            entity.PublishedAtUtc = dto.PublishedAtUtc ?? (dto.IsPublished && entity.PublishedAtUtc == null ? DateTime.UtcNow : entity.PublishedAtUtc);
            entity.IsFeatured = dto.IsFeatured;
            entity.IsPublished = dto.IsPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null)
        {
            var entity = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Blog post not found.");

            entity.IsDeleted = true;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null)
        {
            var entity = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Blog post not found.");

            entity.IsPublished = isPublished;
            if (isPublished && entity.PublishedAtUtc == null)
            {
                entity.PublishedAtUtc = DateTime.UtcNow;
            }
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
