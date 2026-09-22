using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a public company/brand profile in the CMS (e.g., "Neurix AI", "Daleel").
    /// Serves as the top-level tenant/brand identifier for all CMS-managed content.
    /// </summary>
    public class CmsCompanyProfile : IAuditable
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;

        public string? LogoPath { get; set; }
        public string? FaviconPath { get; set; }
        public string? PrimaryColor { get; set; }
        public string? AccentColor { get; set; }

        public string? TaglineEn { get; set; }
        public string? TaglineAr { get; set; }

        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? AddressEn { get; set; }
        public string? AddressAr { get; set; }

        public string? Website { get; set; }

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
