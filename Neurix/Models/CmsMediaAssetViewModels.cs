using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsMediaAssetListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public string? SelectedCategory { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsMediaAssetDto> Assets { get; set; } = Array.Empty<CmsMediaAssetDto>();
        public IReadOnlyDictionary<Guid, int> UsageCounts { get; set; } = new Dictionary<Guid, int>();
        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Array.Empty<SelectListItem>();
    }

    public class CmsMediaAssetUploadViewModel
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "File upload is required.")]
        [Display(Name = "Select File")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "Category")]
        [StringLength(100)]
        public string? Category { get; set; } = "General";

        [Display(Name = "Alt Text (English)")]
        [StringLength(300)]
        public string? AltTextEn { get; set; }

        [Display(Name = "Alt Text (Arabic)")]
        [StringLength(300)]
        public string? AltTextAr { get; set; }

        [Display(Name = "Tags (Comma-separated)")]
        [StringLength(500)]
        public string? Tags { get; set; }
    }
}
