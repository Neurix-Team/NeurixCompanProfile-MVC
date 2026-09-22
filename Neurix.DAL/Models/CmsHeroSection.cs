using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents the dynamic hero banner section (#hero) on the homepage.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsHeroSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        // Eyebrow / Badge
        public string BadgeEn { get; set; } = "Empowering the Future";
        public string BadgeAr { get; set; } = "تمكين المستقبل";

        // Headline parts (bilingual with gradient highlight)
        public string TitlePrefixEn { get; set; } = "Building Human-Centered ";
        public string TitleHighlightEn { get; set; } = "AI";
        public string TitleSuffixEn { get; set; } = " for Tomorrow";

        public string TitlePrefixAr { get; set; } = " ";
        public string TitleHighlightAr { get; set; } = "ذكاء اصطناعي";
        public string TitleSuffixAr { get; set; } = " محوره الإنسان، من أجل الغد";

        // Subtitle / Description
        public string SubtitleEn { get; set; } = "Neurix AI is building smarter digital experiences for tomorrow.";
        public string SubtitleAr { get; set; } = "نصنع في نيوركس AI تجارب رقمية أذكى لمستقبلنا.";

        // Primary Action Button (CTA 1)
        public string PrimaryButtonTextEn { get; set; } = "Explore Our Divisions";
        public string PrimaryButtonTextAr { get; set; } = "استكشف اقسامنا";
        public string PrimaryButtonUrl { get; set; } = "#divisions";

        // Secondary Action Button (CTA 2)
        public string SecondaryButtonTextEn { get; set; } = "Get in Touch";
        public string SecondaryButtonTextAr { get; set; } = "تواصل معنا";
        public string SecondaryButtonUrl { get; set; } = "/Home/Contact";

        // Metrics / Statistics Cards (3 items)
        public string Stat1Value { get; set; } = "6+";
        public string Stat1LabelEn { get; set; } = "AI Systems";
        public string Stat1LabelAr { get; set; } = "أنظمة الذكاء الاصطناعي";

        public string Stat2Value { get; set; } = "2+";
        public string Stat2LabelEn { get; set; } = "Enterprise Scale";
        public string Stat2LabelAr { get; set; } = "حلول المؤسسات";

        public string Stat3Value { get; set; } = "5+";
        public string Stat3LabelEn { get; set; } = "Digital Platforms";
        public string Stat3LabelAr { get; set; } = "المنصات الرقمية";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
