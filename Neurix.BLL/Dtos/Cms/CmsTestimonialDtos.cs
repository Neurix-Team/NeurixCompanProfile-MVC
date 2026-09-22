using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsTestimonialSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string AuthorNameEn { get; set; } = string.Empty;
        public string AuthorNameAr { get; set; } = string.Empty;
        public string? AuthorTitleEn { get; set; }
        public string? AuthorTitleAr { get; set; }
        public string? CompanyName { get; set; }
        public string? AuthorPhotoPath { get; set; }
        public string QuoteEn { get; set; } = string.Empty;
        public string QuoteAr { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CmsTestimonialDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string AuthorNameEn { get; set; } = string.Empty;
        public string AuthorNameAr { get; set; } = string.Empty;
        public string? AuthorTitleEn { get; set; }
        public string? AuthorTitleAr { get; set; }
        public string? CompanyName { get; set; }
        public string? AuthorPhotoPath { get; set; }
        public string QuoteEn { get; set; } = string.Empty;
        public string QuoteAr { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsTestimonialUpsertDto
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "English author name is required.")]
        [StringLength(200)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic author name is required.")]
        [StringLength(200)]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(200)]
        public string? TitleEn { get; set; }

        [StringLength(200)]
        public string? TitleAr { get; set; }

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? AuthorPhotoPath { get; set; }

        [Required(ErrorMessage = "English quote is required.")]
        [StringLength(1500)]
        public string QuoteEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic quote is required.")]
        [StringLength(1500)]
        public string QuoteAr { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Range(0, 1000)]
        public int DisplayOrder { get; set; } = 0;

        public bool IsFeatured { get; set; } = true;
        public bool IsPublished { get; set; } = true;
    }
}
