using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Header copy for the homepage "Our Ecosystem" divisions band (#divisions).
    /// The cards themselves live in <see cref="CmsDivisionItem"/>.
    /// </summary>
    public class CmsDivisionsSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string BadgeEn { get; set; } = "Our Ecosystem";
        public string BadgeAr { get; set; } = "منظومتنا";

        public string TitlePrefixEn { get; set; } = "Five subsidiaries. ";
        public string TitleHighlightEn { get; set; } = "One ecosystem.";
        public string TitlePrefixAr { get; set; } = "خمسة فروع. ";
        public string TitleHighlightAr { get; set; } = "منظومة واحدة.";

        public string DescriptionEn { get; set; } = "Each division is purpose-built to drive a specific domain of Neurix AI's broader mission.";
        public string DescriptionAr { get; set; } = "كل فرع مصمم لقيادة مجال محدد ضمن رسالة نيوركس AI الأشمل.";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
