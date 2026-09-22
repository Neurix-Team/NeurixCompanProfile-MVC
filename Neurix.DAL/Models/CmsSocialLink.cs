using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a social media or community link for a company brand profile.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsSocialLink : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string Platform { get; set; } = string.Empty; // e.g., "linkedin", "twitter", "facebook", "instagram", "youtube", "github"
        public string Url { get; set; } = string.Empty;
        public string? DisplayName { get; set; } // e.g., "LinkedIn", "X (Twitter)", "YouTube"
        public string? IconName { get; set; } // Lucide icon name, e.g., "linkedin", "twitter", "facebook"

        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
