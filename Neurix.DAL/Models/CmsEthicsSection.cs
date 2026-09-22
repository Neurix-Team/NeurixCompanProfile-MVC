using System;

namespace Neurix.DAL.Models
{
    public class CmsEthicsSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string BadgeEn { get; set; } = "Operational Strategy";
        public string BadgeAr { get; set; } = "الاستراتيجية التشغيلية";

        public string TitlePrefixEn { get; set; } = "Our Vision & ";
        public string TitleHighlightEn { get; set; } = "Implementation Framework";
        public string TitlePrefixAr { get; set; } = "رؤيتنا و ";
        public string TitleHighlightAr { get; set; } = "إطار عمل التنفيذ";

        public string DescriptionEn { get; set; } = "Neurix AI serves as the technological engine of the ecosystem, transforming scientific research, AI-driven tools, and technical infrastructure into practical solutions for education, healthcare, government, academia, and professional communities.";
        public string DescriptionAr { get; set; } = "تعمل نيوركس كمحرك تقني للمنظومة، حيث تقوم بتحويل البحث العلمي والذكاء الاصطناعي والبنية التحتية إلى حلول عملية في مجالات التعليم، الرعاية الصحية، الحكومات، الأكاديميات، والمجتمعات المهنية.";

        // Top Overlapping Image Card (e.g. Software Setup)
        public string? TopImagePath { get; set; } = "/images/Software.png";
        public string? TopImageAltEn { get; set; } = "Software Office Desk Setup";
        public string? TopImageAltAr { get; set; } = "بيئة تطوير البرمجيات";

        // Bottom Overlapping Image Card (e.g. Research Lab)
        public string? BottomImagePath { get; set; } = "/images/Research.png";
        public string? BottomImageAltEn { get; set; } = "Pristine Laboratory Setup";
        public string? BottomImageAltAr { get; set; } = "مختبر الأبحاث العلمية";

        // Half Background Texture Image
        public string? BackgroundImagePath { get; set; } = "/images/Infrastructure.png";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
