using System;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsHeroSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitleSuffixEn { get; set; } = string.Empty;

        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;
        public string TitleSuffixAr { get; set; } = string.Empty;

        public string SubtitleEn { get; set; } = string.Empty;
        public string SubtitleAr { get; set; } = string.Empty;

        public string PrimaryButtonTextEn { get; set; } = string.Empty;
        public string PrimaryButtonTextAr { get; set; } = string.Empty;
        public string PrimaryButtonUrl { get; set; } = string.Empty;

        public string SecondaryButtonTextEn { get; set; } = string.Empty;
        public string SecondaryButtonTextAr { get; set; } = string.Empty;
        public string SecondaryButtonUrl { get; set; } = string.Empty;

        public string Stat1Value { get; set; } = string.Empty;
        public string Stat1LabelEn { get; set; } = string.Empty;
        public string Stat1LabelAr { get; set; } = string.Empty;

        public string Stat2Value { get; set; } = string.Empty;
        public string Stat2LabelEn { get; set; } = string.Empty;
        public string Stat2LabelAr { get; set; } = string.Empty;

        public string Stat3Value { get; set; } = string.Empty;
        public string Stat3LabelEn { get; set; } = string.Empty;
        public string Stat3LabelAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsHeroSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitleSuffixEn { get; set; } = string.Empty;

        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;
        public string TitleSuffixAr { get; set; } = string.Empty;

        public string SubtitleEn { get; set; } = string.Empty;
        public string SubtitleAr { get; set; } = string.Empty;

        public string PrimaryButtonTextEn { get; set; } = string.Empty;
        public string PrimaryButtonTextAr { get; set; } = string.Empty;
        public string PrimaryButtonUrl { get; set; } = string.Empty;

        public string SecondaryButtonTextEn { get; set; } = string.Empty;
        public string SecondaryButtonTextAr { get; set; } = string.Empty;
        public string SecondaryButtonUrl { get; set; } = string.Empty;

        public string Stat1Value { get; set; } = string.Empty;
        public string Stat1LabelEn { get; set; } = string.Empty;
        public string Stat1LabelAr { get; set; } = string.Empty;

        public string Stat2Value { get; set; } = string.Empty;
        public string Stat2LabelEn { get; set; } = string.Empty;
        public string Stat2LabelAr { get; set; } = string.Empty;

        public string Stat3Value { get; set; } = string.Empty;
        public string Stat3LabelEn { get; set; } = string.Empty;
        public string Stat3LabelAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;
    }

    public class CmsHumanVisionSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;

        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string Paragraph1En { get; set; } = string.Empty;
        public string Paragraph1Ar { get; set; } = string.Empty;

        public string Paragraph2En { get; set; } = string.Empty;
        public string Paragraph2Ar { get; set; } = string.Empty;

        public string? ImagePath { get; set; }
        public string? ImageAltEn { get; set; }
        public string? ImageAltAr { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsHumanVisionSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;

        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string Paragraph1En { get; set; } = string.Empty;
        public string Paragraph1Ar { get; set; } = string.Empty;

        public string Paragraph2En { get; set; } = string.Empty;
        public string Paragraph2Ar { get; set; } = string.Empty;

        public string? ImagePath { get; set; }
        public string? ImageAltEn { get; set; }
        public string? ImageAltAr { get; set; }

        public bool IsPublished { get; set; } = true;
    }

    public class CmsPioneersSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;

        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string Paragraph1En { get; set; } = string.Empty;
        public string Paragraph1Ar { get; set; } = string.Empty;

        public string Paragraph2En { get; set; } = string.Empty;
        public string Paragraph2Ar { get; set; } = string.Empty;

        public string? ImagePath { get; set; }
        public string? ImageAltEn { get; set; }
        public string? ImageAltAr { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsPioneersSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;

        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string Paragraph1En { get; set; } = string.Empty;
        public string Paragraph1Ar { get; set; } = string.Empty;

        public string Paragraph2En { get; set; } = string.Empty;
        public string Paragraph2Ar { get; set; } = string.Empty;

        public string? ImagePath { get; set; }
        public string? ImageAltEn { get; set; }
        public string? ImageAltAr { get; set; }

        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // CORE CAPABILITIES / PILLARS SECTION DTOs (#pillars)
    // ════════════════════════════════════════════════════════════════
    public class CmsPillarsSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string TitleEn { get; set; } = "Core Capabilities";
        public string TitleAr { get; set; } = "الركائز الاساسية";

        public string SubtitleEn { get; set; } = string.Empty;
        public string SubtitleAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsPillarsSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string TitleEn { get; set; } = "Core Capabilities";
        public string TitleAr { get; set; } = "الركائز الاساسية";

        public string SubtitleEn { get; set; } = string.Empty;
        public string SubtitleAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;
    }

    public class CmsPillarItemDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string IconName { get; set; } = "brain";
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsPillarItemUpsertDto
    {
        public Guid? Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string IconName { get; set; } = "brain";
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // VISION & IMPLEMENTATION FRAMEWORK DTOs (#ethics)
    // ════════════════════════════════════════════════════════════════
    public class CmsEthicsSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = "Operational Strategy";
        public string BadgeAr { get; set; } = "الاستراتيجية التشغيلية";

        public string TitlePrefixEn { get; set; } = "Our Vision & ";
        public string TitleHighlightEn { get; set; } = "Implementation Framework";
        public string TitlePrefixAr { get; set; } = "رؤيتنا و ";
        public string TitleHighlightAr { get; set; } = "إطار عمل التنفيذ";

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public string? TopImagePath { get; set; }
        public string? TopImageAltEn { get; set; }
        public string? TopImageAltAr { get; set; }

        public string? BottomImagePath { get; set; }
        public string? BottomImageAltEn { get; set; }
        public string? BottomImageAltAr { get; set; }

        public string? BackgroundImagePath { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsEthicsSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = "Operational Strategy";
        public string BadgeAr { get; set; } = "الاستراتيجية التشغيلية";

        public string TitlePrefixEn { get; set; } = "Our Vision & ";
        public string TitleHighlightEn { get; set; } = "Implementation Framework";
        public string TitlePrefixAr { get; set; } = "رؤيتنا و ";
        public string TitleHighlightAr { get; set; } = "إطار عمل التنفيذ";

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public string? TopImagePath { get; set; }
        public string? TopImageAltEn { get; set; }
        public string? TopImageAltAr { get; set; }

        public string? BottomImagePath { get; set; }
        public string? BottomImageAltEn { get; set; }
        public string? BottomImageAltAr { get; set; }

        public string? BackgroundImagePath { get; set; }

        public bool IsPublished { get; set; } = true;
    }

    public class CmsCtaSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = "Let's Build Together";
        public string BadgeAr { get; set; } = "لنبني معاً";

        public string TitlePrefixEn { get; set; } = "Ready to engineer";
        public string TitleHighlightEn { get; set; } = " your future?";
        public string TitlePrefixAr { get; set; } = "مستعد لهندسة";
        public string TitleHighlightAr { get; set; } = " مستقبلك؟";

        public string ButtonTextEn { get; set; } = "Contact Us";
        public string ButtonTextAr { get; set; } = "تواصل معنا";
        public string ButtonUrl { get; set; } = "/Home/Contact";

        public string ContactEmail { get; set; } = "neurix@aidaleel.com";
        public string? BackgroundImagePath { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsCtaSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = "Let's Build Together";
        public string BadgeAr { get; set; } = "لنبني معاً";

        public string TitlePrefixEn { get; set; } = "Ready to engineer";
        public string TitleHighlightEn { get; set; } = " your future?";
        public string TitlePrefixAr { get; set; } = "مستعد لهندسة";
        public string TitleHighlightAr { get; set; } = " مستقبلك؟";

        public string ButtonTextEn { get; set; } = "Contact Us";
        public string ButtonTextAr { get; set; } = "تواصل معنا";
        public string ButtonUrl { get; set; } = "/Home/Contact";

        public string ContactEmail { get; set; } = "neurix@aidaleel.com";
        public string? BackgroundImagePath { get; set; }

        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // OUR ECOSYSTEM / DIVISIONS DTOs (#divisions)
    // ════════════════════════════════════════════════════════════════
    public class CmsDivisionsSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsDivisionsSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;
    }

    public class CmsDivisionItemDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string IconName { get; set; } = "layers";
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public string LinkUrl { get; set; } = "/";
        public string LinkTextEn { get; set; } = "Explore";
        public string LinkTextAr { get; set; } = "المزيد";

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsDivisionItemUpsertDto
    {
        public Guid? Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string IconName { get; set; } = "layers";
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public string LinkUrl { get; set; } = "/";
        public string LinkTextEn { get; set; } = "Explore";
        public string LinkTextAr { get; set; } = "المزيد";

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // AI & ENGINEERING DTOs (#services)
    // ════════════════════════════════════════════════════════════════
    public class CmsAiEngineeringSectionDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsAiEngineeringSectionUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;
    }

    public class CmsAiEngineeringItemDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string IconName { get; set; } = "code";
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsAiEngineeringItemUpsertDto
    {
        public Guid? Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string IconName { get; set; } = "code";
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // LIST-SECTION HEADER DTOs (#portfolio, #insights, #testimonials)
    // ════════════════════════════════════════════════════════════════
    public class CmsListSectionHeaderDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string SectionKey { get; set; } = string.Empty;

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string? SubtitleEn { get; set; }
        public string? SubtitleAr { get; set; }

        public string? ItemLinkTextEn { get; set; }
        public string? ItemLinkTextAr { get; set; }

        public string? DefaultCategoryLabelEn { get; set; }
        public string? DefaultCategoryLabelAr { get; set; }

        public string? ReadTimeSuffixEn { get; set; }
        public string? ReadTimeSuffixAr { get; set; }

        public string? UndatedLabelEn { get; set; }
        public string? UndatedLabelAr { get; set; }

        public string? ButtonTextEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonUrl { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsListSectionHeaderUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string SectionKey { get; set; } = string.Empty;

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        public string? SubtitleEn { get; set; }
        public string? SubtitleAr { get; set; }

        public string? ItemLinkTextEn { get; set; }
        public string? ItemLinkTextAr { get; set; }

        public string? DefaultCategoryLabelEn { get; set; }
        public string? DefaultCategoryLabelAr { get; set; }

        public string? ReadTimeSuffixEn { get; set; }
        public string? ReadTimeSuffixAr { get; set; }

        public string? UndatedLabelEn { get; set; }
        public string? UndatedLabelAr { get; set; }

        public string? ButtonTextEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonUrl { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
