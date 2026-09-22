using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    /// <summary>
    /// Summary DTO for listing services in cards, tables, and overview sections.
    /// </summary>
    public class CmsServiceSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionAr { get; set; }
        public string? IconName { get; set; }
        public string? ImagePath { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Detailed read model for a service.
    /// </summary>
    public class CmsServiceDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }
        public string? IconName { get; set; }
        public string? ImagePath { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// <summary>
    /// Input DTO for creating or updating a CMS service.
    /// </summary>
    public class CmsServiceUpsertDto
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "English service name is required.")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters.")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic service name is required.")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters.")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "English short description cannot exceed 500 characters.")]
        public string? ShortDescriptionEn { get; set; }

        [StringLength(500, ErrorMessage = "Arabic short description cannot exceed 500 characters.")]
        public string? ShortDescriptionAr { get; set; }

        [StringLength(4000, ErrorMessage = "English description cannot exceed 4000 characters.")]
        public string? DescriptionEn { get; set; }

        [StringLength(4000, ErrorMessage = "Arabic description cannot exceed 4000 characters.")]
        public string? DescriptionAr { get; set; }

        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string? IconName { get; set; }

        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters.")]
        public string? ImagePath { get; set; }

        [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000.")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
    }
}
