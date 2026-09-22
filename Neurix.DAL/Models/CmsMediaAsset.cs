using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a stored digital asset in the CMS Media Library.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsMediaAsset : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // e.g. "/uploads/cms/media/image.png"

        public long FileSizeBytes { get; set; }
        public string? ContentType { get; set; } // e.g. "image/png", "image/svg+xml"

        public string? AltTextEn { get; set; }
        public string? AltTextAr { get; set; }
        public string? Category { get; set; } // e.g. "Logos", "Banners", "Team", "Blog", "General"
        public string? Tags { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
