using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsTeamMemberSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string? BioEn { get; set; }
        public string? BioAr { get; set; }
        public string? PhotoPath { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? Email { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsTeamMemberDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string? BioEn { get; set; }
        public string? BioAr { get; set; }
        public string? PhotoPath { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? Email { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsTeamMemberUpsertDto
    {
        [Required(ErrorMessage = "Company brand profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "English name is required.")]
        [StringLength(200)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required.")]
        [StringLength(200)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English title is required.")]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic title is required.")]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? BioEn { get; set; }

        [StringLength(1000)]
        public string? BioAr { get; set; }

        [StringLength(500)]
        public string? PhotoPath { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "Invalid LinkedIn URL.")]
        public string? LinkedInUrl { get; set; }

        [StringLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Range(0, 1000)]
        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
    }
}
