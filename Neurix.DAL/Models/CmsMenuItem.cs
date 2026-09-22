using System;

namespace Neurix.DAL.Models
{
    public enum CmsMenuItemPlacement
    {
        Header = 1,
        Footer = 2,
        Both = 3,
        FooterBottom = 4
    }

    /// <summary>
    /// Represents a customizable navigation link in the site header, footer, or both.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsMenuItem : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string LabelEn { get; set; } = string.Empty;
        public string LabelAr { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconName { get; set; }

        public CmsMenuItemPlacement Placement { get; set; } = CmsMenuItemPlacement.Header;
        public int DisplayOrder { get; set; } = 0;
        public bool OpenInNewTab { get; set; } = false;

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
