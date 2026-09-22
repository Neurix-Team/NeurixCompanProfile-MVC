using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a thought leadership article, research insight, or blog post.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsBlogPost : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string? SummaryEn { get; set; }
        public string? SummaryAr { get; set; }

        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        public string? CoverImagePath { get; set; }
        public string? Category { get; set; } // e.g., "AI Research", "Software Engineering", "Ecosystem", "News"
        public string? AuthorNameEn { get; set; }
        public string? AuthorNameAr { get; set; }
        public string? Tags { get; set; } // Comma-separated tags

        public int ReadTimeMinutes { get; set; } = 5;
        public DateTime? PublishedAtUtc { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
