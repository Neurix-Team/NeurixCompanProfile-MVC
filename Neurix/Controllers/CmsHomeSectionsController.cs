using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.Common;
using Neurix.DAL.Models;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/home-sections")]
    public class CmsHomeSectionsController : Controller
    {
        private readonly ICmsHomeSectionService _homeSectionService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CmsHomeSectionsController> _logger;

        public CmsHomeSectionsController(
            ICmsHomeSectionService homeSectionService,
            ICmsCompanyProfileService profileService,
            IWebHostEnvironment environment,
            ILogger<CmsHomeSectionsController> logger)
        {
            _homeSectionService = homeSectionService;
            _profileService = profileService;
            _environment = environment;
            _logger = logger;
        }

        // ════════════════════════════════════════════════════════════════
        // INDEX / OVERVIEW
        // ════════════════════════════════════════════════════════════════
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedCompany = companyId.HasValue
                ? companies.FirstOrDefault(c => c.Id == companyId.Value)
                : companies.FirstOrDefault();

            var selectedId = selectedCompany?.Id ?? Guid.Empty;

            CmsHeroSectionDto? hero = null;
            CmsHumanVisionSectionDto? vision = null;
            CmsPioneersSectionDto? pioneers = null;
            CmsPillarsSectionDto? pillars = null;
            var pillarItems = Array.Empty<CmsPillarItemDto>() as System.Collections.Generic.IReadOnlyList<CmsPillarItemDto>;
            CmsDivisionsSectionDto? divisions = null;
            var divisionItems = Array.Empty<CmsDivisionItemDto>() as System.Collections.Generic.IReadOnlyList<CmsDivisionItemDto>;
            CmsAiEngineeringSectionDto? aiEngineering = null;
            var aiEngineeringItems = Array.Empty<CmsAiEngineeringItemDto>() as System.Collections.Generic.IReadOnlyList<CmsAiEngineeringItemDto>;
            var listHeaders = Array.Empty<CmsListSectionHeaderDto>() as System.Collections.Generic.IReadOnlyList<CmsListSectionHeaderDto>;
            CmsEthicsSectionDto? ethics = null;
            CmsCtaSectionDto? cta = null;

            if (selectedId != Guid.Empty)
            {
                hero = await _homeSectionService.GetHeroSectionByCompanyIdAsync(selectedId);
                vision = await _homeSectionService.GetHumanVisionSectionByCompanyIdAsync(selectedId);
                pioneers = await _homeSectionService.GetPioneersSectionByCompanyIdAsync(selectedId);
                pillars = await _homeSectionService.GetPillarsSectionByCompanyIdAsync(selectedId);
                pillarItems = await _homeSectionService.GetPillarItemsByCompanyIdAsync(selectedId, includeUnpublished: true);
                divisions = await _homeSectionService.GetDivisionsSectionByCompanyIdAsync(selectedId);
                divisionItems = await _homeSectionService.GetDivisionItemsByCompanyIdAsync(selectedId, includeUnpublished: true);
                aiEngineering = await _homeSectionService.GetAiEngineeringSectionByCompanyIdAsync(selectedId);
                aiEngineeringItems = await _homeSectionService.GetAiEngineeringItemsByCompanyIdAsync(selectedId, includeUnpublished: true);
                listHeaders = await _homeSectionService.GetListSectionHeadersByCompanyIdAsync(selectedId);
                ethics = await _homeSectionService.GetEthicsSectionByCompanyIdAsync(selectedId);
                cta = await _homeSectionService.GetCtaSectionByCompanyIdAsync(selectedId);
            }

            var model = new CmsHomeSectionsIndexViewModel
            {
                SelectedCompanyId = selectedId,
                Companies = companies,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                HeroSection = hero,
                HumanVisionSection = vision,
                PioneersSection = pioneers,
                PillarsSection = pillars,
                Pillars = pillarItems,
                DivisionsSection = divisions,
                DivisionItems = divisionItems,
                AiEngineeringSection = aiEngineering,
                AiEngineeringItems = aiEngineeringItems,
                ListSectionHeaders = listHeaders,
                EthicsSection = ethics,
                CtaSection = cta
            };

            return View(model);
        }

        // ════════════════════════════════════════════════════════════════
        // 1. EDIT HERO SECTION
        // ════════════════════════════════════════════════════════════════
        [HttpGet("hero")]
        public async Task<IActionResult> EditHero([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetHeroSectionByCompanyIdAsync(selectedId);

            var model = new CmsHeroSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                BadgeEn = existing?.BadgeEn ?? "Empowering the Future",
                BadgeAr = existing?.BadgeAr ?? "تمكين المستقبل",
                TitlePrefixEn = existing?.TitlePrefixEn ?? "Building Human-Centered ",
                TitleHighlightEn = existing?.TitleHighlightEn ?? "AI",
                TitleSuffixEn = existing?.TitleSuffixEn ?? " for Tomorrow",
                TitlePrefixAr = existing?.TitlePrefixAr ?? " ",
                TitleHighlightAr = existing?.TitleHighlightAr ?? "ذكاء اصطناعي",
                TitleSuffixAr = existing?.TitleSuffixAr ?? " محوره الإنسان، من أجل الغد",
                SubtitleEn = existing?.SubtitleEn ?? "Neurix AI is building smarter digital experiences for tomorrow.",
                SubtitleAr = existing?.SubtitleAr ?? "نصنع في نيوركس AI تجارب رقمية أذكى لمستقبلنا.",
                PrimaryButtonTextEn = existing?.PrimaryButtonTextEn ?? "Explore Our Divisions",
                PrimaryButtonTextAr = existing?.PrimaryButtonTextAr ?? "استكشف اقسامنا",
                PrimaryButtonUrl = existing?.PrimaryButtonUrl ?? "#divisions",
                SecondaryButtonTextEn = existing?.SecondaryButtonTextEn ?? "Get in Touch",
                SecondaryButtonTextAr = existing?.SecondaryButtonTextAr ?? "تواصل معنا",
                SecondaryButtonUrl = existing?.SecondaryButtonUrl ?? "/Home/Contact",
                Stat1Value = existing?.Stat1Value ?? "6+",
                Stat1LabelEn = existing?.Stat1LabelEn ?? "AI Systems",
                Stat1LabelAr = existing?.Stat1LabelAr ?? "أنظمة الذكاء الاصطناعي",
                Stat2Value = existing?.Stat2Value ?? "2+",
                Stat2LabelEn = existing?.Stat2LabelEn ?? "Enterprise Scale",
                Stat2LabelAr = existing?.Stat2LabelAr ?? "حلول المؤسسات",
                Stat3Value = existing?.Stat3Value ?? "5+",
                Stat3LabelEn = existing?.Stat3LabelEn ?? "Digital Platforms",
                Stat3LabelAr = existing?.Stat3LabelAr ?? "المنصات الرقمية",
                IsPublished = existing?.IsPublished ?? true
            };

            return View(model);
        }

        [HttpPost("hero")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHero(CmsHeroSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHeroCompanyOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsHeroSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitleSuffixEn = model.TitleSuffixEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                TitleSuffixAr = model.TitleSuffixAr ?? string.Empty,
                SubtitleEn = model.SubtitleEn ?? string.Empty,
                SubtitleAr = model.SubtitleAr ?? string.Empty,
                PrimaryButtonTextEn = model.PrimaryButtonTextEn ?? string.Empty,
                PrimaryButtonTextAr = model.PrimaryButtonTextAr ?? string.Empty,
                PrimaryButtonUrl = model.PrimaryButtonUrl ?? string.Empty,
                SecondaryButtonTextEn = model.SecondaryButtonTextEn ?? string.Empty,
                SecondaryButtonTextAr = model.SecondaryButtonTextAr ?? string.Empty,
                SecondaryButtonUrl = model.SecondaryButtonUrl ?? string.Empty,
                Stat1Value = model.Stat1Value ?? string.Empty,
                Stat1LabelEn = model.Stat1LabelEn ?? string.Empty,
                Stat1LabelAr = model.Stat1LabelAr ?? string.Empty,
                Stat2Value = model.Stat2Value ?? string.Empty,
                Stat2LabelEn = model.Stat2LabelEn ?? string.Empty,
                Stat2LabelAr = model.Stat2LabelAr ?? string.Empty,
                Stat3Value = model.Stat3Value ?? string.Empty,
                Stat3LabelEn = model.Stat3LabelEn ?? string.Empty,
                Stat3LabelAr = model.Stat3LabelAr ?? string.Empty,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpsertHeroSectionAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save Hero section.");
                await PopulateHeroCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Hero section updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        // ════════════════════════════════════════════════════════════════
        // 2. EDIT HUMAN VISION SECTION
        // ════════════════════════════════════════════════════════════════
        [HttpGet("human-vision")]
        public async Task<IActionResult> EditHumanVision([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetHumanVisionSectionByCompanyIdAsync(selectedId);

            var model = new CmsHumanVisionSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                BadgeEn = existing?.BadgeEn ?? "Our Core Philosophy",
                BadgeAr = existing?.BadgeAr ?? "فلسفتنا الأساسية",
                TitlePrefixEn = existing?.TitlePrefixEn ?? "Neurix AI ",
                TitleHighlightEn = existing?.TitleHighlightEn ?? "Human Vision",
                TitlePrefixAr = existing?.TitlePrefixAr ?? "رؤية نيوركس AI ",
                TitleHighlightAr = existing?.TitleHighlightAr ?? "الإنسانية",
                Paragraph1En = existing?.Paragraph1En ?? string.Empty,
                Paragraph1Ar = existing?.Paragraph1Ar ?? string.Empty,
                Paragraph2En = existing?.Paragraph2En ?? string.Empty,
                Paragraph2Ar = existing?.Paragraph2Ar ?? string.Empty,
                ImagePath = existing?.ImagePath ?? "/images/Bringing Clarity to Complexity_1 2.png",
                ImageAltEn = existing?.ImageAltEn ?? "Neurix AI Human Vision",
                ImageAltAr = existing?.ImageAltAr ?? "رؤية نيوركس AI الإنسانية",
                IsPublished = existing?.IsPublished ?? true
            };

            return View(model);
        }

        [HttpPost("human-vision")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHumanVision(CmsHumanVisionSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateHumanVisionCompanyOptionsAsync(model);
                return View(model);
            }

            string? uploadedImagePath = model.ImagePath;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    uploadedImagePath = await CmsMediaUploadHelper.SaveImageAsync(
                        model.ImageFile,
                        _environment,
                        "sections");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload Human Vision illustration image.");
                    ModelState.AddModelError("ImageFile", $"Image upload failed: {ex.Message}");
                    await PopulateHumanVisionCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsHumanVisionSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                Paragraph1En = model.Paragraph1En ?? string.Empty,
                Paragraph1Ar = model.Paragraph1Ar ?? string.Empty,
                Paragraph2En = model.Paragraph2En ?? string.Empty,
                Paragraph2Ar = model.Paragraph2Ar ?? string.Empty,
                ImagePath = uploadedImagePath,
                ImageAltEn = model.ImageAltEn ?? string.Empty,
                ImageAltAr = model.ImageAltAr ?? string.Empty,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpsertHumanVisionSectionAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save Human Vision section.");
                await PopulateHumanVisionCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Human Vision section updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        // ════════════════════════════════════════════════════════════════
        // 3. EDIT MESSAGE TO PIONEERS SECTION
        // ════════════════════════════════════════════════════════════════
        [HttpGet("pioneers")]
        public async Task<IActionResult> EditPioneers([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetPioneersSectionByCompanyIdAsync(selectedId);

            var model = new CmsPioneersSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                BadgeEn = existing?.BadgeEn ?? "For the Innovators",
                BadgeAr = existing?.BadgeAr ?? "للمبتكرين",
                TitlePrefixEn = existing?.TitlePrefixEn ?? "A Message to the ",
                TitleHighlightEn = existing?.TitleHighlightEn ?? "Pioneers of Innovation",
                TitlePrefixAr = existing?.TitlePrefixAr ?? "رسالة ",
                TitleHighlightAr = existing?.TitleHighlightAr ?? "لرواد الابتكار والأبداع",
                Paragraph1En = existing?.Paragraph1En ?? string.Empty,
                Paragraph1Ar = existing?.Paragraph1Ar ?? string.Empty,
                Paragraph2En = existing?.Paragraph2En ?? string.Empty,
                Paragraph2Ar = existing?.Paragraph2Ar ?? string.Empty,
                ImagePath = existing?.ImagePath ?? "/images/Infrastructure.png",
                ImageAltEn = existing?.ImageAltEn ?? "Pioneers of Innovation",
                ImageAltAr = existing?.ImageAltAr ?? "رواد الابتكار",
                IsPublished = existing?.IsPublished ?? true
            };

            return View(model);
        }

        [HttpPost("pioneers")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPioneers(CmsPioneersSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePioneersCompanyOptionsAsync(model);
                return View(model);
            }

            string? uploadedImagePath = model.ImagePath;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    uploadedImagePath = await CmsMediaUploadHelper.SaveImageAsync(
                        model.ImageFile,
                        _environment,
                        "sections");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload Pioneers illustration image.");
                    ModelState.AddModelError("ImageFile", $"Image upload failed: {ex.Message}");
                    await PopulatePioneersCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsPioneersSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                Paragraph1En = model.Paragraph1En ?? string.Empty,
                Paragraph1Ar = model.Paragraph1Ar ?? string.Empty,
                Paragraph2En = model.Paragraph2En ?? string.Empty,
                Paragraph2Ar = model.Paragraph2Ar ?? string.Empty,
                ImagePath = uploadedImagePath,
                ImageAltEn = model.ImageAltEn ?? string.Empty,
                ImageAltAr = model.ImageAltAr ?? string.Empty,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpsertPioneersSectionAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save Pioneers section.");
                await PopulatePioneersCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Message to Pioneers section updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        // ════════════════════════════════════════════════════════════════
        // 4. EDIT PILLARS SECTION & ITEMS (#pillars)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("pillars")]
        public async Task<IActionResult> EditPillars([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetPillarsSectionByCompanyIdAsync(selectedId);
            var items = await _homeSectionService.GetPillarItemsByCompanyIdAsync(selectedId, includeUnpublished: true);

            var model = new CmsPillarsSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                TitleEn = existing?.TitleEn ?? "Core Capabilities",
                TitleAr = existing?.TitleAr ?? "الركائز الاساسية",
                SubtitleEn = existing?.SubtitleEn ?? "The foundational pillars driving our intelligent digital ecosystems and enterprise solutions.",
                SubtitleAr = existing?.SubtitleAr ?? "الركائز الأساسية التي تقود منظوماتنا الرقمية الذكيّة وحلول المؤسسات.",
                IsPublished = existing?.IsPublished ?? true,
                PillarItems = items
            };

            return View(model);
        }

        [HttpPost("pillars")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPillars(CmsPillarsSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePillarsCompanyOptionsAsync(model);
                model.PillarItems = await _homeSectionService.GetPillarItemsByCompanyIdAsync(model.CompanyProfileId, includeUnpublished: true);
                return View(model);
            }

            var dto = new CmsPillarsSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                TitleEn = model.TitleEn?.Trim() ?? "Core Capabilities",
                TitleAr = model.TitleAr?.Trim() ?? "الركائز الاساسية",
                SubtitleEn = model.SubtitleEn ?? string.Empty,
                SubtitleAr = model.SubtitleAr ?? string.Empty,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpsertPillarsSectionAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save Pillars section.");
                await PopulatePillarsCompanyOptionsAsync(model);
                model.PillarItems = await _homeSectionService.GetPillarItemsByCompanyIdAsync(model.CompanyProfileId, includeUnpublished: true);
                return View(model);
            }

            TempData["SuccessMessage"] = "Core Capabilities section header updated successfully.";
            return RedirectToAction(nameof(EditPillars), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("pillars/create")]
        public async Task<IActionResult> CreatePillar([FromQuery] Guid companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsPillarItemFormViewModel
            {
                CompanyProfileId = companyId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == companyId
                }),
                IconName = "brain",
                IsPublished = true
            };

            return View("PillarItemForm", model);
        }

        [HttpPost("pillars/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePillar(CmsPillarItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePillarItemCompanyOptionsAsync(model);
                return View("PillarItemForm", model);
            }

            var dto = new CmsPillarItemUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                IconName = model.IconName,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                DescriptionEn = model.DescriptionEn ?? string.Empty,
                DescriptionAr = model.DescriptionAr ?? string.Empty,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.CreatePillarItemAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create pillar item.");
                await PopulatePillarItemCompanyOptionsAsync(model);
                return View("PillarItemForm", model);
            }

            TempData["SuccessMessage"] = "Capability pillar item created successfully.";
            return RedirectToAction(nameof(EditPillars), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("pillars/edit/{id:guid}")]
        public async Task<IActionResult> EditPillar(Guid id)
        {
            var existing = await _homeSectionService.GetPillarItemByIdAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Pillar item not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsPillarItemFormViewModel
            {
                Id = existing.Id,
                CompanyProfileId = existing.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == existing.CompanyProfileId
                }),
                IconName = existing.IconName,
                TitleEn = existing.TitleEn,
                TitleAr = existing.TitleAr,
                DescriptionEn = existing.DescriptionEn,
                DescriptionAr = existing.DescriptionAr,
                DisplayOrder = existing.DisplayOrder,
                IsPublished = existing.IsPublished
            };

            return View("PillarItemForm", model);
        }

        [HttpPost("pillars/edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPillar(Guid id, CmsPillarItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePillarItemCompanyOptionsAsync(model);
                return View("PillarItemForm", model);
            }

            var dto = new CmsPillarItemUpsertDto
            {
                Id = id,
                CompanyProfileId = model.CompanyProfileId,
                IconName = model.IconName,
                TitleEn = model.TitleEn,
                TitleAr = model.TitleAr,
                DescriptionEn = model.DescriptionEn ?? string.Empty,
                DescriptionAr = model.DescriptionAr ?? string.Empty,
                DisplayOrder = model.DisplayOrder,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpdatePillarItemAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update pillar item.");
                await PopulatePillarItemCompanyOptionsAsync(model);
                return View("PillarItemForm", model);
            }

            TempData["SuccessMessage"] = "Capability pillar item updated successfully.";
            return RedirectToAction(nameof(EditPillars), new { companyId = model.CompanyProfileId });
        }

        [HttpPost("pillars/delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePillar(Guid id, [FromForm] Guid companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _homeSectionService.DeletePillarItemAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete pillar item.";
            }
            else
            {
                TempData["SuccessMessage"] = "Capability pillar item deleted successfully.";
            }

            return RedirectToAction(nameof(EditPillars), new { companyId });
        }

        // ════════════════════════════════════════════════════════════════
        // 4b. EDIT OUR ECOSYSTEM / DIVISIONS SECTION (#divisions)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("divisions")]
        public async Task<IActionResult> EditDivisions([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetDivisionsSectionByCompanyIdAsync(selectedId);
            var items = await _homeSectionService.GetDivisionItemsByCompanyIdAsync(selectedId, includeUnpublished: true);

            var model = new CmsDivisionsSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                BadgeEn = existing?.BadgeEn ?? "Our Ecosystem",
                BadgeAr = existing?.BadgeAr ?? "منظومتنا",
                TitlePrefixEn = existing?.TitlePrefixEn ?? "Five subsidiaries. ",
                TitleHighlightEn = existing?.TitleHighlightEn ?? "One ecosystem.",
                TitlePrefixAr = existing?.TitlePrefixAr ?? "خمسة فروع. ",
                TitleHighlightAr = existing?.TitleHighlightAr ?? "منظومة واحدة.",
                DescriptionEn = existing?.DescriptionEn,
                DescriptionAr = existing?.DescriptionAr,
                IsPublished = existing?.IsPublished ?? true,
                DivisionItems = items
            };

            return View(model);
        }

        [HttpPost("divisions")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDivisions(CmsDivisionsSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDivisionsCompanyOptionsAsync(model);
                model.DivisionItems = await _homeSectionService.GetDivisionItemsByCompanyIdAsync(model.CompanyProfileId, includeUnpublished: true);
                return View(model);
            }

            var dto = new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                DescriptionEn = model.DescriptionEn ?? string.Empty,
                DescriptionAr = model.DescriptionAr ?? string.Empty,
                IsPublished = model.IsPublished
            };

            var result = await _homeSectionService.UpsertDivisionsSectionAsync(dto, GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save the Divisions section.");
                await PopulateDivisionsCompanyOptionsAsync(model);
                model.DivisionItems = await _homeSectionService.GetDivisionItemsByCompanyIdAsync(model.CompanyProfileId, includeUnpublished: true);
                return View(model);
            }

            TempData["SuccessMessage"] = "Our Ecosystem section header updated successfully.";
            return RedirectToAction(nameof(EditDivisions), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("divisions/create")]
        public async Task<IActionResult> CreateDivisionItem([FromQuery] Guid companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsDivisionItemFormViewModel
            {
                CompanyProfileId = companyId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == companyId
                }),
                IconName = "layers",
                IsPublished = true
            };

            return View("DivisionItemForm", model);
        }

        [HttpPost("divisions/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDivisionItem(CmsDivisionItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDivisionItemCompanyOptionsAsync(model);
                return View("DivisionItemForm", model);
            }

            var result = await _homeSectionService.CreateDivisionItemAsync(BuildDivisionItemDto(model), GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create the division card.");
                await PopulateDivisionItemCompanyOptionsAsync(model);
                return View("DivisionItemForm", model);
            }

            TempData["SuccessMessage"] = "Division card created successfully.";
            return RedirectToAction(nameof(EditDivisions), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("divisions/edit/{id:guid}")]
        public async Task<IActionResult> EditDivisionItem(Guid id)
        {
            var existing = await _homeSectionService.GetDivisionItemByIdAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Division card not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsDivisionItemFormViewModel
            {
                Id = existing.Id,
                CompanyProfileId = existing.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == existing.CompanyProfileId
                }),
                IconName = existing.IconName,
                TitleEn = existing.TitleEn,
                TitleAr = existing.TitleAr,
                DescriptionEn = existing.DescriptionEn,
                DescriptionAr = existing.DescriptionAr,
                LinkUrl = existing.LinkUrl,
                LinkTextEn = existing.LinkTextEn,
                LinkTextAr = existing.LinkTextAr,
                DisplayOrder = existing.DisplayOrder,
                IsPublished = existing.IsPublished
            };

            return View("DivisionItemForm", model);
        }

        [HttpPost("divisions/edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDivisionItem(Guid id, CmsDivisionItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDivisionItemCompanyOptionsAsync(model);
                return View("DivisionItemForm", model);
            }

            var dto = BuildDivisionItemDto(model);
            dto.Id = id;

            var result = await _homeSectionService.UpdateDivisionItemAsync(dto, GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update the division card.");
                await PopulateDivisionItemCompanyOptionsAsync(model);
                return View("DivisionItemForm", model);
            }

            TempData["SuccessMessage"] = "Division card updated successfully.";
            return RedirectToAction(nameof(EditDivisions), new { companyId = model.CompanyProfileId });
        }

        [HttpPost("divisions/delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDivisionItem(Guid id, [FromForm] Guid companyId)
        {
            var result = await _homeSectionService.DeleteDivisionItemAsync(id, GetCurrentUserId());

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete the division card.";
            }
            else
            {
                TempData["SuccessMessage"] = "Division card deleted successfully.";
            }

            return RedirectToAction(nameof(EditDivisions), new { companyId });
        }

        private static CmsDivisionItemUpsertDto BuildDivisionItemDto(CmsDivisionItemFormViewModel model) => new()
        {
            CompanyProfileId = model.CompanyProfileId,
            IconName = model.IconName,
            TitleEn = model.TitleEn,
            TitleAr = model.TitleAr,
            DescriptionEn = model.DescriptionEn ?? string.Empty,
            DescriptionAr = model.DescriptionAr ?? string.Empty,
            LinkUrl = model.LinkUrl,
            LinkTextEn = model.LinkTextEn ?? "Explore",
            LinkTextAr = model.LinkTextAr ?? "المزيد",
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished
        };

        private async Task PopulateDivisionsCompanyOptionsAsync(CmsDivisionsSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulateDivisionItemCompanyOptionsAsync(CmsDivisionItemFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        // ════════════════════════════════════════════════════════════════
        // 4c. EDIT AI & ENGINEERING SECTION (#services)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("ai-engineering")]
        public async Task<IActionResult> EditAiEngineering([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetAiEngineeringSectionByCompanyIdAsync(selectedId);
            var items = await _homeSectionService.GetAiEngineeringItemsByCompanyIdAsync(selectedId, includeUnpublished: true);

            var model = new CmsAiEngineeringSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                BadgeEn = existing?.BadgeEn ?? "Core Services",
                BadgeAr = existing?.BadgeAr ?? "خدماتنا الأساسية",
                TitleEn = existing?.TitleEn ?? "AI & Engineering",
                TitleAr = existing?.TitleAr ?? "الذكاء الاصطناعي والهندسة",
                IsPublished = existing?.IsPublished ?? true,
                ServiceItems = items
            };

            return View(model);
        }

        [HttpPost("ai-engineering")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAiEngineering(CmsAiEngineeringSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAiEngineeringCompanyOptionsAsync(model);
                model.ServiceItems = await _homeSectionService.GetAiEngineeringItemsByCompanyIdAsync(model.CompanyProfileId, includeUnpublished: true);
                return View(model);
            }

            var dto = new CmsAiEngineeringSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitleEn = model.TitleEn ?? string.Empty,
                TitleAr = model.TitleAr ?? string.Empty,
                IsPublished = model.IsPublished
            };

            var result = await _homeSectionService.UpsertAiEngineeringSectionAsync(dto, GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save the AI & Engineering section.");
                await PopulateAiEngineeringCompanyOptionsAsync(model);
                model.ServiceItems = await _homeSectionService.GetAiEngineeringItemsByCompanyIdAsync(model.CompanyProfileId, includeUnpublished: true);
                return View(model);
            }

            TempData["SuccessMessage"] = "AI & Engineering section header updated successfully.";
            return RedirectToAction(nameof(EditAiEngineering), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("ai-engineering/create")]
        public async Task<IActionResult> CreateAiEngineeringItem([FromQuery] Guid companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsAiEngineeringItemFormViewModel
            {
                CompanyProfileId = companyId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == companyId
                }),
                IconName = "code",
                IsPublished = true
            };

            return View("AiEngineeringItemForm", model);
        }

        [HttpPost("ai-engineering/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAiEngineeringItem(CmsAiEngineeringItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAiEngineeringItemCompanyOptionsAsync(model);
                return View("AiEngineeringItemForm", model);
            }

            var result = await _homeSectionService.CreateAiEngineeringItemAsync(BuildAiEngineeringItemDto(model), GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create the service card.");
                await PopulateAiEngineeringItemCompanyOptionsAsync(model);
                return View("AiEngineeringItemForm", model);
            }

            TempData["SuccessMessage"] = "Service card created successfully.";
            return RedirectToAction(nameof(EditAiEngineering), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("ai-engineering/edit/{id:guid}")]
        public async Task<IActionResult> EditAiEngineeringItem(Guid id)
        {
            var existing = await _homeSectionService.GetAiEngineeringItemByIdAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Service card not found.";
                return RedirectToAction(nameof(Index));
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var model = new CmsAiEngineeringItemFormViewModel
            {
                Id = existing.Id,
                CompanyProfileId = existing.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == existing.CompanyProfileId
                }),
                IconName = existing.IconName,
                TitleEn = existing.TitleEn,
                TitleAr = existing.TitleAr,
                DescriptionEn = existing.DescriptionEn,
                DescriptionAr = existing.DescriptionAr,
                DisplayOrder = existing.DisplayOrder,
                IsPublished = existing.IsPublished
            };

            return View("AiEngineeringItemForm", model);
        }

        [HttpPost("ai-engineering/edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAiEngineeringItem(Guid id, CmsAiEngineeringItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAiEngineeringItemCompanyOptionsAsync(model);
                return View("AiEngineeringItemForm", model);
            }

            var dto = BuildAiEngineeringItemDto(model);
            dto.Id = id;

            var result = await _homeSectionService.UpdateAiEngineeringItemAsync(dto, GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update the service card.");
                await PopulateAiEngineeringItemCompanyOptionsAsync(model);
                return View("AiEngineeringItemForm", model);
            }

            TempData["SuccessMessage"] = "Service card updated successfully.";
            return RedirectToAction(nameof(EditAiEngineering), new { companyId = model.CompanyProfileId });
        }

        [HttpPost("ai-engineering/delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAiEngineeringItem(Guid id, [FromForm] Guid companyId)
        {
            var result = await _homeSectionService.DeleteAiEngineeringItemAsync(id, GetCurrentUserId());

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete the service card.";
            }
            else
            {
                TempData["SuccessMessage"] = "Service card deleted successfully.";
            }

            return RedirectToAction(nameof(EditAiEngineering), new { companyId });
        }

        private static CmsAiEngineeringItemUpsertDto BuildAiEngineeringItemDto(CmsAiEngineeringItemFormViewModel model) => new()
        {
            CompanyProfileId = model.CompanyProfileId,
            IconName = model.IconName,
            TitleEn = model.TitleEn,
            TitleAr = model.TitleAr,
            DescriptionEn = model.DescriptionEn ?? string.Empty,
            DescriptionAr = model.DescriptionAr ?? string.Empty,
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished
        };

        private async Task PopulateAiEngineeringCompanyOptionsAsync(CmsAiEngineeringSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulateAiEngineeringItemCompanyOptionsAsync(CmsAiEngineeringItemFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        // ════════════════════════════════════════════════════════════════
        // 4d. EDIT LIST-SECTION HEADERS (#portfolio, #insights, #testimonials)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Static descriptors for the three homepage bands whose cards come from their own CMS
        /// area. Only the surrounding copy is edited here, so the form spells out where the items
        /// themselves are managed.
        /// </summary>
        private sealed record ListSectionDescriptor(
            string Label,
            string Anchor,
            string ItemsManagedAt,
            bool Subtitle,
            bool Button,
            bool ItemLink,
            bool CategoryFallback,
            bool ReadTime);

        private static readonly IReadOnlyDictionary<string, ListSectionDescriptor> ListSectionDescriptors =
            new Dictionary<string, ListSectionDescriptor>(StringComparer.OrdinalIgnoreCase)
            {
                [CmsListSectionKeys.Portfolio] = new("Featured Deployments", "#portfolio", "Projects",
                    Subtitle: false, Button: true, ItemLink: true, CategoryFallback: true, ReadTime: false),
                [CmsListSectionKeys.Insights] = new("Latest Insights", "#insights", "Blog Posts",
                    Subtitle: false, Button: true, ItemLink: true, CategoryFallback: true, ReadTime: true),
                [CmsListSectionKeys.Testimonials] = new("What Partners Say", "#testimonials", "Testimonials",
                    Subtitle: true, Button: false, ItemLink: false, CategoryFallback: false, ReadTime: false)
            };

        [HttpGet("list-header/{sectionKey}")]
        public async Task<IActionResult> EditListHeader(string sectionKey, [FromQuery] Guid? companyId)
        {
            if (!ListSectionDescriptors.TryGetValue(sectionKey ?? string.Empty, out var descriptor))
            {
                TempData["ErrorMessage"] = "Unknown homepage section.";
                return RedirectToAction(nameof(Index));
            }

            var key = sectionKey!.ToLowerInvariant();
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetListSectionHeaderByCompanyIdAsync(selectedId, key);

            var model = new CmsListSectionHeaderFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                SectionKey = key,
                SectionLabel = descriptor.Label,
                SectionAnchor = descriptor.Anchor,
                ItemsManagedAt = descriptor.ItemsManagedAt,
                SupportsSubtitle = descriptor.Subtitle,
                SupportsButton = descriptor.Button,
                SupportsItemLink = descriptor.ItemLink,
                SupportsCategoryFallback = descriptor.CategoryFallback,
                SupportsReadTime = descriptor.ReadTime,
                BadgeEn = existing?.BadgeEn,
                BadgeAr = existing?.BadgeAr,
                TitlePrefixEn = existing?.TitlePrefixEn,
                TitleHighlightEn = existing?.TitleHighlightEn,
                TitlePrefixAr = existing?.TitlePrefixAr,
                TitleHighlightAr = existing?.TitleHighlightAr,
                SubtitleEn = existing?.SubtitleEn,
                SubtitleAr = existing?.SubtitleAr,
                ItemLinkTextEn = existing?.ItemLinkTextEn,
                ItemLinkTextAr = existing?.ItemLinkTextAr,
                DefaultCategoryLabelEn = existing?.DefaultCategoryLabelEn,
                DefaultCategoryLabelAr = existing?.DefaultCategoryLabelAr,
                ReadTimeSuffixEn = existing?.ReadTimeSuffixEn,
                ReadTimeSuffixAr = existing?.ReadTimeSuffixAr,
                UndatedLabelEn = existing?.UndatedLabelEn,
                UndatedLabelAr = existing?.UndatedLabelAr,
                ButtonTextEn = existing?.ButtonTextEn,
                ButtonTextAr = existing?.ButtonTextAr,
                ButtonUrl = existing?.ButtonUrl,
                IsPublished = existing?.IsPublished ?? true
            };

            return View(model);
        }

        [HttpPost("list-header/{sectionKey}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditListHeader(string sectionKey, CmsListSectionHeaderFormViewModel model)
        {
            if (!ListSectionDescriptors.TryGetValue(sectionKey ?? string.Empty, out var descriptor))
            {
                TempData["ErrorMessage"] = "Unknown homepage section.";
                return RedirectToAction(nameof(Index));
            }

            var key = sectionKey!.ToLowerInvariant();
            model.SectionKey = key;
            model.SectionLabel = descriptor.Label;
            model.SectionAnchor = descriptor.Anchor;
            model.ItemsManagedAt = descriptor.ItemsManagedAt;
            model.SupportsSubtitle = descriptor.Subtitle;
            model.SupportsButton = descriptor.Button;
            model.SupportsItemLink = descriptor.ItemLink;
            model.SupportsCategoryFallback = descriptor.CategoryFallback;
            model.SupportsReadTime = descriptor.ReadTime;

            if (!ModelState.IsValid)
            {
                await PopulateListHeaderCompanyOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                SectionKey = key,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                SubtitleEn = descriptor.Subtitle ? model.SubtitleEn : null,
                SubtitleAr = descriptor.Subtitle ? model.SubtitleAr : null,
                ItemLinkTextEn = descriptor.ItemLink ? model.ItemLinkTextEn : null,
                ItemLinkTextAr = descriptor.ItemLink ? model.ItemLinkTextAr : null,
                DefaultCategoryLabelEn = descriptor.CategoryFallback ? model.DefaultCategoryLabelEn : null,
                DefaultCategoryLabelAr = descriptor.CategoryFallback ? model.DefaultCategoryLabelAr : null,
                ReadTimeSuffixEn = descriptor.ReadTime ? model.ReadTimeSuffixEn : null,
                ReadTimeSuffixAr = descriptor.ReadTime ? model.ReadTimeSuffixAr : null,
                UndatedLabelEn = descriptor.ReadTime ? model.UndatedLabelEn : null,
                UndatedLabelAr = descriptor.ReadTime ? model.UndatedLabelAr : null,
                ButtonTextEn = descriptor.Button ? model.ButtonTextEn : null,
                ButtonTextAr = descriptor.Button ? model.ButtonTextAr : null,
                ButtonUrl = descriptor.Button ? model.ButtonUrl : null,
                IsPublished = model.IsPublished
            };

            var result = await _homeSectionService.UpsertListSectionHeaderAsync(dto, GetCurrentUserId());

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save the section header.");
                await PopulateListHeaderCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"{descriptor.Label} section header updated successfully.";
            return RedirectToAction(nameof(EditListHeader), new { sectionKey = key, companyId = model.CompanyProfileId });
        }

        private async Task PopulateListHeaderCompanyOptionsAsync(CmsListSectionHeaderFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        // ════════════════════════════════════════════════════════════════
        // 5. EDIT ETHICS / VISION FRAMEWORK SECTION (#ethics)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("ethics")]
        public async Task<IActionResult> EditEthics([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var existing = await _homeSectionService.GetEthicsSectionByCompanyIdAsync(selectedId);

            var model = new CmsEthicsSectionFormViewModel
            {
                Id = existing?.Id,
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                BadgeEn = existing?.BadgeEn ?? "Operational Strategy",
                BadgeAr = existing?.BadgeAr ?? "الاستراتيجية التشغيلية",
                TitlePrefixEn = existing?.TitlePrefixEn ?? "Our Vision & ",
                TitleHighlightEn = existing?.TitleHighlightEn ?? "Implementation Framework",
                TitlePrefixAr = existing?.TitlePrefixAr ?? "رؤيتنا و ",
                TitleHighlightAr = existing?.TitleHighlightAr ?? "إطار عمل التنفيذ",
                DescriptionEn = existing?.DescriptionEn ?? string.Empty,
                DescriptionAr = existing?.DescriptionAr ?? string.Empty,
                TopImagePath = existing?.TopImagePath ?? "/images/Software.png",
                TopImageAltEn = existing?.TopImageAltEn ?? "Software Office Desk Setup",
                TopImageAltAr = existing?.TopImageAltAr ?? "بيئة تطوير البرمجيات",
                BottomImagePath = existing?.BottomImagePath ?? "/images/Research.png",
                BottomImageAltEn = existing?.BottomImageAltEn ?? "Pristine Laboratory Setup",
                BottomImageAltAr = existing?.BottomImageAltAr ?? "مختبر الأبحاث العلمية",
                BackgroundImagePath = existing?.BackgroundImagePath ?? "/images/Infrastructure.png",
                IsPublished = existing?.IsPublished ?? true
            };

            return View(model);
        }

        [HttpPost("ethics")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEthics(CmsEthicsSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEthicsCompanyOptionsAsync(model);
                return View(model);
            }

            string? uploadedTopImage = model.TopImagePath;
            if (model.TopImageFile != null && model.TopImageFile.Length > 0)
            {
                try
                {
                    uploadedTopImage = await CmsMediaUploadHelper.SaveImageAsync(
                        model.TopImageFile,
                        _environment,
                        "sections");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload Top (Software) image.");
                    ModelState.AddModelError("TopImageFile", $"Top image upload failed: {ex.Message}");
                    await PopulateEthicsCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            string? uploadedBottomImage = model.BottomImagePath;
            if (model.BottomImageFile != null && model.BottomImageFile.Length > 0)
            {
                try
                {
                    uploadedBottomImage = await CmsMediaUploadHelper.SaveImageAsync(
                        model.BottomImageFile,
                        _environment,
                        "sections");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload Bottom (Research) image.");
                    ModelState.AddModelError("BottomImageFile", $"Bottom image upload failed: {ex.Message}");
                    await PopulateEthicsCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsEthicsSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                DescriptionEn = model.DescriptionEn ?? string.Empty,
                DescriptionAr = model.DescriptionAr ?? string.Empty,
                TopImagePath = uploadedTopImage,
                TopImageAltEn = model.TopImageAltEn ?? string.Empty,
                TopImageAltAr = model.TopImageAltAr ?? string.Empty,
                BottomImagePath = uploadedBottomImage,
                BottomImageAltEn = model.BottomImageAltEn ?? string.Empty,
                BottomImageAltAr = model.BottomImageAltAr ?? string.Empty,
                BackgroundImagePath = model.BackgroundImagePath,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpsertEthicsSectionAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to save Ethics section.");
                await PopulateEthicsCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Vision & Implementation Framework section updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        // ════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════
        private async Task PopulateHeroCompanyOptionsAsync(CmsHeroSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulateHumanVisionCompanyOptionsAsync(CmsHumanVisionSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulatePioneersCompanyOptionsAsync(CmsPioneersSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulatePillarsCompanyOptionsAsync(CmsPillarsSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulatePillarItemCompanyOptionsAsync(CmsPillarItemFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private async Task PopulateEthicsCompanyOptionsAsync(CmsEthicsSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        // ═══════════════════════════════════════════════════════════
        // CALL TO ACTION SECTION (#cta)
        // ═══════════════════════════════════════════════════════════

        [HttpGet("cta")]
        public async Task<IActionResult> EditCta([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;
            var section = await _homeSectionService.GetCtaSectionByCompanyIdAsync(selectedId);

            var model = new CmsCtaSectionFormViewModel
            {
                CompanyProfileId = selectedId,
                Id = section?.Id,
                BadgeEn = section?.BadgeEn ?? "Let's Build Together",
                BadgeAr = section?.BadgeAr ?? "لنبني معاً",
                TitlePrefixEn = section?.TitlePrefixEn ?? "Ready to engineer",
                TitleHighlightEn = section?.TitleHighlightEn ?? " your future?",
                TitlePrefixAr = section?.TitlePrefixAr ?? "مستعد لهندسة",
                TitleHighlightAr = section?.TitleHighlightAr ?? " مستقبلك؟",
                ButtonTextEn = section?.ButtonTextEn ?? "Contact Us",
                ButtonTextAr = section?.ButtonTextAr ?? "تواصل معنا",
                ButtonUrl = section?.ButtonUrl ?? "/Home/Contact",
                ContactEmail = section?.ContactEmail ?? "contact@neurix.ai",
                BackgroundImagePath = section?.BackgroundImagePath ?? "/images/Contact Us (Home) 2.png",
                IsPublished = section?.IsPublished ?? true
            };

            await PopulateCtaCompanyOptionsAsync(model);
            return View(model);
        }

        [HttpPost("cta")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCta(CmsCtaSectionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCtaCompanyOptionsAsync(model);
                return View(model);
            }

            string? uploadedBackground = model.BackgroundImagePath;
            if (model.BackgroundImageFile != null && model.BackgroundImageFile.Length > 0)
            {
                try
                {
                    uploadedBackground = await CmsMediaUploadHelper.SaveImageAsync(
                        model.BackgroundImageFile,
                        _environment,
                        "sections");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload CTA Background image.");
                    ModelState.AddModelError("BackgroundImageFile", $"Background image upload failed: {ex.Message}");
                    await PopulateCtaCompanyOptionsAsync(model);
                    return View(model);
                }
            }

            var dto = new CmsCtaSectionUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                BadgeEn = model.BadgeEn ?? string.Empty,
                BadgeAr = model.BadgeAr ?? string.Empty,
                TitlePrefixEn = model.TitlePrefixEn ?? string.Empty,
                TitleHighlightEn = model.TitleHighlightEn ?? string.Empty,
                TitlePrefixAr = model.TitlePrefixAr ?? string.Empty,
                TitleHighlightAr = model.TitleHighlightAr ?? string.Empty,
                ButtonTextEn = model.ButtonTextEn ?? string.Empty,
                ButtonTextAr = model.ButtonTextAr ?? string.Empty,
                ButtonUrl = model.ButtonUrl ?? string.Empty,
                ContactEmail = model.ContactEmail ?? string.Empty,
                BackgroundImagePath = uploadedBackground,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _homeSectionService.UpsertCtaSectionAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update CTA section.");
                await PopulateCtaCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Call To Action section updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        private async Task PopulateCtaCompanyOptionsAsync(CmsCtaSectionFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
