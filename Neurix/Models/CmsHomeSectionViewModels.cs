using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsHomeSectionsIndexViewModel
    {
        public Guid SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = new List<CmsCompanyProfileSummaryDto>();
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        public CmsHeroSectionDto? HeroSection { get; set; }
        public CmsHumanVisionSectionDto? HumanVisionSection { get; set; }
        public CmsPioneersSectionDto? PioneersSection { get; set; }
        public CmsPillarsSectionDto? PillarsSection { get; set; }
        public IReadOnlyList<CmsPillarItemDto> Pillars { get; set; } = new List<CmsPillarItemDto>();
        public CmsDivisionsSectionDto? DivisionsSection { get; set; }
        public IReadOnlyList<CmsDivisionItemDto> DivisionItems { get; set; } = new List<CmsDivisionItemDto>();
        public CmsAiEngineeringSectionDto? AiEngineeringSection { get; set; }
        public IReadOnlyList<CmsAiEngineeringItemDto> AiEngineeringItems { get; set; } = new List<CmsAiEngineeringItemDto>();
        public IReadOnlyList<CmsListSectionHeaderDto> ListSectionHeaders { get; set; } = new List<CmsListSectionHeaderDto>();
        public CmsEthicsSectionDto? EthicsSection { get; set; }
        public CmsCtaSectionDto? CtaSection { get; set; }

        public CmsListSectionHeaderDto? ListHeader(string sectionKey) =>
            ListSectionHeaders.FirstOrDefault(h => h.SectionKey == sectionKey);
    }

    public class CmsHeroSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Badge / Eyebrow (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; } = "Empowering the Future";

        [Display(Name = "Badge / Eyebrow (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; } = "تمكين المستقبل";

        [Display(Name = "Headline Prefix (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; } = "Building Human-Centered ";

        [Display(Name = "Headline Highlight (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; } = "AI";

        [Display(Name = "Headline Suffix (EN)")]
        [StringLength(200)]
        public string? TitleSuffixEn { get; set; } = " for Tomorrow";

        [Display(Name = "Headline Prefix (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; } = " ";

        [Display(Name = "Headline Highlight (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; } = "ذكاء اصطناعي";

        [Display(Name = "Headline Suffix (AR)")]
        [StringLength(200)]
        public string? TitleSuffixAr { get; set; } = " محوره الإنسان، من أجل الغد";

        [Display(Name = "Subtitle (EN)")]
        [StringLength(1000)]
        public string? SubtitleEn { get; set; } = "Neurix AI is building smarter digital experiences for tomorrow.";

        [Display(Name = "Subtitle (AR)")]
        [StringLength(1000)]
        public string? SubtitleAr { get; set; } = "نصنع في نيوركس AI تجارب رقمية أذكى لمستقبلنا.";

        [Display(Name = "Primary Button Text (EN)")]
        [StringLength(100)]
        public string? PrimaryButtonTextEn { get; set; } = "Explore Our Divisions";

        [Display(Name = "Primary Button Text (AR)")]
        [StringLength(100)]
        public string? PrimaryButtonTextAr { get; set; } = "استكشف اقسامنا";

        [Display(Name = "Primary Button Link / Action")]
        [StringLength(500)]
        public string? PrimaryButtonUrl { get; set; } = "#divisions";

        [Display(Name = "Secondary Button Text (EN)")]
        [StringLength(100)]
        public string? SecondaryButtonTextEn { get; set; } = "Get in Touch";

        [Display(Name = "Secondary Button Text (AR)")]
        [StringLength(100)]
        public string? SecondaryButtonTextAr { get; set; } = "تواصل معنا";

        [Display(Name = "Secondary Button Link / Action")]
        [StringLength(500)]
        public string? SecondaryButtonUrl { get; set; } = "/Home/Contact";

        // Stats Card 1
        [Display(Name = "Stat 1 Value")]
        [StringLength(50)]
        public string? Stat1Value { get; set; } = "6+";

        [Display(Name = "Stat 1 Label (EN)")]
        [StringLength(100)]
        public string? Stat1LabelEn { get; set; } = "AI Systems";

        [Display(Name = "Stat 1 Label (AR)")]
        [StringLength(100)]
        public string? Stat1LabelAr { get; set; } = "أنظمة الذكاء الاصطناعي";

        // Stats Card 2
        [Display(Name = "Stat 2 Value")]
        [StringLength(50)]
        public string? Stat2Value { get; set; } = "2+";

        [Display(Name = "Stat 2 Label (EN)")]
        [StringLength(100)]
        public string? Stat2LabelEn { get; set; } = "Enterprise Scale";

        [Display(Name = "Stat 2 Label (AR)")]
        [StringLength(100)]
        public string? Stat2LabelAr { get; set; } = "حلول المؤسسات";

        // Stats Card 3
        [Display(Name = "Stat 3 Value")]
        [StringLength(50)]
        public string? Stat3Value { get; set; } = "5+";

        [Display(Name = "Stat 3 Label (EN)")]
        [StringLength(100)]
        public string? Stat3LabelEn { get; set; } = "Digital Platforms";

        [Display(Name = "Stat 3 Label (AR)")]
        [StringLength(100)]
        public string? Stat3LabelAr { get; set; } = "المنصات الرقمية";

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsHumanVisionSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Badge / Eyebrow (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; } = "Our Core Philosophy";

        [Display(Name = "Badge / Eyebrow (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; } = "فلسفتنا الأساسية";

        [Display(Name = "Title Prefix (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; } = "Neurix AI ";

        [Display(Name = "Title Highlight (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; } = "Human Vision";

        [Display(Name = "Title Prefix (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; } = "رؤية نيوركس AI ";

        [Display(Name = "Title Highlight (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; } = "الإنسانية";

        [Display(Name = "First Paragraph (EN)")]
        [StringLength(4000)]
        public string? Paragraph1En { get; set; } = string.Empty;

        [Display(Name = "First Paragraph (AR)")]
        [StringLength(4000)]
        public string? Paragraph1Ar { get; set; } = string.Empty;

        [Display(Name = "Second Paragraph (EN)")]
        [StringLength(4000)]
        public string? Paragraph2En { get; set; } = string.Empty;

        [Display(Name = "Second Paragraph (AR)")]
        [StringLength(4000)]
        public string? Paragraph2Ar { get; set; } = string.Empty;

        [Display(Name = "Illustration Image File")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Illustration Image Path / URL")]
        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Display(Name = "Image Alt Text (EN)")]
        [StringLength(200)]
        public string? ImageAltEn { get; set; }

        [Display(Name = "Image Alt Text (AR)")]
        [StringLength(200)]
        public string? ImageAltAr { get; set; }

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsPioneersSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Badge / Eyebrow (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; } = "For the Innovators";

        [Display(Name = "Badge / Eyebrow (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; } = "للمبتكرين";

        [Display(Name = "Title Prefix (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; } = "A Message to the ";

        [Display(Name = "Title Highlight (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; } = "Pioneers of Innovation";

        [Display(Name = "Title Prefix (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; } = "رسالة ";

        [Display(Name = "Title Highlight (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; } = "لرواد الابتكار والأبداع";

        [Display(Name = "First Paragraph (EN)")]
        [StringLength(4000)]
        public string? Paragraph1En { get; set; } = string.Empty;

        [Display(Name = "First Paragraph (AR)")]
        [StringLength(4000)]
        public string? Paragraph1Ar { get; set; } = string.Empty;

        [Display(Name = "Second Paragraph (EN)")]
        [StringLength(4000)]
        public string? Paragraph2En { get; set; } = string.Empty;

        [Display(Name = "Second Paragraph (AR)")]
        [StringLength(4000)]
        public string? Paragraph2Ar { get; set; } = string.Empty;

        [Display(Name = "Illustration Image File")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Illustration Image Path / URL")]
        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Display(Name = "Image Alt Text (EN)")]
        [StringLength(200)]
        public string? ImageAltEn { get; set; }

        [Display(Name = "Image Alt Text (AR)")]
        [StringLength(200)]
        public string? ImageAltAr { get; set; }

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // CORE CAPABILITIES / PILLARS SECTION VIEWMODELS (#pillars)
    // ════════════════════════════════════════════════════════════════

    public class CmsPillarsSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Section Title (EN)")]
        [StringLength(200)]
        public string TitleEn { get; set; } = "Core Capabilities";

        [Display(Name = "Section Title (AR)")]
        [StringLength(200)]
        public string TitleAr { get; set; } = "الركائز الاساسية";

        [Display(Name = "Section Subtitle / Description (EN)")]
        [StringLength(2000)]
        public string? SubtitleEn { get; set; } = "The foundational pillars driving our intelligent digital ecosystems and enterprise solutions.";

        [Display(Name = "Section Subtitle / Description (AR)")]
        [StringLength(2000)]
        public string? SubtitleAr { get; set; } = "الركائز الأساسية التي تقود منظوماتنا الرقمية الذكيّة وحلول المؤسسات.";

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;

        public IReadOnlyList<CmsPillarItemDto> PillarItems { get; set; } = new List<CmsPillarItemDto>();
    }

    public class CmsPillarItemFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Lucide Icon Name (e.g. brain, layers, briefcase, shield-check, cpu, zap)")]
        [Required]
        [StringLength(100)]
        public string IconName { get; set; } = "brain";

        [Display(Name = "Title (EN)")]
        [Required]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Display(Name = "Title (AR)")]
        [Required]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Description (EN)")]
        [StringLength(2000)]
        public string? DescriptionEn { get; set; } = string.Empty;

        [Display(Name = "Description (AR)")]
        [StringLength(2000)]
        public string? DescriptionAr { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // OUR ECOSYSTEM / DIVISIONS VIEWMODELS (#divisions)
    // ════════════════════════════════════════════════════════════════

    public class CmsDivisionsSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Eyebrow Badge (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; }

        [Display(Name = "Eyebrow Badge (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; }

        [Display(Name = "Headline — Plain Part (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; }

        [Display(Name = "Headline — Gradient Part (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; }

        [Display(Name = "Headline — Plain Part (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; }

        [Display(Name = "Headline — Gradient Part (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; }

        [Display(Name = "Intro Paragraph (EN)")]
        [StringLength(2000)]
        public string? DescriptionEn { get; set; }

        [Display(Name = "Intro Paragraph (AR)")]
        [StringLength(2000)]
        public string? DescriptionAr { get; set; }

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;

        public IReadOnlyList<CmsDivisionItemDto> DivisionItems { get; set; } = new List<CmsDivisionItemDto>();
    }

    public class CmsDivisionItemFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Lucide Icon Name (e.g. beaker, cpu, users, zap, layers)")]
        [Required]
        [StringLength(100)]
        public string IconName { get; set; } = "layers";

        [Display(Name = "Title (EN)")]
        [Required]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Display(Name = "Title (AR)")]
        [Required]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Description (EN)")]
        [StringLength(2000)]
        public string? DescriptionEn { get; set; } = string.Empty;

        [Display(Name = "Description (AR)")]
        [StringLength(2000)]
        public string? DescriptionAr { get; set; } = string.Empty;

        [Display(Name = "Card Link URL")]
        [Required]
        [StringLength(500)]
        public string LinkUrl { get; set; } = "/";

        [Display(Name = "Link Label (EN)")]
        [StringLength(100)]
        public string? LinkTextEn { get; set; } = "Explore";

        [Display(Name = "Link Label (AR)")]
        [StringLength(100)]
        public string? LinkTextAr { get; set; } = "المزيد";

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // AI & ENGINEERING VIEWMODELS (#services)
    // ════════════════════════════════════════════════════════════════

    public class CmsAiEngineeringSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Eyebrow Badge (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; }

        [Display(Name = "Eyebrow Badge (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; }

        [Display(Name = "Section Title (EN)")]
        [StringLength(200)]
        public string? TitleEn { get; set; }

        [Display(Name = "Section Title (AR)")]
        [StringLength(200)]
        public string? TitleAr { get; set; }

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;

        public IReadOnlyList<CmsAiEngineeringItemDto> ServiceItems { get; set; } = new List<CmsAiEngineeringItemDto>();
    }

    public class CmsAiEngineeringItemFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Lucide Icon Name (e.g. code, database, sparkles, terminal)")]
        [Required]
        [StringLength(100)]
        public string IconName { get; set; } = "code";

        [Display(Name = "Title (EN)")]
        [Required]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Display(Name = "Title (AR)")]
        [Required]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Description (EN)")]
        [StringLength(2000)]
        public string? DescriptionEn { get; set; } = string.Empty;

        [Display(Name = "Description (AR)")]
        [StringLength(2000)]
        public string? DescriptionAr { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // LIST-SECTION HEADER VIEWMODELS (#portfolio, #insights, #testimonials)
    // ════════════════════════════════════════════════════════════════

    public class CmsListSectionHeaderFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Required]
        public string SectionKey { get; set; } = string.Empty;

        /// <summary>Human label for the band being edited, e.g. "Featured Projects". Display only.</summary>
        public string SectionLabel { get; set; } = string.Empty;

        /// <summary>Anchor rendered in the page chrome, e.g. "#portfolio". Display only.</summary>
        public string SectionAnchor { get; set; } = string.Empty;

        /// <summary>Where the band gets its items from, shown so the admin knows what this page does not control.</summary>
        public string ItemsManagedAt { get; set; } = string.Empty;

        public bool SupportsSubtitle { get; set; }
        public bool SupportsButton { get; set; }
        public bool SupportsItemLink { get; set; }
        public bool SupportsCategoryFallback { get; set; }
        public bool SupportsReadTime { get; set; }

        [Display(Name = "Eyebrow Badge (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; }

        [Display(Name = "Eyebrow Badge (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; }

        [Display(Name = "Headline — Plain Part (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; }

        [Display(Name = "Headline — Gradient Part (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; }

        [Display(Name = "Headline — Plain Part (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; }

        [Display(Name = "Headline — Gradient Part (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; }

        [Display(Name = "Intro Paragraph (EN)")]
        [StringLength(2000)]
        public string? SubtitleEn { get; set; }

        [Display(Name = "Intro Paragraph (AR)")]
        [StringLength(2000)]
        public string? SubtitleAr { get; set; }

        [Display(Name = "Card Link Label (EN)")]
        [StringLength(100)]
        public string? ItemLinkTextEn { get; set; }

        [Display(Name = "Card Link Label (AR)")]
        [StringLength(100)]
        public string? ItemLinkTextAr { get; set; }

        [Display(Name = "Uncategorised Card Label (EN)")]
        [StringLength(100)]
        public string? DefaultCategoryLabelEn { get; set; }

        [Display(Name = "Uncategorised Card Label (AR)")]
        [StringLength(100)]
        public string? DefaultCategoryLabelAr { get; set; }

        [Display(Name = "Read Time Unit (EN)")]
        [StringLength(50)]
        public string? ReadTimeSuffixEn { get; set; }

        [Display(Name = "Read Time Unit (AR)")]
        [StringLength(50)]
        public string? ReadTimeSuffixAr { get; set; }

        [Display(Name = "Undated Card Label (EN)")]
        [StringLength(100)]
        public string? UndatedLabelEn { get; set; }

        [Display(Name = "Undated Card Label (AR)")]
        [StringLength(100)]
        public string? UndatedLabelAr { get; set; }

        [Display(Name = "Button Label (EN)")]
        [StringLength(100)]
        public string? ButtonTextEn { get; set; }

        [Display(Name = "Button Label (AR)")]
        [StringLength(100)]
        public string? ButtonTextAr { get; set; }

        [Display(Name = "Button URL")]
        [StringLength(500)]
        public string? ButtonUrl { get; set; }

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    // ════════════════════════════════════════════════════════════════
    // VISION & IMPLEMENTATION FRAMEWORK VIEWMODELS (#ethics)
    // ════════════════════════════════════════════════════════════════

    public class CmsEthicsSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Badge / Eyebrow (EN)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; } = "Operational Strategy";

        [Display(Name = "Badge / Eyebrow (AR)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; } = "الاستراتيجية التشغيلية";

        [Display(Name = "Title Prefix (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; } = "Our Vision & ";

        [Display(Name = "Title Highlight (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; } = "Implementation Framework";

        [Display(Name = "Title Prefix (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; } = "رؤيتنا و ";

        [Display(Name = "Title Highlight (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; } = "إطار عمل التنفيذ";

        [Display(Name = "Description / Narrative (EN)")]
        [StringLength(4000)]
        public string? DescriptionEn { get; set; } = string.Empty;

        [Display(Name = "Description / Narrative (AR)")]
        [StringLength(4000)]
        public string? DescriptionAr { get; set; } = string.Empty;

        // Top Overlapping Image Card (e.g. Software Setup)
        [Display(Name = "Top Image File (Software Desk)")]
        public IFormFile? TopImageFile { get; set; }

        [Display(Name = "Top Image Path / URL")]
        [StringLength(500)]
        public string? TopImagePath { get; set; }

        [Display(Name = "Top Image Alt Text (EN)")]
        [StringLength(200)]
        public string? TopImageAltEn { get; set; }

        [Display(Name = "Top Image Alt Text (AR)")]
        [StringLength(200)]
        public string? TopImageAltAr { get; set; }

        // Bottom Overlapping Image Card (e.g. Research Lab)
        [Display(Name = "Bottom Image File (Research Lab)")]
        public IFormFile? BottomImageFile { get; set; }

        [Display(Name = "Bottom Image Path / URL")]
        [StringLength(500)]
        public string? BottomImagePath { get; set; }

        [Display(Name = "Bottom Image Alt Text (EN)")]
        [StringLength(200)]
        public string? BottomImageAltEn { get; set; }

        [Display(Name = "Bottom Image Alt Text (AR)")]
        [StringLength(200)]
        public string? BottomImageAltAr { get; set; }

        // Half Background Texture Image
        [Display(Name = "Background Texture Image Path / URL")]
        [StringLength(500)]
        public string? BackgroundImagePath { get; set; } = "/images/Infrastructure.png";

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsCtaSectionFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid CompanyProfileId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Badge / Eyebrow (EN)")]
        [StringLength(100)]
        public string? BadgeEn { get; set; } = "Let's Build Together";

        [Display(Name = "Badge / Eyebrow (AR)")]
        [StringLength(100)]
        public string? BadgeAr { get; set; } = "لنبني معاً";

        [Display(Name = "Title Prefix (EN)")]
        [StringLength(200)]
        public string? TitlePrefixEn { get; set; } = "Ready to engineer";

        [Display(Name = "Title Highlight (EN)")]
        [StringLength(200)]
        public string? TitleHighlightEn { get; set; } = " your future?";

        [Display(Name = "Title Prefix (AR)")]
        [StringLength(200)]
        public string? TitlePrefixAr { get; set; } = "مستعد لهندسة";

        [Display(Name = "Title Highlight (AR)")]
        [StringLength(200)]
        public string? TitleHighlightAr { get; set; } = " مستقبلك؟";

        [Display(Name = "Button Text (EN)")]
        [StringLength(100)]
        public string? ButtonTextEn { get; set; } = "Contact Us";

        [Display(Name = "Button Text (AR)")]
        [StringLength(100)]
        public string? ButtonTextAr { get; set; } = "تواصل معنا";

        [Display(Name = "Button URL")]
        [StringLength(500)]
        public string? ButtonUrl { get; set; } = "/Home/Contact";

        [Display(Name = "Contact Email")]
        [StringLength(200)]
        public string? ContactEmail { get; set; } = "neurix@aidaleel.com";

        [Display(Name = "Background Texture Image File")]
        public IFormFile? BackgroundImageFile { get; set; }

        [Display(Name = "Background Texture Image Path / URL")]
        [StringLength(500)]
        public string? BackgroundImagePath { get; set; } = "/images/Contact Us (Home) 2.png";

        [Display(Name = "Published on Live Website")]
        public bool IsPublished { get; set; } = true;
    }
}
