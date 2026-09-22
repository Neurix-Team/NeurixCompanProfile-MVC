using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsBlogPostSummaryDto
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
        public string? AuthorNameEn { get; set; }
        public int ReadTimeMinutes { get; set; }
        public DateTime? PublishedAtUtc { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CmsBlogPostDetailDto
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
        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }
        public string? CoverImagePath { get; set; }
        public string? Category { get; set; }
        public string? AuthorNameEn { get; set; }
        public string? AuthorNameAr { get; set; }
        public string? Tags { get; set; }
        public int ReadTimeMinutes { get; set; }
        public DateTime? PublishedAtUtc { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsBlogPostUpsertDto
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

        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(200)]
        public string? AuthorNameEn { get; set; }

        [StringLength(200)]
        public string? AuthorNameAr { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        [Range(1, 120)]
        public int ReadTimeMinutes { get; set; } = 5;

        public DateTime? PublishedAtUtc { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsPublished { get; set; } = true;
    }
}
