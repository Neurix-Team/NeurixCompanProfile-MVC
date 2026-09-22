using System;
using System.Collections.Generic;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsDashboardViewModel
    {
        public int TotalProfilesCount { get; set; }
        public int PublishedProfilesCount { get; set; }

        public int TotalServicesCount { get; set; }
        public int PublishedServicesCount { get; set; }

        public int TotalSocialLinksCount { get; set; }
        public int PublishedSocialLinksCount { get; set; }

        public int TotalTeamMembersCount { get; set; }
        public int PublishedTeamMembersCount { get; set; }

        public int TotalBlogPostsCount { get; set; }
        public int PublishedBlogPostsCount { get; set; }

        public int TotalProjectsCount { get; set; }
        public int PublishedProjectsCount { get; set; }

        public int TotalTestimonialsCount { get; set; }
        public int PublishedTestimonialsCount { get; set; }

        public int TotalMediaAssetsCount { get; set; }

        public int TotalDivisionPagesCount { get; set; }
        public int PublishedDivisionPagesCount { get; set; }

        public int TotalPageBandsCount { get; set; }
        public int PublishedPageBandsCount { get; set; }

        public IReadOnlyList<CmsBrandDashboardSummary> BrandSummaries { get; set; } = new List<CmsBrandDashboardSummary>();
        public IReadOnlyList<CmsServiceSummaryDto> RecentServices { get; set; } = new List<CmsServiceSummaryDto>();
        public IReadOnlyList<CmsSocialLinkSummaryDto> RecentSocialLinks { get; set; } = new List<CmsSocialLinkSummaryDto>();
        public IReadOnlyList<CmsBlogPostSummaryDto> RecentBlogPosts { get; set; } = new List<CmsBlogPostSummaryDto>();
    }

    public class CmsBrandDashboardSummary
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public bool IsPublished { get; set; }
        public int ServicesCount { get; set; }
        public int SocialLinksCount { get; set; }
        public int TeamMembersCount { get; set; }
        public int BlogPostsCount { get; set; }
        public int ProjectsCount { get; set; }
        public int DivisionPagesCount { get; set; }
        public int PageBandsCount { get; set; }
    }
}
