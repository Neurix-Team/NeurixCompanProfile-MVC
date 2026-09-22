using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Represents a portfolio item, research initiative, or case study.
    /// Scoped to a specific CmsCompanyProfile via CompanyProfileId.
    /// </summary>
    public class CmsProject : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string? SummaryEn { get; set; }
        public string? SummaryAr { get; set; }

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? CoverImagePath { get; set; }
        public string? Category { get; set; } // e.g. "AI Systems", "R&D", "Cloud & DevOps", "Automation"
        public string? ClientName { get; set; }
        public string? TechnologiesUsed { get; set; } // Comma-separated tags, e.g. "PyTorch, .NET, Azure, Three.js"
        public string? ProjectUrl { get; set; }
        public string? GithubUrl { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsFeatured { get; set; } = true;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
