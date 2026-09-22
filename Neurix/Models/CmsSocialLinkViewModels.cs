using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsSocialLinkListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = new List<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsSocialLinkSummaryDto> SocialLinks { get; set; } = new List<CmsSocialLinkSummaryDto>();
    }

    public class CmsSocialLinkFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Please select a company/brand profile.")]
        [Display(Name = "Brand / Company Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Platform is required (e.g., linkedin, twitter, facebook).")]
        [Display(Name = "Platform (e.g. linkedin, twitter, github, youtube)")]
        [StringLength(50, ErrorMessage = "Platform cannot exceed 50 characters.")]
        public string Platform { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination URL is required.")]
        [Display(Name = "Target URL")]
        [Url(ErrorMessage = "Please enter a valid URL including http:// or https://")]
        [StringLength(1000, ErrorMessage = "URL cannot exceed 1000 characters.")]
        public string Url { get; set; } = string.Empty;

        [Display(Name = "Display Name (Optional label)")]
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
        public string? DisplayName { get; set; }

        [Display(Name = "Lucide Icon Name (Optional)")]
        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string? IconName { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000.")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Published (visible in footer)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsSocialLinkDetailsViewModel
    {
        public CmsSocialLinkDetailDto SocialLink { get; set; } = new();
    }
}
