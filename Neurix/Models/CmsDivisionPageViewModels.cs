using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsDivisionPageItemViewModel
    {
        public CmsDivisionPageDetailDto Page { get; set; } = null!;
        public int LiveBandsCount { get; set; }
        public int TotalItemsCount { get; set; }
        public string? CapabilitiesBandEditUrl { get; set; }
        public string? CapabilitiesBandTitleEn { get; set; }
    }

    public class CmsDivisionPageListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsDivisionPageDetailDto> Pages { get; set; } = Array.Empty<CmsDivisionPageDetailDto>();
        public IReadOnlyList<CmsDivisionPageItemViewModel> PageItems { get; set; } = Array.Empty<CmsDivisionPageItemViewModel>();
    }

    public class CmsDivisionBandItemSummaryViewModel
    {
        public Guid Id { get; set; }
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }
        public string? IconName { get; set; }
        public string? ImagePath { get; set; }
        public string? LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
        public string EditItemUrl { get; set; } = string.Empty;
    }

    public class CmsDivisionBandSummaryViewModel
    {
        public string BandKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ItemsLabel { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public string? BadgeEn { get; set; }
        public string? BadgeAr { get; set; }
        public string? TitlePrefixEn { get; set; }
        public string? TitlePrefixAr { get; set; }
        public string? TitleHighlightEn { get; set; }
        public string? TitleHighlightAr { get; set; }
        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }
        public string? ImagePath { get; set; }
        public bool SupportsItems { get; set; }
        public int ItemCount { get; set; }
        public IReadOnlyList<CmsDivisionBandItemSummaryViewModel> Items { get; set; } = Array.Empty<CmsDivisionBandItemSummaryViewModel>();
        public string EditBandUrl { get; set; } = string.Empty;
        public string CreateItemUrl { get; set; } = string.Empty;
    }

    public class CmsDivisionPageFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        [Required(ErrorMessage = "Division slug is required.")]
        [Display(Name = "Division Slug (labs, technology, hq, plus, club)")]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "English hero title is required.")]
        [Display(Name = "Hero Title (English)")]
        [StringLength(300)]
        public string HeroTitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic hero title is required.")]
        [Display(Name = "Hero Title (Arabic)")]
        [StringLength(300)]
        public string HeroTitleAr { get; set; } = string.Empty;

        [Display(Name = "Hero Subtitle (English)")]
        [StringLength(1000)]
        public string? HeroSubtitleEn { get; set; }

        [Display(Name = "Hero Subtitle (Arabic)")]
        [StringLength(1000)]
        public string? HeroSubtitleAr { get; set; }

        [Display(Name = "Division Mission / Mandate (English)")]
        [StringLength(2000)]
        public string? MissionEn { get; set; }

        [Display(Name = "Division Mission / Mandate (Arabic)")]
        [StringLength(2000)]
        public string? MissionAr { get; set; }

        [Display(Name = "Cover Image URL / Path")]
        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Upload Cover Image")]
        public IFormFile? CoverImageFile { get; set; }

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// Live page blocks and capabilities associated with this division (e.g. Laboratory Capabilities band and capability cards).
        /// </summary>
        public List<CmsDivisionBandSummaryViewModel> LiveBands { get; set; } = new();
    }
}
