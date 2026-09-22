using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a team member, executive, or key contributor for a brand profile.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsTeamMember : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string? BioEn { get; set; }
        public string? BioAr { get; set; }

        public string? PhotoPath { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? Email { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
