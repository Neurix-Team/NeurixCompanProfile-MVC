using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a dynamic CMS content page (e.g., About Us, Privacy Policy, Terms of Service, Contact).
    /// Supports bilingual hero sections, mission/vision pairs, rich body text, and bottom CTA banners.
    /// </summary>
    public class CmsContentPage : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile CompanyProfile { get; set; } = null!;

        /// <summary>
        /// Unique page identifier under the company profile (e.g. "about", "privacy", "contact", "terms").
        /// </summary>
        public string Slug { get; set; } = "";

        // ── Page Titles & SEO Metadata ──
        public string TitleEn { get; set; } = "";
        public string TitleAr { get; set; } = "";
        public string? MetaDescriptionEn { get; set; }
        public string? MetaDescriptionAr { get; set; }

        // ── Hero Section ──
        public string? HeroBadgeEn { get; set; }
        public string? HeroBadgeAr { get; set; }
        public string? HeroTitlePrefixEn { get; set; }
        public string? HeroTitlePrefixAr { get; set; }
        public string? HeroTitleHighlightEn { get; set; }
        public string? HeroTitleHighlightAr { get; set; }
        public string? HeroSubtitleEn { get; set; }
        public string? HeroSubtitleAr { get; set; }

        // ── Mission & Vision (Used in About page) ──
        public string? MissionTitleEn { get; set; }
        public string? MissionTitleAr { get; set; }
        public string? MissionTextEn { get; set; }
        public string? MissionTextAr { get; set; }

        public string? VisionTitleEn { get; set; }
        public string? VisionTitleAr { get; set; }
        public string? VisionTextEn { get; set; }
        public string? VisionTextAr { get; set; }

        // ── Rich Content Body (HTML / Markdown for Privacy, Terms, etc.) ──
        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        // ── Bottom CTA Banner ──
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

        // ── Extensibility JSON ──
        public string? ExtraDataJson { get; set; }

        // ── State ──
        public bool IsPublished { get; set; } = true;

        // ── IAuditable ──
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
