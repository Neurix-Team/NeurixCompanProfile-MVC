using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents the dynamic Message to Pioneers section (#pioneers) on the homepage.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsPioneersSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        // Eyebrow / Badge
        public string BadgeEn { get; set; } = "For the Innovators";
        public string BadgeAr { get; set; } = "للمبتكرين";

        // Title parts (prefix + gradient highlight)
        public string TitlePrefixEn { get; set; } = "A Message to the ";
        public string TitleHighlightEn { get; set; } = "Pioneers of Innovation";

        public string TitlePrefixAr { get; set; } = "رسالة ";
        public string TitleHighlightAr { get; set; } = "لرواد الابتكار والأبداع";

        // Multiline / Multi-paragraph body text
        public string Paragraph1En { get; set; } = "To all programmers and visionaries: Your mission extends beyond writing code; you are shaping the future.";
        public string Paragraph1Ar { get; set; } = "مهمتكم تتجاوز كتابة الأكواد إلى صناعة المستقبل وتوجيه مساره.";

        public string Paragraph2En { get; set; } = "You hold the power to make technology a catalyst for progress rather than a tool for distraction. It is your responsibility to channel your skills to elevate humanity, not just advance systems. Let your creativity build a more conscious and human-centered world.";
        public string Paragraph2Ar { get; set; } = "التكنولوجيا بين أيديكم إما قوة للبناء والوعي أو أداة لتشتيت الأجيال. لذا، مسؤوليتكم هي توظيف مهاراتكم لخدمة الإنسان، وليس مجرد تطوير الأنظمة. اجعلوا من إبداعكم خطوة نحو عالم أكثر وعياً وإنسانية.";

        // Illustration image asset and alt texts
        public string? ImagePath { get; set; } = "/images/Infrastructure.png";
        public string? ImageAltEn { get; set; } = "Pioneers of Innovation";
        public string? ImageAltAr { get; set; } = "رواد الابتكار";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
