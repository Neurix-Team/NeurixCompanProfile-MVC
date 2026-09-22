using System;

namespace Neurix.DAL.Models
{
    public class CmsPillarsSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string TitleEn { get; set; } = "Core Capabilities";
        public string TitleAr { get; set; } = "الركائز الاساسية";

        public string SubtitleEn { get; set; } = "The foundational pillars driving our intelligent digital ecosystems and enterprise solutions.";
        public string SubtitleAr { get; set; } = "الركائز الأساسية التي تقود منظوماتنا الرقمية الذكيّة وحلول المؤسسات.";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
