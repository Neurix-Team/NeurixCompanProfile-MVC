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
    public class CmsTestimonialService : ICmsTestimonialService
    {
        private readonly CmsDbContext _db;

        public CmsTestimonialService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsTestimonialSummaryDto>> GetTestimonialsByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false)
        {
            var query = _db.Testimonials
                .Include(t => t.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(t => t.CompanyProfileId == companyProfileId.Value);
            }

            if (!includeUnpublished)
            {
                query = query.Where(t => t.IsPublished && t.CompanyProfile != null && t.CompanyProfile.IsPublished);
            }

            return await query
                .OrderBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.CreatedAtUtc)
                .Select(t => new CmsTestimonialSummaryDto
                {
                    Id = t.Id,
                    CompanyProfileId = t.CompanyProfileId,
                    CompanyNameEn = t.CompanyProfile != null ? t.CompanyProfile.NameEn : string.Empty,
                    AuthorNameEn = t.AuthorNameEn,
                    AuthorNameAr = t.AuthorNameAr,
                    AuthorTitleEn = t.AuthorTitleEn,
                    AuthorTitleAr = t.AuthorTitleAr,
                    CompanyName = t.CompanyName,
                    AuthorPhotoPath = t.AuthorPhotoPath,
                    QuoteEn = t.QuoteEn,
                    QuoteAr = t.QuoteAr,
                    Rating = t.Rating,
                    DisplayOrder = t.DisplayOrder,
                    IsFeatured = t.IsFeatured,
                    IsPublished = t.IsPublished,
                    CreatedAtUtc = t.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CmsTestimonialSummaryDto>> GetPublishedTestimonialsByCompanySlugAsync(string companySlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsTestimonialSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            return await _db.Testimonials
                .Include(t => t.CompanyProfile)
                .AsNoTracking()
                .Where(t => t.IsPublished && t.CompanyProfile != null && t.CompanyProfile.IsPublished && t.CompanyProfile.Slug == normalized)
                .OrderBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.CreatedAtUtc)
                .Select(t => new CmsTestimonialSummaryDto
                {
                    Id = t.Id,
                    CompanyProfileId = t.CompanyProfileId,
                    CompanyNameEn = t.CompanyProfile != null ? t.CompanyProfile.NameEn : string.Empty,
                    AuthorNameEn = t.AuthorNameEn,
                    AuthorNameAr = t.AuthorNameAr,
                    AuthorTitleEn = t.AuthorTitleEn,
                    AuthorTitleAr = t.AuthorTitleAr,
                    CompanyName = t.CompanyName,
                    AuthorPhotoPath = t.AuthorPhotoPath,
                    QuoteEn = t.QuoteEn,
                    QuoteAr = t.QuoteAr,
                    Rating = t.Rating,
                    DisplayOrder = t.DisplayOrder,
                    IsFeatured = t.IsFeatured,
                    IsPublished = t.IsPublished,
                    CreatedAtUtc = t.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CmsTestimonialSummaryDto>> GetFeaturedTestimonialsByCompanySlugAsync(string companySlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsTestimonialSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            return await _db.Testimonials
                .Include(t => t.CompanyProfile)
                .AsNoTracking()
                .Where(t => t.IsPublished && t.IsFeatured && t.CompanyProfile != null && t.CompanyProfile.IsPublished && t.CompanyProfile.Slug == normalized)
                .OrderBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.CreatedAtUtc)
                .Select(t => new CmsTestimonialSummaryDto
                {
                    Id = t.Id,
                    CompanyProfileId = t.CompanyProfileId,
                    CompanyNameEn = t.CompanyProfile != null ? t.CompanyProfile.NameEn : string.Empty,
                    AuthorNameEn = t.AuthorNameEn,
                    AuthorNameAr = t.AuthorNameAr,
                    AuthorTitleEn = t.AuthorTitleEn,
                    AuthorTitleAr = t.AuthorTitleAr,
                    CompanyName = t.CompanyName,
                    AuthorPhotoPath = t.AuthorPhotoPath,
                    QuoteEn = t.QuoteEn,
                    QuoteAr = t.QuoteAr,
                    Rating = t.Rating,
                    DisplayOrder = t.DisplayOrder,
                    IsFeatured = t.IsFeatured,
                    IsPublished = t.IsPublished,
                    CreatedAtUtc = t.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<CmsTestimonialDetailDto?> GetByIdAsync(Guid id)
        {
            var t = await _db.Testimonials
                .Include(item => item.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (t == null) return null;

            return new CmsTestimonialDetailDto
            {
                Id = t.Id,
                CompanyProfileId = t.CompanyProfileId,
                CompanyNameEn = t.CompanyProfile != null ? t.CompanyProfile.NameEn : string.Empty,
                CompanySlug = t.CompanyProfile != null ? t.CompanyProfile.Slug : string.Empty,
                AuthorNameEn = t.AuthorNameEn,
                AuthorNameAr = t.AuthorNameAr,
                AuthorTitleEn = t.AuthorTitleEn,
                AuthorTitleAr = t.AuthorTitleAr,
                CompanyName = t.CompanyName,
                AuthorPhotoPath = t.AuthorPhotoPath,
                QuoteEn = t.QuoteEn,
                QuoteAr = t.QuoteAr,
                Rating = t.Rating,
                DisplayOrder = t.DisplayOrder,
                IsFeatured = t.IsFeatured,
                IsPublished = t.IsPublished,
                CreatedAtUtc = t.CreatedAtUtc,
                UpdatedAtUtc = t.UpdatedAtUtc
            };
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsTestimonialUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult<Guid>.Fail("Testimonial data is required.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");

            var entity = new CmsTestimonial
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                AuthorNameEn = dto.NameEn.Trim(),
                AuthorNameAr = dto.NameAr.Trim(),
                AuthorTitleEn = dto.TitleEn?.Trim(),
                AuthorTitleAr = dto.TitleAr?.Trim(),
                CompanyName = dto.CompanyName?.Trim(),
                AuthorPhotoPath = dto.AuthorPhotoPath?.Trim(),
                QuoteEn = dto.QuoteEn.Trim(),
                QuoteAr = dto.QuoteAr.Trim(),
                Rating = dto.Rating is >= 1 and <= 5 ? dto.Rating : 5,
                DisplayOrder = dto.DisplayOrder,
                IsFeatured = dto.IsFeatured,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.Testimonials.Add(entity);
            await _db.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(entity.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsTestimonialUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult.Fail("Testimonial data is required.");

            var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Testimonial not found.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult.Fail("Selected company profile does not exist.");

            entity.CompanyProfileId = dto.CompanyProfileId;
            entity.AuthorNameEn = dto.NameEn.Trim();
            entity.AuthorNameAr = dto.NameAr.Trim();
            entity.AuthorTitleEn = dto.TitleEn?.Trim();
            entity.AuthorTitleAr = dto.TitleAr?.Trim();
            entity.CompanyName = dto.CompanyName?.Trim();
            entity.AuthorPhotoPath = dto.AuthorPhotoPath?.Trim();
            entity.QuoteEn = dto.QuoteEn.Trim();
            entity.QuoteAr = dto.QuoteAr.Trim();
            entity.Rating = dto.Rating is >= 1 and <= 5 ? dto.Rating : 5;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsFeatured = dto.IsFeatured;
            entity.IsPublished = dto.IsPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null)
        {
            var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Testimonial not found.");

            entity.IsDeleted = true;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null)
        {
            var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Testimonial not found.");

            entity.IsPublished = isPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
