using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a key-value site configuration setting (e.g., SEO metadata, contact labels, footer text).
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsSiteSetting : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string Key { get; set; } = string.Empty; // e.g. "seo.meta.description", "contact.heading", "footer.copyright"
        public string? ValueEn { get; set; }
        public string? ValueAr { get; set; }
        public string SettingType { get; set; } = "text"; // "text", "textarea", "html", "url", "boolean"
        public string GroupName { get; set; } = "General"; // "General", "SEO", "Contact", "Footer"
        public string Label { get; set; } = string.Empty; // Human-readable label

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
