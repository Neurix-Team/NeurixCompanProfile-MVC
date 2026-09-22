using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsContentPageItemViewModel
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public bool HasBody { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int LiveBandsCount { get; set; }
        public int TotalItemsCount { get; set; }
    }

    public class CmsContentPageListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = new List<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsContentPageItemViewModel> Pages { get; set; } = new List<CmsContentPageItemViewModel>();
    }

    public class CmsContentPageFormViewModel
    {
        public Guid? Id { get; set; }

        public List<CmsDivisionBandSummaryViewModel> LiveBands { get; set; } = new();

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand / Company Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Page slug is required.")]
        [Display(Name = "Page Slug")]
        [StringLength(100)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "English page title is required.")]
        [Display(Name = "Page Title (English)")]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic page title is required.")]
        [Display(Name = "Page Title (Arabic)")]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Meta Description (English)")]
        [StringLength(500)]
        public string? MetaDescriptionEn { get; set; }

        [Display(Name = "Meta Description (Arabic)")]
        [StringLength(500)]
        public string? MetaDescriptionAr { get; set; }

        // ── Hero ──
        [Display(Name = "Hero Badge (English)")]
        [StringLength(150)]
        public string? HeroBadgeEn { get; set; }

        [Display(Name = "Hero Badge (Arabic)")]
        [StringLength(150)]
        public string? HeroBadgeAr { get; set; }

        [Display(Name = "Hero Title — Plain Part (English)")]
        [StringLength(300)]
        public string? HeroTitlePrefixEn { get; set; }

        [Display(Name = "Hero Title — Plain Part (Arabic)")]
        [StringLength(300)]
        public string? HeroTitlePrefixAr { get; set; }

        [Display(Name = "Hero Title — Highlighted Part (English)")]
        [StringLength(300)]
        public string? HeroTitleHighlightEn { get; set; }

        [Display(Name = "Hero Title — Highlighted Part (Arabic)")]
        [StringLength(300)]
        public string? HeroTitleHighlightAr { get; set; }

        [Display(Name = "Hero Subtitle (English)")]
        [StringLength(2000)]
        public string? HeroSubtitleEn { get; set; }

        [Display(Name = "Hero Subtitle (Arabic)")]
        [StringLength(2000)]
        public string? HeroSubtitleAr { get; set; }

        // ── Rich body ──
        [Display(Name = "Body Content (English) — HTML allowed")]
        public string? BodyEn { get; set; }

        [Display(Name = "Body Content (Arabic) — HTML allowed")]
        public string? BodyAr { get; set; }

        // ── Mission / Vision ──
        [Display(Name = "Mission Title (English)")]
        [StringLength(200)]
        public string? MissionTitleEn { get; set; }

        [Display(Name = "Mission Title (Arabic)")]
        [StringLength(200)]
        public string? MissionTitleAr { get; set; }

        [Display(Name = "Mission Text (English)")]
        [StringLength(4000)]
        public string? MissionTextEn { get; set; }

        [Display(Name = "Mission Text (Arabic)")]
        [StringLength(4000)]
        public string? MissionTextAr { get; set; }

        [Display(Name = "Vision Title (English)")]
        [StringLength(200)]
        public string? VisionTitleEn { get; set; }

        [Display(Name = "Vision Title (Arabic)")]
        [StringLength(200)]
        public string? VisionTitleAr { get; set; }

        [Display(Name = "Vision Text (English)")]
        [StringLength(4000)]
        public string? VisionTextEn { get; set; }

        [Display(Name = "Vision Text (Arabic)")]
        [StringLength(4000)]
        public string? VisionTextAr { get; set; }

        // ── Closing CTA banner ──
        [Display(Name = "CTA Badge (English)")]
        [StringLength(150)]
        public string? CtaBadgeEn { get; set; }

        [Display(Name = "CTA Badge (Arabic)")]
        [StringLength(150)]
        public string? CtaBadgeAr { get; set; }

        [Display(Name = "CTA Title (English)")]
        [StringLength(300)]
        public string? CtaTitleEn { get; set; }

        [Display(Name = "CTA Title (Arabic)")]
        [StringLength(300)]
        public string? CtaTitleAr { get; set; }

        [Display(Name = "CTA Subtitle (English)")]
        [StringLength(1000)]
        public string? CtaSubtitleEn { get; set; }

        [Display(Name = "CTA Subtitle (Arabic)")]
        [StringLength(1000)]
        public string? CtaSubtitleAr { get; set; }

        [Display(Name = "CTA Button Text (English)")]
        [StringLength(150)]
        public string? CtaButtonTextEn { get; set; }

        [Display(Name = "CTA Button Text (Arabic)")]
        [StringLength(150)]
        public string? CtaButtonTextAr { get; set; }

        [Display(Name = "Published (visible on public site)")]
        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// Drives which field groups the edit form shows. "about" needs mission/vision and
        /// the CTA banner; "privacy" needs the rich body; "contact" only needs the hero.
        /// Unknown slugs get every group so a new page is never uneditable.
        /// </summary>
        public bool ShowMissionVision => Slug is "about";
        public bool ShowCta => Slug is "about";
        public bool ShowBody => Slug is not ("about" or "contact");
    }
}
