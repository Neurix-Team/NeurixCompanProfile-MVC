using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a client testimonial, partner endorsement, or stakeholder review.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsTestimonial : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string AuthorNameEn { get; set; } = string.Empty;
        public string AuthorNameAr { get; set; } = string.Empty;

        public string? AuthorTitleEn { get; set; }
        public string? AuthorTitleAr { get; set; }

        public string? CompanyName { get; set; }
        public string? AuthorPhotoPath { get; set; }

        public string QuoteEn { get; set; } = string.Empty;
        public string QuoteAr { get; set; } = string.Empty;

        public int Rating { get; set; } = 5; // 1-5 stars

        public int DisplayOrder { get; set; } = 0;

        public bool IsFeatured { get; set; } = true;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
