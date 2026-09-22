using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsDivisionPageDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string HeroTitleEn { get; set; } = string.Empty;
        public string HeroTitleAr { get; set; } = string.Empty;
        public string? HeroSubtitleEn { get; set; }
        public string? HeroSubtitleAr { get; set; }
        public string? MissionEn { get; set; }
        public string? MissionAr { get; set; }
        public string? ContentJson { get; set; }
        public string? CoverImagePath { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsDivisionPageUpsertDto
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "Division slug is required.")]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "English hero title is required.")]
        [StringLength(300)]
        public string HeroTitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic hero title is required.")]
        [StringLength(300)]
        public string HeroTitleAr { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? HeroSubtitleEn { get; set; }

        [StringLength(1000)]
        public string? HeroSubtitleAr { get; set; }

        [StringLength(2000)]
        public string? MissionEn { get; set; }

        [StringLength(2000)]
        public string? MissionAr { get; set; }

        public string? ContentJson { get; set; }

        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
