using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsTeamMemberListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsTeamMemberSummaryDto> TeamMembers { get; set; } = Array.Empty<CmsTeamMemberSummaryDto>();
    }

    public class CmsTeamMemberFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        [Required(ErrorMessage = "English name is required.")]
        [Display(Name = "Full Name (English)")]
        [StringLength(200)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required.")]
        [Display(Name = "Full Name (Arabic)")]
        [StringLength(200)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English job title is required.")]
        [Display(Name = "Job Title / Role (English)")]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic job title is required.")]
        [Display(Name = "Job Title / Role (Arabic)")]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "Biography (English)")]
        [StringLength(1000)]
        public string? BioEn { get; set; }

        [Display(Name = "Biography (Arabic)")]
        [StringLength(1000)]
        public string? BioAr { get; set; }

        [Display(Name = "Photo URL / Path")]
        [StringLength(500)]
        public string? PhotoPath { get; set; }

        [Display(Name = "Upload Member Photo")]
        public IFormFile? PhotoFile { get; set; }

        [Display(Name = "LinkedIn Profile URL")]
        [StringLength(500)]
        [Url(ErrorMessage = "Invalid LinkedIn URL.")]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "Direct Email")]
        [StringLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 1000)]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsTeamMemberDetailsViewModel
    {
        public CmsTeamMemberDetailDto Member { get; set; } = new();
    }
}
