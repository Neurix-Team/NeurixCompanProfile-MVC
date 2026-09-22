using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    /// <summary>Row shape for the CMS list screen.</summary>
    public class CmsContentPageSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public bool HasBody { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>Everything the public views and the CMS edit form need.</summary>
    public class CmsContentPageDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string? MetaDescriptionEn { get; set; }
        public string? MetaDescriptionAr { get; set; }

        public string? HeroBadgeEn { get; set; }
        public string? HeroBadgeAr { get; set; }
        public string? HeroTitlePrefixEn { get; set; }
        public string? HeroTitlePrefixAr { get; set; }
        public string? HeroTitleHighlightEn { get; set; }
        public string? HeroTitleHighlightAr { get; set; }
        public string? HeroSubtitleEn { get; set; }
        public string? HeroSubtitleAr { get; set; }

        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        public string? MissionTitleEn { get; set; }
        public string? MissionTitleAr { get; set; }
        public string? MissionTextEn { get; set; }
        public string? MissionTextAr { get; set; }
        public string? VisionTitleEn { get; set; }
        public string? VisionTitleAr { get; set; }
        public string? VisionTextEn { get; set; }
        public string? VisionTextAr { get; set; }

        public string? CtaBadgeEn { get; set; }
        public string? CtaBadgeAr { get; set; }
        public string? CtaTitleEn { get; set; }
        public string? CtaTitleAr { get; set; }
        public string? CtaSubtitleEn { get; set; }
        public string? CtaSubtitleAr { get; set; }
        public string? CtaButtonTextEn { get; set; }
        public string? CtaButtonTextAr { get; set; }
        public string? CtaButtonUrl { get; set; }
        public string? ContactEmail { get; set; }

        public string? ExtraDataJson { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Create-or-update payload. Slug plus CompanyProfileId identify the row, matching the
    /// unique index, so the CMS never has to decide between insert and update.
    /// </summary>
    public class CmsContentPageUpsertDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "Page slug is required.")]
        [StringLength(100)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "English page title is required.")]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic page title is required.")]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MetaDescriptionEn { get; set; }

        [StringLength(500)]
        public string? MetaDescriptionAr { get; set; }

        [StringLength(150)]
        public string? HeroBadgeEn { get; set; }

        [StringLength(150)]
        public string? HeroBadgeAr { get; set; }

        [StringLength(300)]
        public string? HeroTitlePrefixEn { get; set; }

        [StringLength(300)]
        public string? HeroTitlePrefixAr { get; set; }

        [StringLength(300)]
        public string? HeroTitleHighlightEn { get; set; }

        [StringLength(300)]
        public string? HeroTitleHighlightAr { get; set; }

        [StringLength(2000)]
        public string? HeroSubtitleEn { get; set; }

        [StringLength(2000)]
        public string? HeroSubtitleAr { get; set; }

        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        [StringLength(200)]
        public string? MissionTitleEn { get; set; }

        [StringLength(200)]
        public string? MissionTitleAr { get; set; }

        [StringLength(4000)]
        public string? MissionTextEn { get; set; }

        [StringLength(4000)]
        public string? MissionTextAr { get; set; }

        [StringLength(200)]
        public string? VisionTitleEn { get; set; }

        [StringLength(200)]
        public string? VisionTitleAr { get; set; }

        [StringLength(4000)]
        public string? VisionTextEn { get; set; }

        [StringLength(4000)]
        public string? VisionTextAr { get; set; }

        [StringLength(150)]
        public string? CtaBadgeEn { get; set; }

        [StringLength(150)]
        public string? CtaBadgeAr { get; set; }

        [StringLength(300)]
        public string? CtaTitleEn { get; set; }

        [StringLength(300)]
        public string? CtaTitleAr { get; set; }

        [StringLength(1000)]
        public string? CtaSubtitleEn { get; set; }

        [StringLength(1000)]
        public string? CtaSubtitleAr { get; set; }

        [StringLength(150)]
        public string? CtaButtonTextEn { get; set; }

        [StringLength(150)]
        public string? CtaButtonTextAr { get; set; }

        [StringLength(500)]
        public string? CtaButtonUrl { get; set; }

        [StringLength(256)]
        public string? ContactEmail { get; set; }

        public string? ExtraDataJson { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
