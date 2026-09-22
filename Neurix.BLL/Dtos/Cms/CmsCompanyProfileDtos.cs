using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    /// <summary>
    /// Lightweight DTO representing a company/brand profile in dropdowns, selectors, and summary cards.
    /// </summary>
    public class CmsCompanyProfileSummaryDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string? FaviconPath { get; set; }
        public string? PrimaryColor { get; set; }
        public string? AccentColor { get; set; }
        public string? TaglineEn { get; set; }
        public string? TaglineAr { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Full read model representing a company/brand profile for public rendering and admin detail views.
    /// </summary>
    public class CmsCompanyProfileDetailDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string? FaviconPath { get; set; }
        public string? PrimaryColor { get; set; }
        public string? AccentColor { get; set; }
        public string? TaglineEn { get; set; }
        public string? TaglineAr { get; set; }
        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressEn { get; set; }
        public string? AddressAr { get; set; }
        public string? Website { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Input DTO for creating or updating a company/brand profile in the CMS.
    /// </summary>
    public class CmsCompanyProfileUpsertDto
    {
        [Required(ErrorMessage = "English name is required.")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters.")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required.")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters.")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Logo path cannot exceed 500 characters.")]
        public string? LogoPath { get; set; }

        [StringLength(500, ErrorMessage = "Favicon path cannot exceed 500 characters.")]
        public string? FaviconPath { get; set; }

        [StringLength(50, ErrorMessage = "Primary color code cannot exceed 50 characters.")]
        [RegularExpression(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", ErrorMessage = "Primary color must be a valid hex color code (e.g. #00C2D4).")]
        public string? PrimaryColor { get; set; }

        [StringLength(50, ErrorMessage = "Accent color code cannot exceed 50 characters.")]
        [RegularExpression(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", ErrorMessage = "Accent color must be a valid hex color code (e.g. #5B5FEF).")]
        public string? AccentColor { get; set; }

        [StringLength(300, ErrorMessage = "English tagline cannot exceed 300 characters.")]
        public string? TaglineEn { get; set; }

        [StringLength(300, ErrorMessage = "Arabic tagline cannot exceed 300 characters.")]
        public string? TaglineAr { get; set; }

        [StringLength(500, ErrorMessage = "English short description cannot exceed 500 characters.")]
        public string? ShortDescriptionEn { get; set; }

        [StringLength(500, ErrorMessage = "Arabic short description cannot exceed 500 characters.")]
        public string? ShortDescriptionAr { get; set; }

        [StringLength(4000, ErrorMessage = "English description cannot exceed 4000 characters.")]
        public string? DescriptionEn { get; set; }

        [StringLength(4000, ErrorMessage = "Arabic description cannot exceed 4000 characters.")]
        public string? DescriptionAr { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters.")]
        public string? Phone { get; set; }

        [StringLength(500, ErrorMessage = "English address cannot exceed 500 characters.")]
        public string? AddressEn { get; set; }

        [StringLength(500, ErrorMessage = "Arabic address cannot exceed 500 characters.")]
        public string? AddressAr { get; set; }

        [Url(ErrorMessage = "Invalid website URL.")]
        [StringLength(300, ErrorMessage = "Website cannot exceed 300 characters.")]
        public string? Website { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
