using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// One repeated element inside a <see cref="CmsPageBand"/>: a capability card, a small
    /// uppercase chip in a hero strip, an image in a staggered image grid, or a call-to-action
    /// button. Which of those it renders as is decided by the band, not by the row, so the same
    /// table serves all four shapes and a page can gain a card without a schema change.
    /// </summary>
    public class CmsPageBandItem : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        /// <summary>Which public page this item belongs to. Values come from <c>CmsPageKeys</c>.</summary>
        public string PageKey { get; set; } = string.Empty;

        /// <summary>Which band on that page owns this item.</summary>
        public string BandKey { get; set; } = string.Empty;

        /// <summary>Lucide icon name. Ignored by bands that render chips or images.</summary>
        public string? IconName { get; set; }

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        /// <summary>Image for bands that render an image grid rather than cards.</summary>
        public string? ImagePath { get; set; }

        public string? LinkUrl { get; set; }
        public string? LinkTextEn { get; set; }
        public string? LinkTextAr { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
