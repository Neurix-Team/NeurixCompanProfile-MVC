using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    /// <summary>
    /// Summary DTO for listing social links.
    /// </summary>
    public class CmsSocialLinkSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? IconName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Detailed read model for a social link.
    /// </summary>
    public class CmsSocialLinkDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? IconName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Input DTO for creating or updating a social link.
    /// </summary>
    public class CmsSocialLinkUpsertDto
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "Platform name is required (e.g., linkedin, twitter, facebook).")]
        [StringLength(50, ErrorMessage = "Platform cannot exceed 50 characters.")]
        public string Platform { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL is required.")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(1000, ErrorMessage = "URL cannot exceed 1000 characters.")]
        public string Url { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
        public string? DisplayName { get; set; }

        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string? IconName { get; set; }

        [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000.")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
    }
}
