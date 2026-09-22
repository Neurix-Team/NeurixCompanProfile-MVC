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
    public class CmsTeamMemberService : ICmsTeamMemberService
    {
        private readonly CmsDbContext _db;

        public CmsTeamMemberService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsTeamMemberSummaryDto>> GetTeamMembersByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false)
        {
            var query = _db.TeamMembers
                .Include(m => m.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(m => m.CompanyProfileId == companyProfileId.Value);
            }

            if (!includeUnpublished)
            {
                query = query.Where(m => m.IsPublished && m.CompanyProfile != null && m.CompanyProfile.IsPublished);
            }

            return await query
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.NameEn)
                .Select(m => new CmsTeamMemberSummaryDto
                {
                    Id = m.Id,
                    CompanyProfileId = m.CompanyProfileId,
                    CompanyNameEn = m.CompanyProfile != null ? m.CompanyProfile.NameEn : string.Empty,
                    NameEn = m.NameEn,
                    NameAr = m.NameAr,
                    TitleEn = m.TitleEn,
                    TitleAr = m.TitleAr,
                    BioEn = m.BioEn,
                    BioAr = m.BioAr,
                    PhotoPath = m.PhotoPath,
                    LinkedInUrl = m.LinkedInUrl,
                    Email = m.Email,
                    DisplayOrder = m.DisplayOrder,
                    IsPublished = m.IsPublished,
                    CreatedAtUtc = m.CreatedAtUtc,
                    UpdatedAtUtc = m.UpdatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CmsTeamMemberSummaryDto>> GetPublishedMembersByCompanySlugAsync(string companySlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsTeamMemberSummaryDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            return await _db.TeamMembers
                .Include(m => m.CompanyProfile)
                .AsNoTracking()
                .Where(m => m.IsPublished && m.CompanyProfile != null && m.CompanyProfile.IsPublished && m.CompanyProfile.Slug == normalized)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.NameEn)
                .Select(m => new CmsTeamMemberSummaryDto
                {
                    Id = m.Id,
                    CompanyProfileId = m.CompanyProfileId,
                    CompanyNameEn = m.CompanyProfile != null ? m.CompanyProfile.NameEn : string.Empty,
                    NameEn = m.NameEn,
                    NameAr = m.NameAr,
                    TitleEn = m.TitleEn,
                    TitleAr = m.TitleAr,
                    BioEn = m.BioEn,
                    BioAr = m.BioAr,
                    PhotoPath = m.PhotoPath,
                    LinkedInUrl = m.LinkedInUrl,
                    Email = m.Email,
                    DisplayOrder = m.DisplayOrder,
                    IsPublished = m.IsPublished,
                    CreatedAtUtc = m.CreatedAtUtc,
                    UpdatedAtUtc = m.UpdatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<CmsTeamMemberDetailDto?> GetByIdAsync(Guid id)
        {
            var member = await _db.TeamMembers
                .Include(m => m.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null) return null;

            return new CmsTeamMemberDetailDto
            {
                Id = member.Id,
                CompanyProfileId = member.CompanyProfileId,
                CompanyNameEn = member.CompanyProfile != null ? member.CompanyProfile.NameEn : string.Empty,
                CompanySlug = member.CompanyProfile != null ? member.CompanyProfile.Slug : string.Empty,
                NameEn = member.NameEn,
                NameAr = member.NameAr,
                TitleEn = member.TitleEn,
                TitleAr = member.TitleAr,
                BioEn = member.BioEn,
                BioAr = member.BioAr,
                PhotoPath = member.PhotoPath,
                LinkedInUrl = member.LinkedInUrl,
                Email = member.Email,
                DisplayOrder = member.DisplayOrder,
                IsPublished = member.IsPublished,
                CreatedAtUtc = member.CreatedAtUtc,
                UpdatedAtUtc = member.UpdatedAtUtc
            };
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsTeamMemberUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult<Guid>.Fail("Team member data is required.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");

            var entity = new CmsTeamMember
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                NameEn = dto.NameEn.Trim(),
                NameAr = dto.NameAr.Trim(),
                TitleEn = dto.TitleEn.Trim(),
                TitleAr = dto.TitleAr.Trim(),
                BioEn = dto.BioEn?.Trim(),
                BioAr = dto.BioAr?.Trim(),
                PhotoPath = dto.PhotoPath?.Trim(),
                LinkedInUrl = dto.LinkedInUrl?.Trim(),
                Email = dto.Email?.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId
            };

            _db.TeamMembers.Add(entity);
            await _db.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(entity.Id);
        }

        public async Task<ServiceResult> UpdateAsync(Guid id, CmsTeamMemberUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult.Fail("Team member data is required.");

            var entity = await _db.TeamMembers.FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Team member not found.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult.Fail("Selected company profile does not exist.");

            entity.CompanyProfileId = dto.CompanyProfileId;
            entity.NameEn = dto.NameEn.Trim();
            entity.NameAr = dto.NameAr.Trim();
            entity.TitleEn = dto.TitleEn.Trim();
            entity.TitleAr = dto.TitleAr.Trim();
            entity.BioEn = dto.BioEn?.Trim();
            entity.BioAr = dto.BioAr?.Trim();
            entity.PhotoPath = dto.PhotoPath?.Trim();
            entity.LinkedInUrl = dto.LinkedInUrl?.Trim();
            entity.Email = dto.Email?.Trim();
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsPublished = dto.IsPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null)
        {
            var entity = await _db.TeamMembers.FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Team member not found.");

            entity.IsDeleted = true;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null)
        {
            var entity = await _db.TeamMembers.FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Team member not found.");

            entity.IsPublished = isPublished;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
