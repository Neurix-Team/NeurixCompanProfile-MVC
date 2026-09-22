using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Models;

namespace Neurix.Models
{
    public class CmsMenuItemListViewModel
    {
        public Guid SelectedCompanyId { get; set; }
        public CmsMenuItemPlacement? SelectedPlacement { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = new List<CmsCompanyProfileSummaryDto>();
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();
        public IReadOnlyList<CmsMenuItemSummaryDto> MenuItems { get; set; } = new List<CmsMenuItemSummaryDto>();
    }

    public class CmsMenuItemFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company profile is required.")]
        [Display(Name = "Company Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "English label is required.")]
        [StringLength(100, ErrorMessage = "English label cannot exceed 100 characters.")]
        [Display(Name = "Menu Label (EN)")]
        public string LabelEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic label is required.")]
        [StringLength(100, ErrorMessage = "Arabic label cannot exceed 100 characters.")]
        [Display(Name = "Menu Label (AR)")]
        public string LabelAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL / Route path is required.")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
        [Display(Name = "Target URL / Route")]
        public string Url { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        [Display(Name = "Icon Name (Lucide)")]
        public string? IconName { get; set; }

        [Required(ErrorMessage = "Placement location is required.")]
        [Display(Name = "Placement Location")]
        public CmsMenuItemPlacement Placement { get; set; } = CmsMenuItemPlacement.Header;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Open in New Tab")]
        public bool OpenInNewTab { get; set; } = false;

        [Display(Name = "Published on Live Site")]
        public bool IsPublished { get; set; } = true;
    }
}
