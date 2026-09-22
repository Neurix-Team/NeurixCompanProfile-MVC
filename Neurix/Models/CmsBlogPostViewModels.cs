using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsBlogPostListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsBlogPostSummaryDto> Posts { get; set; } = Array.Empty<CmsBlogPostSummaryDto>();
    }

    public class CmsBlogPostFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        [Required(ErrorMessage = "Slug is required.")]
        [Display(Name = "URL Slug")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "English title is required.")]
        [Display(Name = "Article Title (English)")]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic title is required.")]
        [Display(Name = "Article Title (Arabic)")]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Summary / Excerpt (English)")]
        [StringLength(1000)]
        public string? SummaryEn { get; set; }

        [Display(Name = "Summary / Excerpt (Arabic)")]
        [StringLength(1000)]
        public string? SummaryAr { get; set; }

        [Display(Name = "Full Article Body (English)")]
        public string? BodyEn { get; set; }

        [Display(Name = "Full Article Body (Arabic)")]
        public string? BodyAr { get; set; }

        [Display(Name = "Cover Image URL / Path")]
        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Upload Cover Image")]
        public IFormFile? CoverImageFile { get; set; }

        [Display(Name = "Category")]
        [StringLength(100)]
        public string? Category { get; set; } = "AI Research";

        [Display(Name = "Author Name (English)")]
        [StringLength(200)]
        public string? AuthorNameEn { get; set; }

        [Display(Name = "Author Name (Arabic)")]
        [StringLength(200)]
        public string? AuthorNameAr { get; set; }

        [Display(Name = "Tags (Comma-separated)")]
        [StringLength(500)]
        public string? Tags { get; set; }

        [Display(Name = "Estimated Read Time (Minutes)")]
        [Range(1, 120)]
        public int ReadTimeMinutes { get; set; } = 5;

        [Display(Name = "Publish Date (UTC)")]
        public DateTime? PublishedAtUtc { get; set; }

        [Display(Name = "Feature on Homepage")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsBlogPostDetailsViewModel
    {
        public CmsBlogPostDetailDto Post { get; set; } = new();
    }
}
