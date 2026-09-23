using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services;
using Neurix.BLL.Services.Cms;
using Neurix.Models;
using System.Diagnostics;

namespace Neurix.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILeadService _leadService;
        private readonly ICmsServiceService _cmsServiceService;
        private readonly ICmsCompanyProfileService _cmsCompanyProfileService;
        private readonly ICmsTestimonialService _cmsTestimonialService;
        private readonly ICmsBlogPostService _cmsBlogPostService;
        private readonly ICmsProjectService _cmsProjectService;
        private readonly ICmsTeamMemberService _cmsTeamMemberService;
        private readonly ICmsDivisionPageService _cmsDivisionPageService;
        private readonly ICmsSiteSettingService _cmsSiteSettingService;
        private readonly ICmsHomeSectionService _cmsHomeSectionService;
        private readonly ICmsContentPageService _cmsContentPageService;
        private readonly ICmsPageBandService _cmsPageBandService;
        private readonly IMemoryCache _cache;
        private readonly Neurix.DAL.Data.CmsContentRevision? _contentRevision;
        private readonly ILogger<HomeController> _logger;

        private const string HomeViewModelCacheKey = "home:neurix";
        private static readonly TimeSpan HomeViewModelCacheDuration = TimeSpan.FromSeconds(30);

        public HomeController(
            ILeadService leadService,
            ICmsServiceService cmsServiceService,
            ICmsCompanyProfileService cmsCompanyProfileService,
            ICmsTestimonialService cmsTestimonialService,
            ICmsBlogPostService cmsBlogPostService,
            ICmsProjectService cmsProjectService,
            ICmsTeamMemberService cmsTeamMemberService,
            ICmsDivisionPageService cmsDivisionPageService,
            ICmsSiteSettingService cmsSiteSettingService,
            ICmsHomeSectionService cmsHomeSectionService,
            ICmsContentPageService cmsContentPageService,
            ICmsPageBandService cmsPageBandService,
            IMemoryCache cache,
            ILogger<HomeController> logger,
            Neurix.DAL.Data.CmsContentRevision? contentRevision = null)
        {
            _leadService = leadService;
            _cmsServiceService = cmsServiceService;
            _cmsCompanyProfileService = cmsCompanyProfileService;
            _cmsTestimonialService = cmsTestimonialService;
            _cmsBlogPostService = cmsBlogPostService;
            _cmsProjectService = cmsProjectService;
            _cmsTeamMemberService = cmsTeamMemberService;
            _cmsDivisionPageService = cmsDivisionPageService;
            _cmsSiteSettingService = cmsSiteSettingService;
            _cmsHomeSectionService = cmsHomeSectionService;
            _cmsContentPageService = cmsContentPageService;
            _cmsPageBandService = cmsPageBandService;
            _cache = cache;
            _logger = logger;
            _contentRevision = contentRevision;
        }

        public async Task<IActionResult> Index()
        {
            var cacheKey = $"{HomeViewModelCacheKey}:{_contentRevision?.Current ?? 0}";
            if (_cache.TryGetValue(cacheKey, out HomePageViewModel? cachedModel) && cachedModel is not null)
            {
                return View(cachedModel);
            }

            var model = new HomePageViewModel();

            // The homepage alone makes ~19 sequential CMS reads (hero, vision, pioneers,
            // pillars, divisions, AI/engineering, ethics, CTA, three list headers, services,
            // testimonials, projects, posts, company profile). None of it is per-visitor, so
            // caching the whole built view model for a short window is what keeps a repeat
            // homepage load from redoing all of that work on every single request — see
            // §8.1: do not turn these into Task.WhenAll on the same DbContext instead.
            try
            {
                model.Services = await _cmsServiceService.GetPublishedServicesByCompanySlugAsync("neurix");
                model.CompanyProfile = await _cmsCompanyProfileService.GetBySlugAsync("neurix");
                var hasPublishedProfile = model.CompanyProfile != null;
                model.Testimonials = await _cmsTestimonialService.GetFeaturedTestimonialsByCompanySlugAsync("neurix");
                model.FeaturedProjects = await _cmsProjectService.GetFeaturedProjectsByCompanySlugAsync("neurix", 4);
                model.FeaturedPosts = await _cmsBlogPostService.GetFeaturedPostsByCompanySlugAsync("neurix", 3);
                model.HeroSection = await _cmsHomeSectionService.GetHeroSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.HumanVisionSection = await _cmsHomeSectionService.GetHumanVisionSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.PioneersSection = await _cmsHomeSectionService.GetPioneersSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.PillarsSection = await _cmsHomeSectionService.GetPillarsSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.Pillars = await _cmsHomeSectionService.GetPillarItemsByCompanySlugAsync("neurix");
                model.DivisionsSection = await _cmsHomeSectionService.GetDivisionsSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.DivisionItems = await _cmsHomeSectionService.GetDivisionItemsByCompanySlugAsync("neurix");
                model.AiEngineeringSection = await _cmsHomeSectionService.GetAiEngineeringSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.AiEngineeringItems = await _cmsHomeSectionService.GetAiEngineeringItemsByCompanySlugAsync("neurix");
                model.EthicsSection = await _cmsHomeSectionService.GetEthicsSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.CtaSection = await _cmsHomeSectionService.GetCtaSectionByCompanySlugAsync("neurix", includeUnpublished: hasPublishedProfile);
                model.PortfolioHeader = await _cmsHomeSectionService.GetListSectionHeaderByCompanySlugAsync("neurix", CmsListSectionKeys.Portfolio, includeUnpublished: hasPublishedProfile);
                model.InsightsHeader = await _cmsHomeSectionService.GetListSectionHeaderByCompanySlugAsync("neurix", CmsListSectionKeys.Insights, includeUnpublished: hasPublishedProfile);
                model.TestimonialsHeader = await _cmsHomeSectionService.GetListSectionHeaderByCompanySlugAsync("neurix", CmsListSectionKeys.Testimonials, includeUnpublished: hasPublishedProfile);

                _cache.Set(cacheKey, model, HomeViewModelCacheDuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic CMS data for the homepage. Falling back to default state.");
            }

            return View(model);
        }

        public async Task<IActionResult> Privacy()
        {
            return View(await GetContentPageAsync("privacy"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Contact(string? type = null)
        {
            var model = new ContactViewModel();
            if (!string.IsNullOrEmpty(type) && type.Equals("quote", StringComparison.OrdinalIgnoreCase))
            {
                model.InquiryType = "Quote";
            }

            await LoadContactViewDataAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Capture the enquiry as a CRM lead. The CRM outage itself is fail-soft
                // everywhere else in this controller, but here a save failure must be
                // reported to the visitor instead of a false "thank you" — otherwise the
                // enquiry is silently lost with no way for anyone to follow up.
                var trackingId = Guid.NewGuid();
                try
                {
                    await _leadService.CaptureWebsiteEnquiryAsync(new WebsiteEnquiry
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        CompanyName = model.CompanyName,
                        InquiryType = model.InquiryType,
                        Message = model.Message
                    });

                    TempData["SuccessMessage"] = "Thank you for reaching out. Our team will contact you shortly.";
                    return RedirectToAction("Contact");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to capture a website enquiry as a CRM lead. TrackingId: {TrackingId}", trackingId);
                    ViewData["ErrorMessage"] = "Something went wrong on our end and your message was not sent. Please try again.";
                }
            }

            await LoadContactViewDataAsync();
            return View(model);
        }

        private async Task LoadContactViewDataAsync()
        {
            try
            {
                ViewData["CompanyProfile"] = await _cmsCompanyProfileService.GetBySlugAsync("neurix");
                var contactCopy = new CmsSiteCopy(await _cmsSiteSettingService.GetSettingsByCompanySlugAsync("neurix"));
                ViewData["PublicSiteCopy"] = contactCopy;
                ViewData["ContactHeadingEn"] = contactCopy.English("contact.heading", "Let's Build the Future Together");
                ViewData["ContactHeadingAr"] = contactCopy.Arabic("contact.heading", "لنبنِ المستقبل معاً");
                ViewData["ContentPage"] = await GetContentPageAsync("contact");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load company profile for Contact page.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            CmsCompanyProfileDetailDto? profile = null;
            try
            {
                profile = await _cmsCompanyProfileService.GetBySlugAsync("neurix");
                ViewData["TeamMembers"] = await _cmsTeamMemberService.GetPublishedMembersByCompanySlugAsync("neurix");
                ViewData["ContentPage"] = await GetContentPageAsync("about");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load company profile for About page.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.About);
            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Labs()
        {
            CmsDivisionPageDetailDto? division = null;
            try
            {
                division = await _cmsDivisionPageService.GetBySlugAsync("neurix", "labs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic division page for Labs.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Labs);
            return View(division);
        }

        [HttpGet]
        public async Task<IActionResult> Technology()
        {
            CmsDivisionPageDetailDto? division = null;
            try
            {
                division = await _cmsDivisionPageService.GetBySlugAsync("neurix", "technology");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic division page for Technology.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Technology);
            return View(division);
        }

        [HttpGet]
        public async Task<IActionResult> HQ()
        {
            CmsDivisionPageDetailDto? division = null;
            try
            {
                division = await _cmsDivisionPageService.GetBySlugAsync("neurix", "hq");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic division page for HQ.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Hq);
            return View(division);
        }

        [HttpGet]
        public async Task<IActionResult> Plus()
        {
            CmsDivisionPageDetailDto? division = null;
            try
            {
                division = await _cmsDivisionPageService.GetBySlugAsync("neurix", "plus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic division page for Plus.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Plus);
            return View(division);
        }

        [HttpGet]
        public async Task<IActionResult> Club()
        {
            CmsDivisionPageDetailDto? division = null;
            try
            {
                division = await _cmsDivisionPageService.GetBySlugAsync("neurix", "club");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic division page for Club.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Club);
            return View(division);
        }

        [HttpGet]
        public async Task<IActionResult> AI()
        {
            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Ai);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Insights()
        {
            IReadOnlyList<CmsBlogPostSummaryDto> posts = Array.Empty<CmsBlogPostSummaryDto>();
            try
            {
                posts = await _cmsBlogPostService.GetPublishedPostsByCompanySlugAsync("neurix");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load published blog posts.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Insights);
            // The per-card labels ("min read", the uncategorised stand-in) are the same copy
            // the homepage band uses, so both read the one CmsListSectionHeader row.
            ViewData["CardMeta"] = await GetListHeaderAsync(CmsListSectionKeys.Insights);
            return View(posts);
        }

        [HttpGet("insights/{slug}")]
        public async Task<IActionResult> InsightDetail(string slug)
        {
            CmsBlogPostDetailDto? post = null;
            try
            {
                post = await _cmsBlogPostService.GetBySlugAsync("neurix", slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load article with slug {Slug}.", slug);
            }

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Portfolio()
        {
            IReadOnlyList<CmsProjectSummaryDto> projects = Array.Empty<CmsProjectSummaryDto>();
            try
            {
                projects = await _cmsProjectService.GetPublishedProjectsByCompanySlugAsync("neurix");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load published portfolio projects.");
            }

            ViewData["PageBands"] = await GetPageBandsAsync(CmsPageKeys.Portfolio);
            ViewData["CardMeta"] = await GetListHeaderAsync(CmsListSectionKeys.Portfolio);
            return View(projects);
        }

        [HttpGet("portfolio/{slug}")]
        public async Task<IActionResult> ProjectDetail(string slug)
        {
            CmsProjectDetailDto? project = null;
            try
            {
                project = await _cmsProjectService.GetBySlugAsync("neurix", slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load project with slug {Slug}.", slug);
            }

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        /// <summary>
        /// The homepage list band's header row, reused by the matching list page so the
        /// per-card labels are edited in one place. Fail-soft like every other CMS read here.
        /// </summary>
        private async Task<CmsListSectionHeaderDto?> GetListHeaderAsync(string sectionKey)
        {
            try
            {
                return await _cmsHomeSectionService
                    .GetListSectionHeaderByCompanySlugAsync("neurix", sectionKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load the '{SectionKey}' list header.", sectionKey);
                return null;
            }
        }

        /// <summary>
        /// Loads the CMS bands of one secondary page. Fail-soft in the same way as
        /// <see cref="GetContentPageAsync"/>: every view keeps its original copy as a
        /// fallback, so an unreachable CMS database leaves the page looking untouched.
        /// </summary>
        private async Task<CmsPageContentDto> GetPageBandsAsync(string pageKey)
        {
            try
            {
                return await _cmsPageBandService.GetPageContentByCompanySlugAsync("neurix", pageKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load CMS bands for the '{PageKey}' page.", pageKey);
                return new CmsPageContentDto { PageKey = pageKey };
            }
        }

        [HttpGet]
        /// <summary>
        /// Loads a CMS content page for the public site. Fail-soft on purpose: the views all
        /// carry their original copy as a fallback, so a missing row or an unreachable CMS
        /// database leaves the page looking exactly as it did before Group 3.
        /// </summary>
        private async Task<CmsContentPageDetailDto?> GetContentPageAsync(string slug)
        {
            try
            {
                return await _cmsContentPageService.GetBySlugAsync("neurix", slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load CMS content page '{Slug}'.", slug);
                return null;
            }
        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        // Re-execute target for UseStatusCodePagesWithReExecute (Program.cs): turns a
        // body-less 404 into a real page instead of the browser's generic error card.
        [HttpGet]
        public IActionResult ShowStatusCode(int? code)
        {
            if (code == StatusCodes.Status404NotFound)
            {
                return View("NotFound");
            }

            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // Legal pages are CMS-managed content pages (slugs "terms"/"security"). Until an
        // editor publishes a body they intentionally render the same honest Coming Soon
        // card as Privacy — no placeholder policy text.
        public async Task<IActionResult> Terms()
        {
            return View(await GetContentPageAsync("terms"));
        }

        public async Task<IActionResult> Security()
        {
            return View(await GetContentPageAsync("security"));
        }
    }
}
