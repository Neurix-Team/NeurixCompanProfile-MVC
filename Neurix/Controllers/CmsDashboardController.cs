using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Services.Cms;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms")]
    public class CmsDashboardController : Controller
    {
        private readonly ICmsCompanyProfileService _profileService;
        private readonly ICmsServiceService _serviceService;
        private readonly ICmsSocialLinkService _socialLinkService;
        private readonly ICmsTeamMemberService _teamService;
        private readonly ICmsBlogPostService _blogService;
        private readonly ICmsProjectService _projectService;
        private readonly ICmsTestimonialService _testimonialService;
        private readonly ICmsMediaAssetService _mediaService;
        private readonly ICmsDivisionPageService? _divisionService;
        private readonly ICmsPageBandService? _pageBandService;
        private readonly ILogger<CmsDashboardController> _logger;

        public CmsDashboardController(
            ICmsCompanyProfileService profileService,
            ICmsServiceService serviceService,
            ICmsSocialLinkService socialLinkService,
            ICmsTeamMemberService teamService,
            ICmsBlogPostService blogService,
            ICmsProjectService projectService,
            ICmsTestimonialService testimonialService,
            ICmsMediaAssetService mediaService,
            ILogger<CmsDashboardController> logger,
            ICmsDivisionPageService? divisionService = null,
            ICmsPageBandService? pageBandService = null)
        {
            _profileService = profileService;
            _serviceService = serviceService;
            _socialLinkService = socialLinkService;
            _teamService = teamService;
            _blogService = blogService;
            _projectService = projectService;
            _testimonialService = testimonialService;
            _mediaService = mediaService;
            _logger = logger;
            _divisionService = divisionService;
            _pageBandService = pageBandService;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            var profiles = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var services = await _serviceService.GetServicesByCompanyAsync(null, includeUnpublished: true);
            var socialLinks = await _socialLinkService.GetLinksByCompanyAsync(null, includeUnpublished: true);
            var teamMembers = await _teamService.GetTeamMembersByCompanyAsync(null, includeUnpublished: true);
            var blogPosts = await _blogService.GetBlogPostsByCompanyAsync(null, includeUnpublished: true);
            var projects = await _projectService.GetProjectsByCompanyAsync(null, includeUnpublished: true);
            var testimonials = await _testimonialService.GetTestimonialsByCompanyAsync(null, includeUnpublished: true);
            var mediaAssets = await _mediaService.GetAssetsByCompanyAsync(null);

            var divisionPages = _divisionService != null
                ? await _divisionService.GetDivisionPagesByCompanyAsync(null, includeUnpublished: true)
                : Array.Empty<Neurix.BLL.Dtos.Cms.CmsDivisionPageDetailDto>();

            var totalBands = 0;
            var publishedBands = 0;
            if (_pageBandService != null)
            {
                foreach (var prof in profiles)
                {
                    var bands = await _pageBandService.GetAllBandsByCompanyIdAsync(prof.Id);
                    totalBands += bands.Count;
                    publishedBands += bands.Count(b => b.IsPublished);
                }
            }

            var brandSummaries = profiles.Select(p => new CmsBrandDashboardSummary
            {
                Id = p.Id,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                Slug = p.Slug,
                LogoPath = p.LogoPath,
                IsPublished = p.IsPublished,
                ServicesCount = services.Count(s => s.CompanyProfileId == p.Id),
                SocialLinksCount = socialLinks.Count(l => l.CompanyProfileId == p.Id),
                TeamMembersCount = teamMembers.Count(m => m.CompanyProfileId == p.Id),
                BlogPostsCount = blogPosts.Count(b => b.CompanyProfileId == p.Id),
                ProjectsCount = projects.Count(pr => pr.CompanyProfileId == p.Id),
                DivisionPagesCount = divisionPages.Count(dp => dp.CompanyProfileId == p.Id),
                PageBandsCount = totalBands
            }).ToList();

            var model = new CmsDashboardViewModel
            {
                TotalProfilesCount = profiles.Count,
                PublishedProfilesCount = profiles.Count(p => p.IsPublished),
                TotalServicesCount = services.Count,
                PublishedServicesCount = services.Count(s => s.IsPublished),
                TotalSocialLinksCount = socialLinks.Count,
                PublishedSocialLinksCount = socialLinks.Count(l => l.IsPublished),
                TotalTeamMembersCount = teamMembers.Count,
                PublishedTeamMembersCount = teamMembers.Count(m => m.IsPublished),
                TotalBlogPostsCount = blogPosts.Count,
                PublishedBlogPostsCount = blogPosts.Count(b => b.IsPublished),
                TotalProjectsCount = projects.Count,
                PublishedProjectsCount = projects.Count(p => p.IsPublished),
                TotalTestimonialsCount = testimonials.Count,
                PublishedTestimonialsCount = testimonials.Count(t => t.IsPublished),
                TotalMediaAssetsCount = mediaAssets.Count,
                TotalDivisionPagesCount = divisionPages.Count,
                PublishedDivisionPagesCount = divisionPages.Count(dp => dp.IsPublished),
                TotalPageBandsCount = totalBands,
                PublishedPageBandsCount = publishedBands,
                BrandSummaries = brandSummaries,
                RecentServices = services.OrderByDescending(s => s.CreatedAtUtc).Take(5).ToList(),
                RecentSocialLinks = socialLinks.OrderByDescending(l => l.CreatedAtUtc).Take(5).ToList(),
                RecentBlogPosts = blogPosts.OrderByDescending(b => b.CreatedAtUtc).Take(5).ToList()
            };

            return View(model);
        }
    }
}
