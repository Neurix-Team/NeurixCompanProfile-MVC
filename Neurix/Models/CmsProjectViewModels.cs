using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsProjectListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsProjectSummaryDto> Projects { get; set; } = Array.Empty<CmsProjectSummaryDto>();
    }

    public class CmsProjectFormViewModel
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
        [Display(Name = "Project Title (English)")]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic title is required.")]
        [Display(Name = "Project Title (Arabic)")]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Summary / Overview (English)")]
        [StringLength(1000)]
        public string? SummaryEn { get; set; }

        [Display(Name = "Summary / Overview (Arabic)")]
        [StringLength(1000)]
        public string? SummaryAr { get; set; }

        [Display(Name = "Full Case Study / Description (English)")]
        public string? DescriptionEn { get; set; }

        [Display(Name = "Full Case Study / Description (Arabic)")]
        public string? DescriptionAr { get; set; }

        [Display(Name = "Cover Image URL / Path")]
        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Upload Cover Image")]
        public IFormFile? CoverImageFile { get; set; }

        [Display(Name = "Category / Domain")]
        [StringLength(100)]
        public string? Category { get; set; } = "AI Systems";

        [Display(Name = "Client / Partner Name")]
        [StringLength(200)]
        public string? ClientName { get; set; }

        [Display(Name = "Technologies Used (Comma-separated)")]
        [StringLength(500)]
        public string? TechnologiesUsed { get; set; }

        [Display(Name = "Live Project URL")]
        [StringLength(500)]
        [Url(ErrorMessage = "Invalid project URL.")]
        public string? ProjectUrl { get; set; }

        [Display(Name = "GitHub Repository URL")]
        [StringLength(500)]
        [Url(ErrorMessage = "Invalid GitHub URL.")]
        public string? GithubUrl { get; set; }

        [Display(Name = "Completion Date")]
        public DateTime? CompletedAtUtc { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 1000)]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Feature on Homepage")]
        public bool IsFeatured { get; set; } = true;

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsProjectDetailsViewModel
    {
        public CmsProjectDetailDto Project { get; set; } = new();
    }
}
