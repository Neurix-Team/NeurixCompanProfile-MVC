using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsServiceListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = new List<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsServiceSummaryDto> Services { get; set; } = new List<CmsServiceSummaryDto>();
    }

    public class CmsServiceFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Please select a company/brand profile.")]
        [Display(Name = "Brand / Company Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "English service name is required.")]
        [Display(Name = "Service Name (English)")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters.")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic service name is required.")]
        [Display(Name = "Service Name (Arabic)")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters.")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [Display(Name = "URL Slug")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens (e.g. 'ai-research').")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Short Description (English)")]
        [StringLength(500, ErrorMessage = "English short description cannot exceed 500 characters.")]
        public string? ShortDescriptionEn { get; set; }

        [Display(Name = "Short Description (Arabic)")]
        [StringLength(500, ErrorMessage = "Arabic short description cannot exceed 500 characters.")]
        public string? ShortDescriptionAr { get; set; }

        [Display(Name = "Full Description (English)")]
        [StringLength(4000, ErrorMessage = "English description cannot exceed 4000 characters.")]
        public string? DescriptionEn { get; set; }

        [Display(Name = "Full Description (Arabic)")]
        [StringLength(4000, ErrorMessage = "Arabic description cannot exceed 4000 characters.")]
        public string? DescriptionAr { get; set; }

        [Display(Name = "Lucide Icon Name")]
        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string? IconName { get; set; }

        [Display(Name = "Image / Graphic Path")]
        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters.")]
        public string? ImagePath { get; set; }

        [Display(Name = "Upload New Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000.")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Published (visible on public website)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsServiceDetailsViewModel
    {
        public CmsServiceDetailDto Service { get; set; } = new();
    }
}
