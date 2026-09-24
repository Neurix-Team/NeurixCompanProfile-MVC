using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsSiteSettingListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsSiteSettingDto> Settings { get; set; } = Array.Empty<CmsSiteSettingDto>();
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();
    }

    public class CmsSiteSettingSaveItem
    {
        public string Key { get; set; } = string.Empty;
        public string? ValueEn { get; set; }
        public string? ValueAr { get; set; }
        public string SettingType { get; set; } = "text";
        public string GroupName { get; set; } = "General";
        public string Label { get; set; } = string.Empty;
    }

    public class CmsSiteSettingBatchUpdateViewModel
    {
        public Guid CompanyProfileId { get; set; }
        public List<CmsSiteSettingSaveItem> Settings { get; set; } = new();
    }

    public class CmsSiteSettingFormViewModel
    {
        [Required(ErrorMessage = "Please select a company/brand profile.")]
        [Display(Name = "Brand / Company Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        [Required(ErrorMessage = "Setting key is required.")]
        [Display(Name = "Key")]
        [StringLength(100, ErrorMessage = "Key cannot exceed 100 characters.")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Label is required.")]
        [Display(Name = "Label")]
        [StringLength(200, ErrorMessage = "Label cannot exceed 200 characters.")]
        public string Label { get; set; } = string.Empty;

        [Display(Name = "English Value")]
        [StringLength(4000)]
        public string? ValueEn { get; set; }

        [Display(Name = "Arabic Value")]
        [StringLength(4000)]
        public string? ValueAr { get; set; }

        [Required(ErrorMessage = "Setting type is required.")]
        [Display(Name = "Setting Type")]
        public string SettingType { get; set; } = "text";

        public IEnumerable<SelectListItem> SettingTypeOptions { get; set; } = Array.Empty<SelectListItem>();

        [Required(ErrorMessage = "Group name is required.")]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; } = "General";

        public IEnumerable<SelectListItem> GroupOptions { get; set; } = Array.Empty<SelectListItem>();
    }

    /// <summary>Preset keys chosen on the CMS Theme page (see Neurix.Common.SiteTheme).</summary>
    public class CmsThemeViewModel
    {
        public string Palette { get; set; } = string.Empty;
        public string Light { get; set; } = string.Empty;
        public string Dark { get; set; } = string.Empty;
    }
}
