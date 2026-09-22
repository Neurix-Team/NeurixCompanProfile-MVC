using System;
using System.ComponentModel.DataAnnotations;
using Neurix.DAL.Models;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsMenuItemSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string LabelEn { get; set; } = string.Empty;
        public string LabelAr { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public CmsMenuItemPlacement Placement { get; set; }
        public int DisplayOrder { get; set; }
        public bool OpenInNewTab { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsMenuItemDetailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanySlug { get; set; } = string.Empty;
        public string LabelEn { get; set; } = string.Empty;
        public string LabelAr { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public CmsMenuItemPlacement Placement { get; set; }
        public int DisplayOrder { get; set; }
        public bool OpenInNewTab { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsMenuItemUpsertDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company profile is required.")]
        public Guid CompanyProfileId { get; set; }

        [Required(ErrorMessage = "English label is required.")]
        [StringLength(100, ErrorMessage = "English label cannot exceed 100 characters.")]
        public string LabelEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic label is required.")]
        [StringLength(100, ErrorMessage = "Arabic label cannot exceed 100 characters.")]
        public string LabelAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL / Route is required.")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
        public string Url { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string? IconName { get; set; }

        public CmsMenuItemPlacement Placement { get; set; } = CmsMenuItemPlacement.Header;

        public int DisplayOrder { get; set; } = 0;

        public bool OpenInNewTab { get; set; } = false;

        public bool IsPublished { get; set; } = true;
    }
}
