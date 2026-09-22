using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// One card in the homepage "AI &amp; Engineering" band (#services).
    /// </summary>
    public class CmsAiEngineeringItem : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string IconName { get; set; } = "code";

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
