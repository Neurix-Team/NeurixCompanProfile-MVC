using System.Collections.Generic;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class HomePageViewModel
    {
        public CmsCompanyProfileDetailDto? CompanyProfile { get; set; }
        public IReadOnlyList<CmsServiceSummaryDto> Services { get; set; } = new List<CmsServiceSummaryDto>();
        public IReadOnlyList<CmsTestimonialSummaryDto> Testimonials { get; set; } = new List<CmsTestimonialSummaryDto>();
        public IReadOnlyList<CmsProjectSummaryDto> FeaturedProjects { get; set; } = new List<CmsProjectSummaryDto>();
        public IReadOnlyList<CmsBlogPostSummaryDto> FeaturedPosts { get; set; } = new List<CmsBlogPostSummaryDto>();
        public CmsHeroSectionDto? HeroSection { get; set; }
        public CmsHumanVisionSectionDto? HumanVisionSection { get; set; }
        public CmsPioneersSectionDto? PioneersSection { get; set; }
        public CmsPillarsSectionDto? PillarsSection { get; set; }
        public IReadOnlyList<CmsPillarItemDto> Pillars { get; set; } = new List<CmsPillarItemDto>();
        public CmsDivisionsSectionDto? DivisionsSection { get; set; }
        public IReadOnlyList<CmsDivisionItemDto> DivisionItems { get; set; } = new List<CmsDivisionItemDto>();
        public CmsAiEngineeringSectionDto? AiEngineeringSection { get; set; }
        public IReadOnlyList<CmsAiEngineeringItemDto> AiEngineeringItems { get; set; } = new List<CmsAiEngineeringItemDto>();
        public CmsEthicsSectionDto? EthicsSection { get; set; }
        public CmsCtaSectionDto? CtaSection { get; set; }

        /// <summary>Header copy for #portfolio, keyed "portfolio". Null falls the view back to its built-in wording.</summary>
        public CmsListSectionHeaderDto? PortfolioHeader { get; set; }
        public CmsListSectionHeaderDto? InsightsHeader { get; set; }
        public CmsListSectionHeaderDto? TestimonialsHeader { get; set; }
    }
}
