using System;
using System.ComponentModel.DataAnnotations;

namespace Neurix.BLL.Dtos.Cms
{
    public class CmsSiteSettingDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? ValueEn { get; set; }
        public string? ValueAr { get; set; }
        public string SettingType { get; set; } = "text";
        public string GroupName { get; set; } = "General";
        public string Label { get; set; } = string.Empty;
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CmsSiteSettingUpsertDto
    {
        [Required]
        public Guid CompanyProfileId { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? ValueEn { get; set; }

        [StringLength(4000)]
        public string? ValueAr { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingType { get; set; } = "text";

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; } = "General";

        [Required]
        [StringLength(200)]
        public string Label { get; set; } = string.Empty;
    }
}
