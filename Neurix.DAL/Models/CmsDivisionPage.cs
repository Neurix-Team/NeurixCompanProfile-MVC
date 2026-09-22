using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents CMS-managed content blocks for specific division pages (e.g., "labs", "technology", "hq", "plus", "club").
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsDivisionPage : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string Slug { get; set; } = string.Empty; // "labs", "technology", "hq", "plus", "club"

        public string HeroTitleEn { get; set; } = string.Empty;
        public string HeroTitleAr { get; set; } = string.Empty;

        public string? HeroSubtitleEn { get; set; }
        public string? HeroSubtitleAr { get; set; }

        public string? MissionEn { get; set; }
        public string? MissionAr { get; set; }

        public string? ContentJson { get; set; } // JSON serialized key capability items or sections
        public string? CoverImagePath { get; set; }

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
