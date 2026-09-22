using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Neurix.Models;
using Xunit;

namespace Neurix.Tests
{
    public class E2EHomepageSectionsIntegrationTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsHomeSectionService _sectionService;
        private readonly Guid _profileId;

        public E2EHomepageSectionsIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _sectionService = new CmsHomeSectionService(_db, NullLogger<CmsHomeSectionService>.Instance);

            _profileId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = _profileId,
                Slug = "neurix",
                NameEn = "Neurix AI",
                NameAr = "نيوركس AI",
                IsPublished = true
            });
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task FullWorkflow_UpsertAllThreeSections_AndVerifyBilingualIntegrity()
        {
            // 1. Upsert Hero Section
            var heroDto = new CmsHeroSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Empowering Next-Gen AI",
                BadgeAr = "تمكين الذكاء المستقبلي",
                TitlePrefixEn = "Building Human-Centered ",
                TitleHighlightEn = "AI",
                TitleSuffixEn = " for Tomorrow",
                TitlePrefixAr = " ",
                TitleHighlightAr = "ذكاء اصطناعي",
                TitleSuffixAr = " محوره الإنسان، من أجل الغد",
                SubtitleEn = "Neurix AI transforms applied intelligence into production reality.",
                SubtitleAr = "تقوم نيوركس AI بتحويل الذكاء التطبيقي إلى واقع إنتاجي ملموس.",
                PrimaryButtonTextEn = "Explore Ecosystem",
                PrimaryButtonTextAr = "استكشف المنظومة",
                PrimaryButtonUrl = "#divisions",
                SecondaryButtonTextEn = "Contact Our Team",
                SecondaryButtonTextAr = "تواصل مع فريقنا",
                SecondaryButtonUrl = "/Home/Contact",
                Stat1Value = "12+",
                Stat1LabelEn = "Core AI Engines",
                Stat1LabelAr = "محركات ذكاء أساسية",
                Stat2Value = "4+",
                Stat2LabelEn = "Enterprise Deployments",
                Stat2LabelAr = "تطبيقات مؤسسية",
                Stat3Value = "9+",
                Stat3LabelEn = "Sovereign Frameworks",
                Stat3LabelAr = "أطر عمل سيادية",
                IsPublished = true
            };
            var heroResult = await _sectionService.UpsertHeroSectionAsync(heroDto);
            Assert.True(heroResult.Success);

            // 2. Upsert Human Vision Section
            var visionDto = new CmsHumanVisionSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Our Human-First Philosophy",
                BadgeAr = "فلسفتنا الإنسانية الرائدة",
                TitlePrefixEn = "Neurix AI ",
                TitleHighlightEn = "Human Vision",
                TitlePrefixAr = "رؤية نيوركس AI ",
                TitleHighlightAr = "الإنسانية",
                Paragraph1En = "Technology should elevate the human spirit rather than exploit vulnerabilities.",
                Paragraph1Ar = "يجب أن تكون التكنولوجيا رافداً لسمو الإنسان بدلاً من استغلال نقاط الضعف.",
                Paragraph2En = "We channel neural research directly toward meaningful real-world prosperity.",
                Paragraph2Ar = "نوجه أبحاثنا نحو رفاهية المجتمع وحماية الصحة الإدراكية للشباب.",
                ImagePath = "/images/Bringing Clarity to Complexity_1 2.png",
                ImageAltEn = "Neurix AI Dynamic Vision",
                ImageAltAr = "رؤية نيوركس الديناميكية",
                IsPublished = true
            };
            var visionResult = await _sectionService.UpsertHumanVisionSectionAsync(visionDto);
            Assert.True(visionResult.Success);

            // 3. Upsert Message to Pioneers Section
            var pioneersDto = new CmsPioneersSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "To Future Builders",
                BadgeAr = "إلى بناة المستقبل",
                TitlePrefixEn = "A Manifesto for ",
                TitleHighlightEn = "Global Innovators",
                TitlePrefixAr = "بيان إلى ",
                TitleHighlightAr = "رواد الابتكار العالميين",
                Paragraph1En = "To creators and engineers: Your code writes the constitution of tomorrow.",
                Paragraph1Ar = "إلى المطورين والمهندسين: أسطركم البرمجية هي دستور الغد.",
                Paragraph2En = "Build with empathy, construct with integrity.",
                Paragraph2Ar = "ابنوا بحس إنساني، وشيّدوا بنزاهة تامة.",
                ImagePath = "/images/Infrastructure.png",
                ImageAltEn = "Global Innovators",
                ImageAltAr = "رواد الابتكار العالميون",
                IsPublished = true
            };
            var pioneersResult = await _sectionService.UpsertPioneersSectionAsync(pioneersDto);
            Assert.True(pioneersResult.Success);

            // 4. Retrieve via Service and verify all properties
            var hero = await _sectionService.GetHeroSectionByCompanySlugAsync("neurix");
            var vision = await _sectionService.GetHumanVisionSectionByCompanySlugAsync("neurix");
            var pioneers = await _sectionService.GetPioneersSectionByCompanySlugAsync("neurix");

            Assert.NotNull(hero);
            Assert.Equal("Empowering Next-Gen AI", hero.BadgeEn);
            Assert.Equal("تمكين الذكاء المستقبلي", hero.BadgeAr);
            Assert.Equal("12+", hero.Stat1Value);
            Assert.Equal("محركات ذكاء أساسية", hero.Stat1LabelAr);

            Assert.NotNull(vision);
            Assert.Equal("Our Human-First Philosophy", vision.BadgeEn);
            Assert.Equal("فلسفتنا الإنسانية الرائدة", vision.BadgeAr);
            Assert.Equal("Technology should elevate the human spirit rather than exploit vulnerabilities.", vision.Paragraph1En);
            Assert.Equal("يجب أن تكون التكنولوجيا رافداً لسمو الإنسان بدلاً من استغلال نقاط الضعف.", vision.Paragraph1Ar);

            Assert.NotNull(pioneers);
            Assert.Equal("To Future Builders", pioneers.BadgeEn);
            Assert.Equal("إلى بناة المستقبل", pioneers.BadgeAr);
            Assert.Equal("A Manifesto for ", pioneers.TitlePrefixEn);
            Assert.Equal("Global Innovators", pioneers.TitleHighlightEn);
            Assert.Equal("بيان إلى ", pioneers.TitlePrefixAr);
            Assert.Equal("رواد الابتكار العالميين", pioneers.TitleHighlightAr);
        }

        [Fact]
        public async Task FullWorkflow_PillarsAndEthics_FullCrudAndOrderingWorkflow()
        {
            // 1. Upsert Pillars Header
            var pillarsHeaderDto = new CmsPillarsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                TitleEn = "Core Capabilities & Technological Pillars",
                TitleAr = "القدرات الأساسية والركائز التقنية",
                SubtitleEn = "Pioneering intelligent architectures across industries.",
                SubtitleAr = "ريادة البنى المعمارية الذكية عبر مختلف القطاعات.",
                IsPublished = true
            };
            var pillarsHeaderResult = await _sectionService.UpsertPillarsSectionAsync(pillarsHeaderDto);
            Assert.True(pillarsHeaderResult.Success);

            // 2. Create Multiple Pillar Items (CRUD - Create)
            var p1Dto = new CmsPillarItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "brain",
                TitleEn = "Advanced AI",
                TitleAr = "الذكاء الاصطناعي المتقدم",
                DescriptionEn = "Neural networks and cognitive systems.",
                DescriptionAr = "شبكات عصبية وأنظمة إدراكية.",
                DisplayOrder = 1,
                IsPublished = true
            };
            var p1Result = await _sectionService.CreatePillarItemAsync(p1Dto);
            Assert.True(p1Result.Success);
            var p1Id = p1Result.Value;

            var p2Dto = new CmsPillarItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "zap",
                TitleEn = "Quantum Acceleration",
                TitleAr = "التسريع الكمومي",
                DescriptionEn = "High performance quantum-inspired neural accelerators.",
                DescriptionAr = "مسرعات عصبية مستوحاة من الحوسبة الكمومية.",
                DisplayOrder = 2,
                IsPublished = true
            };
            var p2Result = await _sectionService.CreatePillarItemAsync(p2Dto);
            Assert.True(p2Result.Success);
            var p2Id = p2Result.Value;

            var p3Dto = new CmsPillarItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "shield-check",
                TitleEn = "Temporary Item to Delete",
                TitleAr = "عنصر مؤقت للحذف",
                DescriptionEn = "Will be deleted in test.",
                DescriptionAr = "سيتم حذفه في الاختبار.",
                DisplayOrder = 3,
                IsPublished = true
            };
            var p3Result = await _sectionService.CreatePillarItemAsync(p3Dto);
            Assert.True(p3Result.Success);
            var p3Id = p3Result.Value;

            // 3. Verify List (Read)
            var initialList = await _sectionService.GetPillarItemsByCompanySlugAsync("neurix");
            Assert.Equal(3, initialList.Count);
            Assert.Equal("Advanced AI", initialList[0].TitleEn);
            Assert.Equal("Quantum Acceleration", initialList[1].TitleEn);
            Assert.Equal("Temporary Item to Delete", initialList[2].TitleEn);

            // 4. Update an Item (CRUD - Update)
            var p2UpdateDto = new CmsPillarItemUpsertDto
            {
                Id = p2Id,
                CompanyProfileId = _profileId,
                IconName = "cpu",
                TitleEn = "Next-Gen Quantum Processing",
                TitleAr = "المعالجة الكمومية من الجيل القادم",
                DescriptionEn = "Ultra-low latency quantum neural processing units.",
                DescriptionAr = "وحدات معالجة عصبية كمومية فائقة السرعة.",
                DisplayOrder = 2,
                IsPublished = true
            };
            var updateResult = await _sectionService.UpdatePillarItemAsync(p2UpdateDto);
            Assert.True(updateResult.Success);

            // 5. Delete an Item (CRUD - Delete)
            var deleteResult = await _sectionService.DeletePillarItemAsync(p3Id);
            Assert.True(deleteResult.Success);

            // 6. Verify List after Update & Delete
            var updatedList = await _sectionService.GetPillarItemsByCompanySlugAsync("neurix");
            Assert.Equal(2, updatedList.Count);
            Assert.DoesNotContain(updatedList, p => p.Id == p3Id);
            Assert.Equal("Next-Gen Quantum Processing", updatedList[1].TitleEn);
            Assert.Equal("cpu", updatedList[1].IconName);
            Assert.Equal("المعالجة الكمومية من الجيل القادم", updatedList[1].TitleAr);

            // 7. Upsert Ethics / Vision Framework Section
            var ethicsDto = new CmsEthicsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Dynamic Strategy & Ethics",
                BadgeAr = "الاستراتيجية الديناميكية والأخلاقيات",
                TitlePrefixEn = "Pioneering Vision & ",
                TitleHighlightEn = "Architectural Execution",
                TitlePrefixAr = "رؤية رائدة و ",
                TitleHighlightAr = "تنفيذ معماري دقيق",
                DescriptionEn = "Transforming scientific research and algorithmic precision into sovereign systems.",
                DescriptionAr = "تحويل البحث العلمي والدقة الخوارزمية إلى منظومات سيادية.",
                TopImagePath = "/uploads/cms/sections/test-software.webp",
                TopImageAltEn = "Custom Software Workspace",
                TopImageAltAr = "مساحة عمل برمجية مخصصة",
                BottomImagePath = "/uploads/cms/sections/test-research.webp",
                BottomImageAltEn = "Advanced Research Lab",
                BottomImageAltAr = "مختبر أبحاث متقدم",
                BackgroundImagePath = "/images/Infrastructure.png",
                IsPublished = true
            };
            var ethicsResult = await _sectionService.UpsertEthicsSectionAsync(ethicsDto);
            Assert.True(ethicsResult.Success);

            // 8. Retrieve and Verify Ethics Section
            var ethicsSection = await _sectionService.GetEthicsSectionByCompanySlugAsync("neurix");
            Assert.NotNull(ethicsSection);
            Assert.Equal("Dynamic Strategy & Ethics", ethicsSection.BadgeEn);
            Assert.Equal("الاستراتيجية الديناميكية والأخلاقيات", ethicsSection.BadgeAr);
            Assert.Equal("Pioneering Vision & ", ethicsSection.TitlePrefixEn);
            Assert.Equal("Architectural Execution", ethicsSection.TitleHighlightEn);
            Assert.Equal("رؤية رائدة و ", ethicsSection.TitlePrefixAr);
            Assert.Equal("تنفيذ معماري دقيق", ethicsSection.TitleHighlightAr);
            Assert.Equal("/uploads/cms/sections/test-software.webp", ethicsSection.TopImagePath);
            Assert.Equal("Custom Software Workspace", ethicsSection.TopImageAltEn);
            Assert.Equal("مساحة عمل برمجية مخصصة", ethicsSection.TopImageAltAr);
            Assert.Equal("/uploads/cms/sections/test-research.webp", ethicsSection.BottomImagePath);

            // 9. Upsert Call To Action (CTA) Section (#cta)
            var ctaDto = new CmsCtaSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Scale Your Frontier",
                BadgeAr = "طوّر آفاقك المستقبلية",
                TitlePrefixEn = "Ready to build ",
                TitleHighlightEn = "autonomous intelligence?",
                TitlePrefixAr = "مستعد لبناء ",
                TitleHighlightAr = "أنظمة ذكاء ذاتية؟",
                ButtonTextEn = "Start Your Project",
                ButtonTextAr = "ابدأ مشروعك الآن",
                ButtonUrl = "/Home/Contact?type=quote",
                ContactEmail = "contact@neurix.ai",
                BackgroundImagePath = "/images/Contact Us (Home) 2.png",
                IsPublished = true
            };
            var ctaResult = await _sectionService.UpsertCtaSectionAsync(ctaDto);
            Assert.True(ctaResult.Success);

            // 10. Retrieve and Verify CTA Section
            var ctaSection = await _sectionService.GetCtaSectionByCompanySlugAsync("neurix");
            Assert.NotNull(ctaSection);
            Assert.Equal("Scale Your Frontier", ctaSection.BadgeEn);
            Assert.Equal("طوّر آفاقك المستقبلية", ctaSection.BadgeAr);
            Assert.Equal("Ready to build ", ctaSection.TitlePrefixEn);
            Assert.Equal("autonomous intelligence?", ctaSection.TitleHighlightEn);
            Assert.Equal("مستعد لبناء ", ctaSection.TitlePrefixAr);
            Assert.Equal("أنظمة ذكاء ذاتية؟", ctaSection.TitleHighlightAr);
            Assert.Equal("Start Your Project", ctaSection.ButtonTextEn);
            Assert.Equal("ابدأ مشروعك الآن", ctaSection.ButtonTextAr);
            Assert.Equal("/Home/Contact?type=quote", ctaSection.ButtonUrl);
            Assert.Equal("contact@neurix.ai", ctaSection.ContactEmail);
        }
    }
}