using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents the dynamic Human Vision section (#vision-human / #human-vision) on the homepage.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsHumanVisionSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        // Eyebrow / Badge
        public string BadgeEn { get; set; } = "Our Core Philosophy";
        public string BadgeAr { get; set; } = "فلسفتنا الأساسية";

        // Title parts (prefix + gradient highlight)
        public string TitlePrefixEn { get; set; } = "Neurix AI ";
        public string TitleHighlightEn { get; set; } = "Human Vision";

        public string TitlePrefixAr { get; set; } = "رؤية نيوركس AI ";
        public string TitleHighlightAr { get; set; } = "الإنسانية";

        // Multiline / Multi-paragraph body text
        public string Paragraph1En { get; set; } = "Technological advancement is a great achievement, but its misuse—such as gaming addiction and harmful online behaviors—has become a silent threat to our youth.";
        public string Paragraph1Ar { get; set; } = "يُعد التقدم التكنولوجي إنجازاً عظيماً، لكن سوء استخدامه—كإدمان الألعاب والسلوكيات الضارة—يحوله إلى تهديد صامت يشتت شبابنا.";

        public string Paragraph2En { get; set; } = "Protecting their minds and guiding them toward the purposeful use of technology is a shared responsibility. Neurix AI's mission is to empower young people to use technology positively, transforming their energy into a force for innovation and societal growth to build a digital future that uplifts humanity.";
        public string Paragraph2Ar { get; set; } = "حماية عقول الشباب وتوجيههم نحو الاستخدام الهادف للتكنولوجيا هي مسؤولية مشتركة. لذلك، تسعى 'نيوركس AI' إلى تمكين الشباب من استثمار التكنولوجيا بصورة إيجابية، وتحويل طاقاتهم إلى قوة للابتكار وتطوير المجتمع، لنبني معاً مستقبلاً رقمياً يرتقي بالإنسان.";

        // Illustration image asset and alt texts
        public string? ImagePath { get; set; } = "/images/Bringing Clarity to Complexity_1 2.png";
        public string? ImageAltEn { get; set; } = "Neurix AI Human Vision";
        public string? ImageAltAr { get; set; } = "رؤية نيوركس AI الإنسانية";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
