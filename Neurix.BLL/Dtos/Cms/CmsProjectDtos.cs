using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsProjectSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string? SummaryEn { get; set; }
        public string? SummaryAr { get; set; }
        public string? CoverImagePath { get; set; }
        public string? Category { get; set; }
        public string? ClientName { get; set; }
        public string? TechnologiesUsed { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CmsProjectDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string? SummaryEn { get; set; }
        public string? SummaryAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }
        public string? CoverImagePath { get; set; }
        public string? Category { get; set; }
        public string? ClientName { get; set; }
        public string? TechnologiesUsed { get; set; }
        public string? ProjectUrl { get; set; }
        public string? GithubUrl { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsProjectUpsertDto
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "Slug is required.")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "English title is required.")]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic title is required.")]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SummaryEn { get; set; }

        [StringLength(1000)]
        public string? SummaryAr { get; set; }

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(200)]
        public string? ClientName { get; set; }

        [StringLength(500)]
        public string? TechnologiesUsed { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "Invalid project URL.")]
        public string? ProjectUrl { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "Invalid Github URL.")]
        public string? GithubUrl { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        [Range(0, 1000)]
        public int DisplayOrder { get; set; } = 0;

        public bool IsFeatured { get; set; } = true;
        public bool IsPublished { get; set; } = true;
    }
}
