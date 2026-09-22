using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsContentPageService : ICmsContentPageService
    {
        // Body is rendered with @Html.Raw on the public site, so it must be sanitized to an
        // allowlist on the way in — an Admin-only editor does not make submitted HTML safe
        // by itself. The default allowlist keeps common formatting/links/images and strips
        // script, event handlers, and javascript: URLs.
        private static readonly HtmlSanitizer BodySanitizer = new();

        private readonly CmsDbContext _db;

        public CmsContentPageService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<CmsContentPageDetailDto?> GetBySlugAsync(string companySlug, string pageSlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug) || string.IsNullOrWhiteSpace(pageSlug))
            {
                return null;
            }

            var normalizedCompany = companySlug.Trim().ToLowerInvariant();
            var normalizedPage = pageSlug.Trim().ToLowerInvariant();

            var page = await _db.ContentPages
                .Include(p => p.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IsPublished &&
                                          p.CompanyProfile != null &&
                                          p.CompanyProfile.IsPublished &&
                                          p.CompanyProfile.Slug == normalizedCompany &&
                                          p.Slug == normalizedPage);

            return page == null ? null : ToDetail(page);
        }

        public async Task<IReadOnlyList<CmsContentPageSummaryDto>> GetAllByCompanyAsync(Guid? companyProfileId)
        {
            var query = _db.ContentPages
                .Include(p => p.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(p => p.CompanyProfileId == companyProfileId.Value);
            }

            return await query
                .OrderBy(p => p.Slug)
                .Select(p => new CmsContentPageSummaryDto
                {
                    Id = p.Id,
                    CompanyProfileId = p.CompanyProfileId,
                    CompanyNameEn = p.CompanyProfile != null ? p.CompanyProfile.NameEn : string.Empty,
                    Slug = p.Slug,
                    TitleEn = p.TitleEn,
                    TitleAr = p.TitleAr,
                    HasBody = p.BodyEn != null && p.BodyEn != string.Empty,
                    IsPublished = p.IsPublished,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<CmsContentPageDetailDto?> GetByIdAsync(Guid id)
        {
            var page = await _db.ContentPages
                .Include(p => p.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return page == null ? null : ToDetail(page);
        }

        public async Task<ServiceResult<Guid>> UpsertAsync(CmsContentPageUpsertDto dto, Guid? userId = null)
        {
            if (dto == null)
            {
                return ServiceResult<Guid>.Fail("Content page data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Slug))
            {
                return ServiceResult<Guid>.Fail("Page slug is required.");
            }

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
            {
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");
            }

            var slug = dto.Slug.Trim().ToLowerInvariant();

            // Match on (company, slug) rather than Id so a re-submitted create cannot
            // violate the unique index — it updates the existing page instead.
            var entity = await _db.ContentPages
                .FirstOrDefaultAsync(p => p.CompanyProfileId == dto.CompanyProfileId && p.Slug == slug);

            var isNew = entity == null;
            if (isNew)
            {
                entity = new CmsContentPage
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = dto.CompanyProfileId,
                    Slug = slug,
                    CreatedByUserId = userId
                };
                _db.ContentPages.Add(entity);
            }

            Apply(entity!, dto);

            await _db.SaveChangesAsync();
            return ServiceResult<Guid>.Ok(entity!.Id);
        }

        // ── mapping ──────────────────────────────────────────────────────────────

        private static void Apply(CmsContentPage entity, CmsContentPageUpsertDto dto)
        {
            entity.TitleEn = dto.TitleEn.Trim();
            entity.TitleAr = dto.TitleAr.Trim();
            entity.MetaDescriptionEn = Clean(dto.MetaDescriptionEn);
            entity.MetaDescriptionAr = Clean(dto.MetaDescriptionAr);

            entity.HeroBadgeEn = Clean(dto.HeroBadgeEn);
            entity.HeroBadgeAr = Clean(dto.HeroBadgeAr);
            entity.HeroTitlePrefixEn = Clean(dto.HeroTitlePrefixEn);
            entity.HeroTitlePrefixAr = Clean(dto.HeroTitlePrefixAr);
            entity.HeroTitleHighlightEn = Clean(dto.HeroTitleHighlightEn);
            entity.HeroTitleHighlightAr = Clean(dto.HeroTitleHighlightAr);
            entity.HeroSubtitleEn = Clean(dto.HeroSubtitleEn);
            entity.HeroSubtitleAr = Clean(dto.HeroSubtitleAr);

            // Body keeps its whitespace: it is HTML authored in a textarea, sanitized below.
            entity.BodyEn = string.IsNullOrWhiteSpace(dto.BodyEn) ? null : BodySanitizer.Sanitize(dto.BodyEn);
            entity.BodyAr = string.IsNullOrWhiteSpace(dto.BodyAr) ? null : BodySanitizer.Sanitize(dto.BodyAr);

            entity.MissionTitleEn = Clean(dto.MissionTitleEn);
            entity.MissionTitleAr = Clean(dto.MissionTitleAr);
            entity.MissionTextEn = Clean(dto.MissionTextEn);
            entity.MissionTextAr = Clean(dto.MissionTextAr);
            entity.VisionTitleEn = Clean(dto.VisionTitleEn);
            entity.VisionTitleAr = Clean(dto.VisionTitleAr);
            entity.VisionTextEn = Clean(dto.VisionTextEn);
            entity.VisionTextAr = Clean(dto.VisionTextAr);

            entity.CtaBadgeEn = Clean(dto.CtaBadgeEn);
            entity.CtaBadgeAr = Clean(dto.CtaBadgeAr);
            entity.CtaTitleEn = Clean(dto.CtaTitleEn);
            entity.CtaTitleAr = Clean(dto.CtaTitleAr);
            entity.CtaSubtitleEn = Clean(dto.CtaSubtitleEn);
            entity.CtaSubtitleAr = Clean(dto.CtaSubtitleAr);
            entity.CtaButtonTextEn = Clean(dto.CtaButtonTextEn);
            entity.CtaButtonTextAr = Clean(dto.CtaButtonTextAr);
            entity.CtaButtonUrl = Clean(dto.CtaButtonUrl);
            entity.ContactEmail = Clean(dto.ContactEmail);

            entity.ExtraDataJson = Clean(dto.ExtraDataJson);
            entity.IsPublished = dto.IsPublished;
        }

        /// <summary>Trims, and turns blank input into null so the views fall back to defaults.</summary>
        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static CmsContentPageDetailDto ToDetail(CmsContentPage p) => new()
        {
            Id = p.Id,
            CompanyProfileId = p.CompanyProfileId,
            CompanyNameEn = p.CompanyProfile?.NameEn ?? string.Empty,
            CompanySlug = p.CompanyProfile?.Slug ?? string.Empty,
            Slug = p.Slug,
            TitleEn = p.TitleEn,
            TitleAr = p.TitleAr,
            MetaDescriptionEn = p.MetaDescriptionEn,
            MetaDescriptionAr = p.MetaDescriptionAr,
            HeroBadgeEn = p.HeroBadgeEn,
            HeroBadgeAr = p.HeroBadgeAr,
            HeroTitlePrefixEn = p.HeroTitlePrefixEn,
            HeroTitlePrefixAr = p.HeroTitlePrefixAr,
            HeroTitleHighlightEn = p.HeroTitleHighlightEn,
            HeroTitleHighlightAr = p.HeroTitleHighlightAr,
            HeroSubtitleEn = p.HeroSubtitleEn,
            HeroSubtitleAr = p.HeroSubtitleAr,
            BodyEn = p.BodyEn,
            BodyAr = p.BodyAr,
            MissionTitleEn = p.MissionTitleEn,
            MissionTitleAr = p.MissionTitleAr,
            MissionTextEn = p.MissionTextEn,
            MissionTextAr = p.MissionTextAr,
            VisionTitleEn = p.VisionTitleEn,
            VisionTitleAr = p.VisionTitleAr,
            VisionTextEn = p.VisionTextEn,
            VisionTextAr = p.VisionTextAr,
            CtaBadgeEn = p.CtaBadgeEn,
            CtaBadgeAr = p.CtaBadgeAr,
            CtaTitleEn = p.CtaTitleEn,
            CtaTitleAr = p.CtaTitleAr,
            CtaSubtitleEn = p.CtaSubtitleEn,
            CtaSubtitleAr = p.CtaSubtitleAr,
            CtaButtonTextEn = p.CtaButtonTextEn,
            CtaButtonTextAr = p.CtaButtonTextAr,
            CtaButtonUrl = p.CtaButtonUrl,
            ContactEmail = p.ContactEmail,
            ExtraDataJson = p.ExtraDataJson,
            IsPublished = p.IsPublished,
            CreatedAtUtc = p.CreatedAtUtc,
            UpdatedAtUtc = p.UpdatedAtUtc
        };
    }
}
