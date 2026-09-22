using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsMediaAssetDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string? ContentType { get; set; }
        public string? AltTextEn { get; set; }
        public string? AltTextAr { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CmsMediaAssetCreateDto
    {
        [Required]
        public Guid CompanyProfileId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        [StringLength(100)]
        public string? ContentType { get; set; }

        [StringLength(300)]
        public string? AltTextEn { get; set; }

        [StringLength(300)]
        public string? AltTextAr { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } = "General";

        [StringLength(500)]
        public string? Tags { get; set; }
    }

    public class CmsMediaAssetUsageDto
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityLabel { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = "Edit";
        public Guid? CompanyProfileId { get; set; }
    }
}
