using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsCompanyProfileListViewModel
    {
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Profiles { get; set; } = new List<CmsCompanyProfileSummaryDto>();
    }

    public class CmsCompanyProfileFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "English name is required.")]
        [Display(Name = "Company Name (English)")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters.")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required.")]
        [Display(Name = "Company Name (Arabic)")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters.")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [Display(Name = "URL Slug")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens (e.g. 'neurix', 'daleel').")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Logo Image Path / URL")]
        [StringLength(500, ErrorMessage = "Logo path cannot exceed 500 characters.")]
        public string? LogoPath { get; set; }

        [Display(Name = "Upload New Logo")]
        public IFormFile? LogoFile { get; set; }

        [Display(Name = "Favicon Image Path / URL")]
        [StringLength(500, ErrorMessage = "Favicon path cannot exceed 500 characters.")]
        public string? FaviconPath { get; set; }

        [Display(Name = "Upload New Favicon")]
        public IFormFile? FaviconFile { get; set; }

        [Display(Name = "Primary Brand Color (Hex)")]
        [StringLength(50, ErrorMessage = "Primary color code cannot exceed 50 characters.")]
        [RegularExpression(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", ErrorMessage = "Primary color must be a valid hex color code (e.g. #00C2D4).")]
        public string? PrimaryColor { get; set; }

        [Display(Name = "Accent / Secondary Color (Hex)")]
        [StringLength(50, ErrorMessage = "Accent color code cannot exceed 50 characters.")]
        [RegularExpression(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", ErrorMessage = "Accent color must be a valid hex color code (e.g. #5B5FEF).")]
        public string? AccentColor { get; set; }

        [Display(Name = "Tagline (English)")]
        [StringLength(300, ErrorMessage = "English tagline cannot exceed 300 characters.")]
        public string? TaglineEn { get; set; }

        [Display(Name = "Tagline (Arabic)")]
        [StringLength(300, ErrorMessage = "Arabic tagline cannot exceed 300 characters.")]
        public string? TaglineAr { get; set; }

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

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Contact Email")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone format.")]
        [Display(Name = "Contact Phone")]
        [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters.")]
        public string? Phone { get; set; }

        [Display(Name = "Address (English)")]
        [StringLength(500, ErrorMessage = "English address cannot exceed 500 characters.")]
        public string? AddressEn { get; set; }

        [Display(Name = "Address (Arabic)")]
        [StringLength(500, ErrorMessage = "Arabic address cannot exceed 500 characters.")]
        public string? AddressAr { get; set; }

        [Url(ErrorMessage = "Invalid website URL format.")]
        [Display(Name = "Official Website URL")]
        [StringLength(300, ErrorMessage = "Website cannot exceed 300 characters.")]
        public string? Website { get; set; }

        [Display(Name = "Published (visible on public site)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsCompanyProfileDetailsViewModel
    {
        public CmsCompanyProfileDetailDto Profile { get; set; } = new();
    }
}
