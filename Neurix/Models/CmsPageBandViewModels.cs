using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    /// <summary>
    /// Overview of every CMS-editable band across the secondary public pages, grouped by page.
    /// </summary>
    public class CmsPageBandsIndexViewModel
    {
        public Guid SelectedCompanyId { get; set; }

        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; }
            = Array.Empty<CmsCompanyProfileSummaryDto>();

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        public IReadOnlyList<CmsPageOverviewViewModel> Pages { get; set; }
            = Array.Empty<CmsPageOverviewViewModel>();
    }

    /// <summary>One public page in the overview, with a row per band.</summary>
    public class CmsPageOverviewViewModel
    {
        public string PageKey { get; set; } = string.Empty;

        /// <summary>Display name, e.g. "Neurix AI Labs".</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>The public URL, so the editor can open the page it is editing.</summary>
        public string PublicUrl { get; set; } = "/";

        /// <summary>
        /// Set for the five pages that also have a division-page row, so the overview can point
        /// at the editor that owns their hero title, hero subtitle and mission.
        /// </summary>
        public bool HasDivisionPageRow { get; set; }

        public IReadOnlyList<CmsPageBandRowViewModel> Bands { get; set; }
            = Array.Empty<CmsPageBandRowViewModel>();
    }

    /// <summary>One band row in the overview.</summary>
    public class CmsPageBandRowViewModel
    {
        public string BandKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ItemsLabel { get; set; } = string.Empty;

        /// <summary>Null until the band has been saved once; the page then shows a "Default" chip.</summary>
        public CmsPageBandDto? Band { get; set; }

        public int ItemCount { get; set; }
        public bool SupportsItems { get; set; }
    }

    /// <summary>
    /// Edit form for one band. The Supports* flags come from the controller's descriptor table
    /// and hide the fields the band does not render, so the editor is never offered a box that
    /// would have no effect on the page.
    /// </summary>
    public class CmsPageBandFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        public string PageKey { get; set; } = string.Empty;
        public string BandKey { get; set; } = string.Empty;

        /// <summary>Page and band names for the form heading and breadcrumb.</summary>
        public string PageLabel { get; set; } = string.Empty;
        public string BandLabel { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = "/";

        /// <summary>What this band's items are called, e.g. "Capability cards".</summary>
        public string ItemsLabel { get; set; } = string.Empty;

        public bool SupportsBadge { get; set; } = true;
        public bool SupportsTitle { get; set; } = true;
        public bool SupportsTitleSuffix { get; set; }
        public bool SupportsBody { get; set; } = true;
        public bool SupportsImage { get; set; }
        public bool SupportsButton { get; set; }
        public bool SupportsItemLink { get; set; }
        public bool SupportsEmptyState { get; set; }
        public bool SupportsItems { get; set; }

        public IReadOnlyList<CmsPageBandItemDto> Items { get; set; }
            = Array.Empty<CmsPageBandItemDto>();

        [Display(Name = "Badge (English)")]
        [StringLength(200)]
        public string? BadgeEn { get; set; }

        [Display(Name = "Badge (Arabic)")]
        [StringLength(200)]
        public string? BadgeAr { get; set; }

        [Display(Name = "Heading — first part (English)")]
        [StringLength(300)]
        public string? TitlePrefixEn { get; set; }

        [Display(Name = "Heading — first part (Arabic)")]
        [StringLength(300)]
        public string? TitlePrefixAr { get; set; }

        [Display(Name = "Heading — highlighted part (English)")]
        [StringLength(300)]
        public string? TitleHighlightEn { get; set; }

        [Display(Name = "Heading — highlighted part (Arabic)")]
        [StringLength(300)]
        public string? TitleHighlightAr { get; set; }

        [Display(Name = "Heading — last part (English)")]
        [StringLength(300)]
        public string? TitleSuffixEn { get; set; }

        [Display(Name = "Heading — last part (Arabic)")]
        [StringLength(300)]
        public string? TitleSuffixAr { get; set; }

        [Display(Name = "Paragraph (English)")]
        [StringLength(4000)]
        public string? BodyEn { get; set; }

        [Display(Name = "Paragraph (Arabic)")]
        [StringLength(4000)]
        public string? BodyAr { get; set; }

        [Display(Name = "Image URL / Path")]
        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Button Text (English)")]
        [StringLength(100)]
        public string? ButtonTextEn { get; set; }

        [Display(Name = "Button Text (Arabic)")]
        [StringLength(100)]
        public string? ButtonTextAr { get; set; }

        [Display(Name = "Button Link")]
        [StringLength(500)]
        public string? ButtonUrl { get; set; }

        [Display(Name = "Card Link Text (English)")]
        [StringLength(100)]
        public string? ItemLinkTextEn { get; set; }

        [Display(Name = "Card Link Text (Arabic)")]
        [StringLength(100)]
        public string? ItemLinkTextAr { get; set; }

        [Display(Name = "Empty List Message (English)")]
        [StringLength(1000)]
        public string? EmptyStateEn { get; set; }

        [Display(Name = "Empty List Message (Arabic)")]
        [StringLength(1000)]
        public string? EmptyStateAr { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;
    }

    /// <summary>Create/edit form for one card, chip, image or button inside a band.</summary>
    public class CmsPageBandItemFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        public string PageKey { get; set; } = string.Empty;
        public string BandKey { get; set; } = string.Empty;

        public string PageLabel { get; set; } = string.Empty;
        public string BandLabel { get; set; } = string.Empty;
        public string ItemsLabel { get; set; } = string.Empty;

        public bool SupportsIcon { get; set; } = true;
        public bool SupportsDescription { get; set; } = true;
        public bool SupportsImage { get; set; }
        public bool SupportsLink { get; set; }

        [Required(ErrorMessage = "English title is required.")]
        [Display(Name = "Title (English)")]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic title is required.")]
        [Display(Name = "Title (Arabic)")]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Icon Name (Lucide)")]
        [StringLength(100)]
        public string? IconName { get; set; }

        [Display(Name = "Description (English)")]
        [StringLength(2000)]
        public string? DescriptionEn { get; set; }

        [Display(Name = "Description (Arabic)")]
        [StringLength(2000)]
        public string? DescriptionAr { get; set; }

        [Display(Name = "Image URL / Path")]
        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Link URL")]
        [StringLength(500)]
        public string? LinkUrl { get; set; }

        [Display(Name = "Link Text (English)")]
        [StringLength(100)]
        public string? LinkTextEn { get; set; }

        [Display(Name = "Link Text (Arabic)")]
        [StringLength(100)]
        public string? LinkTextAr { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;
    }
}
