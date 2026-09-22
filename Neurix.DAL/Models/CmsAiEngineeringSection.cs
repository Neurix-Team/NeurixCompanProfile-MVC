using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Header copy for the homepage "AI &amp; Engineering" core-services band (#services).
    /// The cards themselves live in <see cref="CmsAiEngineeringItem"/>.
    /// </summary>
    public class CmsAiEngineeringSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string BadgeEn { get; set; } = "Core Services";
        public string BadgeAr { get; set; } = "خدماتنا الأساسية";

        public string TitleEn { get; set; } = "AI & Engineering";
        public string TitleAr { get; set; } = "الذكاء الاصطناعي والهندسة";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
