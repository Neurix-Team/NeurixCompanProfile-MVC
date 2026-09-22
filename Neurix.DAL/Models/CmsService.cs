using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a service, capability, or strategic solution offered by a brand profile.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsService : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;

        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? IconName { get; set; }
        public string? ImagePath { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
