using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    /// <summary>
    /// Seeds the default company profile (Neurix AI) and its services/social links if they do not already exist,
    /// and cleans up any legacy non-Neurix profiles.
    /// </summary>
    public class CmsSeeder : ICmsSeeder
    {
        private readonly CmsDbContext _db;

        public CmsSeeder(CmsDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            // ── Clean up any legacy Daleel records if they exist in the DB ──
            var legacyDaleel = await _db.CompanyProfiles.FirstOrDefaultAsync(p => p.Slug == "daleel");
            if (legacyDaleel != null)
            {
                var legacyServices = await _db.Services.Where(s => s.CompanyProfileId == legacyDaleel.Id).ToListAsync();
                var legacySocial = await _db.SocialLinks.Where(s => s.CompanyProfileId == legacyDaleel.Id).ToListAsync();

                if (legacyServices.Any()) _db.Services.RemoveRange(legacyServices);
                if (legacySocial.Any()) _db.SocialLinks.RemoveRange(legacySocial);
                _db.CompanyProfiles.Remove(legacyDaleel);
                await _db.SaveChangesAsync();
            }

            // ── Seed Company Profile: Neurix AI ──
            var neurixProfileId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var neurixProfile = await _db.CompanyProfiles.FirstOrDefaultAsync(p => p.Slug == "neurix");
            if (neurixProfile == null)
            {
                neurixProfile = new CmsCompanyProfile
                {
                    Id = neurixProfileId,
                    Slug = "neurix",
                    NameEn = "Neurix AI",
                    NameAr = "نيوركس AI",
                    LogoPath = "/images/neurix-logo.png",
                    FaviconPath = "/images/favicon.svg",
                    PrimaryColor = "#00C2D4",
                    AccentColor = "#5B5FEF",
                    TaglineEn = "Building Human-Centered AI for Tomorrow",
                    TaglineAr = "نصنع في نيوركس AI تجارب رقمية أذكى لمستقبلنا",
                    ShortDescriptionEn = "Neurix AI is building smarter digital experiences for tomorrow.",
                    ShortDescriptionAr = "نصنع في نيوركس AI تجارب رقمية أذكى لمستقبلنا.",
                    DescriptionEn = "Neurix AI is the technological engine and holding company of an integrated ecosystem. It provides scientific research, technical infrastructure, and implementation protocols, while active in managing government projects, university protocols, and international technology partnerships.",
                    DescriptionAr = "نيوركس AI هي المحرك التقني والشركة القابضة لمنظومة متكاملة، حيث تقدم الأبحاث العلمية والبنية التحتية التقنية وبروتوكولات التنفيذ، كما تنشط في إدارة المشاريع الحكومية والبروتوكولات الجامعية والشراكات التقنية الدولية.",
                    Email = "contact@neurix.ai",
                    Phone = "+1 (302) 602 4695",
                    AddressEn = "8th Floor, 98 Hassan El Maamoun St, Nasr City, Cairo, Egypt",
                    AddressAr = "الدور الثامن، 98 شارع حسن المأمون، الحي الأول، مدينة نصر، القاهرة، مصر",
                    Website = "https://neurix.uk",
                    IsPublished = true
                };
                _db.CompanyProfiles.Add(neurixProfile);
                await _db.SaveChangesAsync();
            }
            else
            {
                var updated = false;
                if (neurixProfile.Email == "neurix@aidaleel.com")
                {
                    neurixProfile.Email = "contact@neurix.ai";
                    updated = true;
                }
                if (string.IsNullOrEmpty(neurixProfile.PrimaryColor))
                {
                    neurixProfile.PrimaryColor = "#00C2D4";
                    updated = true;
                }
                if (string.IsNullOrEmpty(neurixProfile.AccentColor))
                {
                    neurixProfile.AccentColor = "#5B5FEF";
                    updated = true;
                }
                if (string.IsNullOrEmpty(neurixProfile.FaviconPath))
                {
                    neurixProfile.FaviconPath = "/images/favicon.svg";
                    updated = true;
                }
                if (updated)
                {
                    await _db.SaveChangesAsync();
                }
            }

            // ── Seed Services for Neurix AI ──
            if (!await _db.Services.AnyAsync(s => s.CompanyProfileId == neurixProfileId))
            {
                _db.Services.AddRange(
                    new CmsService
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "research-and-development",
                        NameEn = "Research & Development",
                        NameAr = "البحث والتطوير",
                        ShortDescriptionEn = "Driving analytical development, scientific research, and applied technology solutions through Neurix AI Lab.",
                        ShortDescriptionAr = "قيادة التطوير التحليلي، والبحث العلمي، وحلول التكنولوجيا التطبيقية من خلال مختبر نيوركس AI.",
                        IconName = "microscope",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsService
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "software-implementation",
                        NameEn = "Software Implementation",
                        NameAr = "تنفيذ البرمجيات",
                        ShortDescriptionEn = "Building integrated in-house applications and deploying research-based software systems through Neurix AI Tech.",
                        ShortDescriptionAr = "بناء التطبيقات البرمجية المتكاملة وتطوير الأنظمة القائمة على الأبحاث من خلال تقنية نيوركس AI.",
                        IconName = "code",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsService
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "professional-infrastructure",
                        NameEn = "Professional Infrastructure",
                        NameAr = "البنية التحتية المهنية",
                        ShortDescriptionEn = "Connecting professionals, experts, students, and remote talent through specialized communities and task-based work models.",
                        ShortDescriptionAr = "ربط المهنيين، الخبراء، الطلاب، والمواهب عن بعد عبر مجتمعات متخصصة ونماذج عمل قائمة على المهام.",
                        IconName = "users",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsService
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "strategic-engagement",
                        NameEn = "Strategic Engagement",
                        NameAr = "المشاركة الاستراتيجية",
                        ShortDescriptionEn = "Collaborating with governments, universities, and international organizations on high-impact digital initiatives.",
                        ShortDescriptionAr = "التعاون مع الحكومات والجامعات والمنظمات الدولية في المبادرات الرقمية ذات التأثير العالي.",
                        IconName = "zap",
                        DisplayOrder = 4,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Social Links for Neurix AI ──
            if (!await _db.SocialLinks.AnyAsync(s => s.CompanyProfileId == neurixProfileId))
            {
                _db.SocialLinks.AddRange(
                    new CmsSocialLink
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Platform = "linkedin",
                        DisplayName = "LinkedIn",
                        Url = "https://www.linkedin.com/company/neurixuk/",
                        IconName = "linkedin",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsSocialLink
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Platform = "twitter",
                        DisplayName = "Twitter / X",
                        Url = "https://x.com/NeurixAI",
                        IconName = "twitter",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsSocialLink
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Platform = "github",
                        DisplayName = "GitHub",
                        Url = "https://github.com/NeurixAI",
                        IconName = "github",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsSocialLink
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Platform = "youtube",
                        DisplayName = "YouTube",
                        Url = "https://youtube.com/@NeurixAI",
                        IconName = "youtube",
                        DisplayOrder = 4,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Site Settings for Neurix AI ──
            if (!await _db.SiteSettings.AnyAsync(s => s.CompanyProfileId == neurixProfileId))
            {
                _db.SiteSettings.AddRange(
                    new CmsSiteSetting
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Key = "seo.meta.description",
                        GroupName = "SEO",
                        Label = "Homepage Meta Description",
                        SettingType = "textarea",
                        ValueEn = "Neurix AI — Technology in service of humanity. Human-centered ecosystems across research, infrastructure, and strategic engagement.",
                        ValueAr = "نيوركس AI — التكنولوجيا في خدمة الإنسانية. منظومات تقنية محورية حول الإنسان تجمع بين البحث، البنية التحتية، والشراكات الاستراتيجية."
                    },
                    new CmsSiteSetting
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Key = "seo.site.title",
                        GroupName = "SEO",
                        Label = "Global Site Title Suffix",
                        SettingType = "text",
                        ValueEn = "Neurix AI Holding",
                        ValueAr = "نيوركس AI القابضة"
                    },
                    new CmsSiteSetting
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Key = "footer.copyright",
                        GroupName = "Footer",
                        Label = "Footer Copyright Text",
                        SettingType = "text",
                        ValueEn = "Neurix AI. All rights reserved.",
                        ValueAr = "نيوركس AI. جميع الحقوق محفوظة."
                    },
                    new CmsSiteSetting
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Key = "contact.heading",
                        GroupName = "Contact",
                        Label = "Contact Page Heading",
                        SettingType = "text",
                        ValueEn = "Let's Build the Future Together",
                        ValueAr = "لنبنِ المستقبل معاً"
                    }
                );
            }

            var sharedCopy = new (string Key, string Group, string Label, string En, string Ar)[]
            {
                ("navbar.cta", "General", "Navigation: Contact Button", "Start a Project", "ابدأ مشروعك"),
                ("contact.form.name", "Contact", "Contact Form: Full Name Label", "Full name *", "الاسم الكامل *"),
                ("contact.form.email", "Contact", "Contact Form: Email Label", "Work email *", "البريد الإلكتروني للعمل *"),
                ("contact.form.inquiry", "Contact", "Contact Form: Inquiry Label", "Type of inquiry *", "نوع الاستفسار *"),
                ("contact.form.company", "Contact", "Contact Form: Company Label", "Company (Optional)", "الشركة (اختياري)"),
                ("contact.form.message", "Contact", "Contact Form: Message Label", "How can we help? *", "كيف يمكننا مساعدتك؟ *"),
                ("contact.form.demo", "Contact", "Contact Form: Demo Option", "Demo", "عرض تجريبي"),
                ("contact.form.quote", "Contact", "Contact Form: Quote Option", "Quote", "تسعيرة"),
                ("contact.form.partnership", "Contact", "Contact Form: Partnership Option", "Partnership", "شراكة"),
                ("contact.form.support", "Contact", "Contact Form: Support Option", "Support", "دعم فني"),
                ("contact.form.response", "Contact", "Contact Form: Response Time", "We respond within 1 business day", "نرد خلال يوم عمل واحد"),
                ("contact.form.submit", "Contact", "Contact Form: Submit Button", "Submit Request", "إرسال الطلب"),
                ("footer.divisions.title", "Footer", "Footer: Divisions Heading", "Divisions", "الأقسام"),
                ("footer.division.labs", "Footer", "Footer: Labs Link", "Neurix AI Labs", "مختبرات نيوركس AI"),
                ("footer.division.technology", "Footer", "Footer: Technology Link", "Neurix AI Technology", "تقنية نيوركس AI"),
                ("footer.division.club", "Footer", "Footer: Club Link", "Neurix AI Club", "نادي نيوركس AI"),
                ("footer.division.plus", "Footer", "Footer: Plus Link", "Neurix AI Plus", "نيوركس AI بلس"),
                ("footer.division.hq", "Footer", "Footer: HQ Link", "Neurix AI HQ", "مقر نيوركس AI"),
                ("footer.company.title", "Footer", "Footer: Company Heading", "Company", "الشركة"),
                ("footer.contact.title", "Footer", "Footer: Contact Heading", "Contact", "تواصل معنا"),
                ("footer.office.primary.title", "Footer", "Footer: Primary Office Heading", "Office (Primary)", "المكتب (الرئيسي)"),
                ("footer.offices.more", "Footer", "Footer: Other Offices Toggle", "Read More Addresses", "قراءة المزيد من العناوين"),
                ("footer.office.uae.title", "Footer", "Footer: UAE Office Heading", "UAE Office", "مكتب الإمارات العربية المتحدة"),
                ("footer.office.uae.address", "Footer", "Footer: UAE Office Address", "2003-040 Aspin Commercial-2003, trade center first, Dubai, UAE", "مكتب 2003-040، برج أسبين التجاري، المركز التجاري الأول، دبي، الإمارات العربية المتحدة"),
                ("footer.office.nevis.title", "Footer", "Footer: Nevis Office Heading", "Saint Kitts & Nevis Office", "مكتب سانت كيتس ونيفيس"),
                ("footer.office.nevis.address", "Footer", "Footer: Nevis Office Address", "The Provident House, Central Government Road, Charlestown, Nevis, Saint Kitts and Nevis", "بروفيدنت هاوس، طريق الحكومة المركزية، تشارلز تاون، نيفيس، سانت كيتس ونيفيس"),
                ("footer.office.usa.title", "Footer", "Footer: USA Office Heading", "USA Office", "مكتب الولايات المتحدة الأمريكية"),
                ("footer.office.usa.address", "Footer", "Footer: USA Office Address", "262 Chapman Rd, Ste 240, Newark, New Castle County, Delaware, USA", "262 طريق تشابمان، جناح 240، نيوارك، مقاطعة نيو كاسل، ديلاوير، الولايات المتحدة الأمريكية"),
                ("footer.newsletter.title", "Footer", "Footer: Newsletter Heading", "Newsletter", "النشرة الإخبارية"),
                ("comingsoon.title", "General", "Coming Soon: Heading", "Coming Soon", "قريباً"),
                ("comingsoon.body", "General", "Coming Soon: Description", "We are engineering something extraordinary. This feature will be available in a future update.", "نحن نهندس شيئاً استثنائياً. ستكون هذه الميزة متاحة في تحديث مستقبلي."),
                ("notfound.title", "General", "404 Page: Heading", "Page Not Found", "الصفحة غير موجودة"),
                ("notfound.body", "General", "404 Page: Description", "The page you were looking for may have been moved, removed, or never existed. The link you followed might also be outdated.", "الصفحة التي تبحث عنها ربما تم نقلها أو حذفها أو لم تكن موجودة أصلاً. قد يكون الرابط الذي اتبعته قديماً أيضاً."),
                ("page.returnhome", "General", "Utility Pages: Return Home Button", "Return to Home", "العودة للرئيسية")
            };
            var existingCopyKeys = (await _db.SiteSettings.IgnoreQueryFilters()
                .Where(s => s.CompanyProfileId == neurixProfileId)
                .Select(s => s.Key)
                .ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var item in sharedCopy)
            {
                if (!existingCopyKeys.Add(item.Key)) continue;
                _db.SiteSettings.Add(new CmsSiteSetting
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    Key = item.Key,
                    GroupName = item.Group,
                    Label = item.Label,
                    SettingType = item.Key.EndsWith(".body") || item.Key.EndsWith(".address") ? "textarea" : "text",
                    ValueEn = item.En,
                    ValueAr = item.Ar
                });
            }

            // ── Seed additional SEO Open Graph settings (idempotent per-key check) ──
            if (!await _db.SiteSettings.AnyAsync(s => s.CompanyProfileId == neurixProfileId && s.Key == "seo.og.title"))
            {
                _db.SiteSettings.Add(new CmsSiteSetting
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    Key = "seo.og.title",
                    GroupName = "SEO",
                    Label = "Open Graph Title",
                    SettingType = "text",
                    ValueEn = "Neurix AI Holding",
                    ValueAr = "نيوركس AI القابضة"
                });
            }

            if (!await _db.SiteSettings.AnyAsync(s => s.CompanyProfileId == neurixProfileId && s.Key == "seo.og.description"))
            {
                _db.SiteSettings.Add(new CmsSiteSetting
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    Key = "seo.og.description",
                    GroupName = "SEO",
                    Label = "Open Graph Description",
                    SettingType = "textarea",
                    ValueEn = "Neurix AI — Technology in service of humanity. Human-centered ecosystems across research, infrastructure, and strategic engagement.",
                    ValueAr = "نيوركس AI — التكنولوجيا في خدمة الإنسانية. منظومات تقنية محورية حول الإنسان تجمع بين البحث، البنية التحتية، والشراكات الاستراتيجية."
                });
            }

            if (!await _db.SiteSettings.AnyAsync(s => s.CompanyProfileId == neurixProfileId && s.Key == "seo.og.image"))
            {
                _db.SiteSettings.Add(new CmsSiteSetting
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    Key = "seo.og.image",
                    GroupName = "SEO",
                    Label = "Open Graph Image URL",
                    SettingType = "url",
                    ValueEn = "/images/neurix-logo.png",
                    ValueAr = "/images/neurix-logo.png"
                });
            }

            // ── Seed Team Members for Neurix AI ──
            if (!await _db.TeamMembers.AnyAsync(m => m.CompanyProfileId == neurixProfileId))
            {
                _db.TeamMembers.AddRange(
                    new CmsTeamMember
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        NameEn = "Fady Ashraf",
                        NameAr = "فادي أشرف",
                        TitleEn = "Chief Executive Officer & Founder",
                        TitleAr = "الرئيس التنفيذي والمؤسس",
                        BioEn = "Architect of the Neurix AI holding framework, leading research and sovereign strategy.",
                        BioAr = "مؤسس منظومة نيوركس AI، يقود استراتيجيات البحث والشراكات السيادية.",
                        LinkedInUrl = "https://www.linkedin.com/in/fadyashraf/",
                        Email = "fady@neurix.ai",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsTeamMember
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        NameEn = "AI Research Team",
                        NameAr = "فريق أبحاث الذكاء الاصطناعي",
                        TitleEn = "Neurix Labs Research Division",
                        TitleAr = "قسم الأبحاث بمختبرات نيوركس",
                        BioEn = "Dedicated mathematicians, researchers, and engineers exploring applied intelligence.",
                        BioAr = "علماء رياضيات وباحثون ومهندسون يستكشفون الذكاء الاصطناعي التطبيقي.",
                        DisplayOrder = 2,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Blog Posts / Insights for Neurix AI ──
            if (!await _db.BlogPosts.AnyAsync(b => b.CompanyProfileId == neurixProfileId))
            {
                _db.BlogPosts.AddRange(
                    new CmsBlogPost
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "human-centered-ai-paradigm",
                        TitleEn = "The Human-Centered AI Paradigm: Beyond Raw Compute",
                        TitleAr = "نموذج الذكاء الاصطناعي المتمحور حول الإنسان: ما وراء الحوسبة المجردة",
                        SummaryEn = "Exploring how ethical frameworks, system safety, and purposeful design create lasting enterprise and civilizational value.",
                        SummaryAr = "استكشاف كيفية مساهمة الأطر الأخلاقية، وأمان الأنظمة، والتصميم الهادف في خلق قيمة مؤسسية وحضارية مستدامة.",
                        Category = "AI Research",
                        AuthorNameEn = "Neurix Labs",
                        AuthorNameAr = "مختبرات نيوركس",
                        ReadTimeMinutes = 6,
                        PublishedAtUtc = DateTime.UtcNow.AddDays(-10),
                        IsFeatured = true,
                        IsPublished = true
                    },
                    new CmsBlogPost
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "sovereign-tech-infrastructure",
                        TitleEn = "Building Sovereign Tech Infrastructure in Emerging Markets",
                        TitleAr = "بناء البنية التحتية التقنية السيادية في الأسواق الناشئة",
                        SummaryEn = "How decentralized talent networks and academic partnerships drive technological independence.",
                        SummaryAr = "كيف تقود شبكات المواهب اللامركزية والشراكات الأكاديمية الاستقلال التكنولوجي.",
                        Category = "Strategic Engagement",
                        AuthorNameEn = "Neurix Plus",
                        AuthorNameAr = "نيوركس بلس",
                        ReadTimeMinutes = 8,
                        PublishedAtUtc = DateTime.UtcNow.AddDays(-3),
                        IsFeatured = true,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Testimonials for Neurix AI ──
            if (!await _db.Testimonials.AnyAsync(t => t.CompanyProfileId == neurixProfileId))
            {
                _db.Testimonials.AddRange(
                    new CmsTestimonial
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        AuthorNameEn = "Dr. Ahmed Mansour",
                        AuthorNameAr = "د. أحمد منصور",
                        AuthorTitleEn = "Dean of Computer Science",
                        AuthorTitleAr = "عميد كلية علوم الحاسب",
                        CompanyName = "Assiut University",
                        QuoteEn = "Neurix AI's research protocol bridges academic theory with production software at a tier we rarely see in the region.",
                        QuoteAr = "بروتوكول أبحاث نيوركس AI يسد الفجوة بين النظريات الأكاديمية والبرمجيات الإنتاجية بمستوى استثنائي.",
                        Rating = 5,
                        DisplayOrder = 1,
                        IsFeatured = true,
                        IsPublished = true
                    },
                    new CmsTestimonial
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        AuthorNameEn = "Elena Rostova",
                        AuthorNameAr = "إيلينا روستوفا",
                        AuthorTitleEn = "Director of Strategic Innovation",
                        AuthorTitleAr = "مدير الابتكار الاستراتيجي",
                        CompanyName = "Global Digital Consortium",
                        QuoteEn = "Working with Neurix gave us the engineering rigor and ethical safety protocols essential for large-scale deployments.",
                        QuoteAr = "العمل مع نيوركس منحنا الدقة الهندسية وبروتوكولات الأمان الأخلاقي الضرورية للتطبيقات واسعة النطاق.",
                        Rating = 5,
                        DisplayOrder = 2,
                        IsFeatured = true,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Projects for Neurix AI ──
            if (!await _db.Projects.AnyAsync(p => p.CompanyProfileId == neurixProfileId))
            {
                _db.Projects.AddRange(
                    new CmsProject
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "neurix-core-platform",
                        TitleEn = "Neurix Ecosystem Core Platform",
                        TitleAr = "منصة النواة لمنظومة نيوركس",
                        SummaryEn = "Integrated multi-tenant enterprise holding architecture with high-speed microservices and real-time analytical telemetry.",
                        SummaryAr = "معمارية مؤسسية متعددة المستأجرين مع خدمات متناهية الصغر فائقة السرعة وقياس تحليلي فوري.",
                        Category = "AI Systems",
                        TechnologiesUsed = ".NET 10, ASP.NET Core, EF Core, Three.js, Tailwind CSS",
                        DisplayOrder = 1,
                        IsFeatured = true,
                        IsPublished = true
                    },
                    new CmsProject
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "sovereign-ai-research-lab",
                        TitleEn = "Sovereign AI Analytical Engine",
                        TitleAr = "المحرك التحليلي للذكاء الاصطناعي السيادي",
                        SummaryEn = "Custom LLM benchmarking and semantic evaluation suite tailored for Arabic and multilingual dialect comprehension.",
                        SummaryAr = "حزمة تقييم دلالي واختبار مخصص للنماذج اللغوية لفهم اللغة العربية واللهجات المتعددة.",
                        Category = "R&D",
                        TechnologiesUsed = "PyTorch, Python, FastAPI, CUDA",
                        DisplayOrder = 2,
                        IsFeatured = true,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Division Pages for Neurix AI ──
            if (!await _db.DivisionPages.AnyAsync(d => d.CompanyProfileId == neurixProfileId))
            {
                _db.DivisionPages.AddRange(
                    new CmsDivisionPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "labs",
                        HeroTitleEn = "Neurix AI Labs",
                        HeroTitleAr = "مختبرات نيوركس AI",
                        HeroSubtitleEn = "Primary R&D center building analytical tools and scientific foundations.",
                        HeroSubtitleAr = "المركز الرئيسي للبحث والتطوير وبناء الأدوات التحليلية والأسس العلمية.",
                        MissionEn = "Transforming fundamental mathematical insights and deep learning breakthroughs into resilient, reproducible technologies.",
                        MissionAr = "تحويل الرؤى الرياضية الأساسية واختراقات التعلم العميق إلى تقنيات مرنة وقابلة لإعادة الإنتاج.",
                        IsPublished = true
                    },
                    new CmsDivisionPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "technology",
                        HeroTitleEn = "Neurix AI Technology",
                        HeroTitleAr = "تكنولوجيا نيوركس AI",
                        HeroSubtitleEn = "Transforms Lab research into integrated applications and market-ready projects.",
                        HeroSubtitleAr = "تحول أبحاث المختبر إلى تطبيقات متكاملة ومشروعات جاهزة للسوق.",
                        MissionEn = "Engineering scalable, high-throughput software architectures that deploy research into mission-critical environments.",
                        MissionAr = "هندسة معماريات برمجية عالية الأداء وقابلة للتوسع لنشر مخرجات الأبحاث في بيئات العمل الحرجة.",
                        IsPublished = true
                    },
                    new CmsDivisionPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "hq",
                        HeroTitleEn = "Neurix AI HQ",
                        HeroTitleAr = "المقر الرئيسي لنيوركس AI",
                        HeroSubtitleEn = "The foundational management and governance stack powering every division.",
                        HeroSubtitleAr = "الحزمة الإدارية والتأسيسية والحوكمة التي تشغل كل قسم.",
                        MissionEn = "Providing cross-division coordination, compliance, sovereign alignment, and operational excellence.",
                        MissionAr = "توفير التنسيق بين الأقسام، والامتثال، والتوافق السيادي، والتميز التشغيلي.",
                        IsPublished = true
                    },
                    new CmsDivisionPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "plus",
                        HeroTitleEn = "Neurix AI Plus",
                        HeroTitleAr = "نيوركس AI بلس",
                        HeroSubtitleEn = "Strategic and sovereign engagements with governments, academia, and industry.",
                        HeroSubtitleAr = "شراكات استراتيجية وسيادية مع الجهات الحكومية والأكاديمية والصناعية.",
                        MissionEn = "Forging public-private protocols, university agreements, and national technological initiatives.",
                        MissionAr = "صياغة بروتوكولات الشراكة بين القطاعين العام والخاص، والاتفاقيات الجامعية، والمبادرات التقنية الوطنية.",
                        IsPublished = true
                    },
                    new CmsDivisionPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "club",
                        HeroTitleEn = "Neurix AI Club",
                        HeroTitleAr = "نادي نيوركس AI",
                        HeroSubtitleEn = "Professional networking and remote work infrastructure for global digital talent.",
                        HeroSubtitleAr = "بنية مهنية للتواصل والعمل عن بُعد للمواهب التقنية حول العالم.",
                        MissionEn = "Empowering developers, researchers, and students through task-based communities and collaborative incubation.",
                        MissionAr = "تمكين المطورين والباحثين والطلاب من خلال مجتمعات قائمة على المهام وحاضنات تعاونية.",
                        IsPublished = true
                    }
                );
            }

            // ── Seed the bands of the secondary public pages ──
            // Copy matches what the views currently hardcode, so seeding changes nothing
            // a visitor sees; it only makes the copy reachable from the dashboard.
            if (!await _db.PageBands.AnyAsync(b => b.CompanyProfileId == neurixProfileId))
            {
                _db.PageBands.AddRange(
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "hero",
                        BadgeEn = "Our Innovation Core",
                        BadgeAr = "جوهر ابتكارنا",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "overview",
                        BadgeEn = "Our Backbone",
                        BadgeAr = "عمودنا الفقري",
                        TitlePrefixEn = "The Research Core ",
                        TitlePrefixAr = "المحرك البحثي ",
                        TitleHighlightEn = "Behind the Ecosystem",
                        TitleHighlightAr = "وراء المنظومة",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "cards",
                        TitlePrefixEn = "Laboratory Capabilities",
                        TitlePrefixAr = "قدرات المختبر",
                        BodyEn = "A research and technology infrastructure that bridges scientific research, systems engineering, and applied innovation to support internal projects and sector-focused solutions.",
                        BodyAr = "بنية بحثية وتقنية تربط بين الأبحاث العلمية، هندسة الأنظمة، والابتكار التطبيقي لدعم المشروعات الداخلية والحلول القطاعية",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "hero",
                        BadgeEn = "Our Tech Division",
                        BadgeAr = "قسم التكنولوجيا",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "overview",
                        TitlePrefixEn = "What Neurix AI Tech Builds",
                        TitlePrefixAr = "ماذا تبني تكنولوجيا نيوركس AI؟",
                        BodyEn = "Transforming research-backed concepts into practical software applications, decision platforms, and scalable AI solutions.",
                        BodyAr = "تحويل المفاهيم القائمة على البحث إلى تطبيقات عملية، ومنصات اتخاذ قرار، وحلول ذكاء اصطناعي قابلة للتوسع.",
                        ImagePath = "/images/Artificial Intelligence Systems.png",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "cta",
                        BadgeEn = "Get Started",
                        BadgeAr = "ابدأ الآن",
                        TitlePrefixEn = "Build with Neurix AI Tech",
                        TitlePrefixAr = "ابدأ البناء مع تكنولوجيا نيوركس AI",
                        BodyEn = "Transform research-backed concepts into practical software applications and scalable technology opportunities.",
                        BodyAr = "حول المفاهيم القائمة على البحث إلى تطبيقات برمجية عملية وفرص تقنية قابلة للتوسع.",
                        ButtonTextEn = "Discuss Your Project",
                        ButtonTextAr = "ناقش مشروعك",
                        ButtonUrl = "/Home/Contact",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "hero",
                        BadgeEn = "Our Leadership",
                        BadgeAr = "قيادتنا",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "overview",
                        BadgeEn = "Central Hub",
                        BadgeAr = "المركز الأساسي",
                        TitlePrefixEn = "Where governance, ",
                        TitlePrefixAr = "حيث تلتقي الحوكمة ",
                        TitleHighlightEn = "operations, & strategy",
                        TitleHighlightAr = "والتشغيل والاستراتيجية",
                        TitleSuffixEn = " come together.",
                        TitleSuffixAr = " معاً.",
                        BodyEn = "Neurix AI HQ provides the internal structure that keeps the group organized, aligned, and operationally consistent through centralized management, oversight, and coordination.",
                        BodyAr = "يوفر المقر الرئيسي لنيوركس AI الهيكل الداخلي الذي يحافظ على تنظيم المجموعة واتساقها التشغيلي من خلال الإدارة المركزية والرقابة والتنسيق.",
                        ImagePath = "/images/HQ Role.png",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        TitlePrefixEn = "Core HQ Functions",
                        TitlePrefixAr = "الوظائف الأساسية للمقر الرئيسي",
                        BodyEn = "Neurix AI HQ connects specialized divisions through shared corporate functions and centralized governance.",
                        BodyAr = "يربط المقر الرئيسي لنيوركس AI بين أذرع المجموعة المتخصصة من خلال وظائف مؤسسية مشتركة وحوكمة مركزية.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "hero",
                        BadgeEn = "Strategic Division",
                        BadgeAr = "القسم الاستراتيجي",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "overview",
                        TitlePrefixEn = "The Scope of Neurix AI Plus",
                        TitlePrefixAr = "نطاق عمل نيوركس AI بلس",
                        BodyEn = "Neurix AI Plus operates through three strategic pillars that define its role within the broader Neurix ecosystem and support institutional collaboration and international expansion.",
                        BodyAr = "ترتكز نيوركس AI بلس على ثلاثة مسارات استراتيجية تحدد دورها داخل منظومة نيوركس وتدعم التعاون المؤسسي والتوسع الدولي.",
                        ImagePath = "/images/International Strategic Partnerships.png",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "cards",
                        TitlePrefixEn = "Strategic Partnership Pathways",
                        TitlePrefixAr = "مسارات الشراكة الاستراتيجية",
                        BodyEn = "Neurix AI Plus supports high-level cooperation with government entities, academic institutions, industrial partners, and strategic international organizations.",
                        BodyAr = "تدعم نيوركس AI بلس التعاون رفيع المستوى مع الجهات الحكومية والمؤسسات الأكاديمية والشركاء الصناعيين والمنظمات الدولية الاستراتيجية.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "hero",
                        BadgeEn = "Our Global Community",
                        BadgeAr = "مجتمعنا العالمي",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "overview",
                        BadgeEn = "Your Journey",
                        BadgeAr = "رحلتك",
                        TitlePrefixEn = "Find Your ",
                        TitlePrefixAr = "اعثر على ",
                        TitleHighlightEn = "Path",
                        TitleHighlightAr = "مسارك",
                        BodyEn = "Neurix AI Club brings together students, professionals, experts, and digital talent through specialized communities, technical collaboration, and meaningful work opportunities.",
                        BodyAr = "يجمع نادي نيوركس AI بين الطلاب والمهنيين والخبراء والمواهب الرقمية من خلال مجتمعات متخصصة وتعاون تقني فعال وفرص عمل ذات قيمة.",
                        ImagePath = "/images/Infrastructure.png",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "cards",
                        TitlePrefixEn = "What Neurix AI Club Offers",
                        TitlePrefixAr = "ماذا يقدم نادي نيوركس AI؟",
                        BodyEn = "Membership gives you access to a connected professional ecosystem built for growth, collaboration, and opportunity.",
                        BodyAr = "تمنحك العضوية وصولاً إلى منظومة مهنية مترابطة مصممة لدعم النمو والتعاون وفتح آفاق الفرص.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "ai",
                        BandKey = "hero",
                        BadgeEn = "Core Technologies",
                        BadgeAr = "التقنيات الأساسية",
                        TitlePrefixEn = "AI is embedded ",
                        TitlePrefixAr = "الذكاء الاصطناعي مدمج ",
                        TitleHighlightEn = "across Neurix AI.",
                        TitleHighlightAr = "عبر نيوريكس AI.",
                        BodyEn = "According to the corporate structure, AI is not a standalone subsidiary. AI capabilities are implemented within Neurix AI Labs and Neurix Technology to deliver verified-data processing, smart analytical tools, and practical human-centered solutions.",
                        BodyAr = "وفقاً للهيكل المؤسسي، لا يمثل الذكاء الاصطناعي فرعاً مستقلاً. يتم تنفيذ قدراته ضمن Neurix AI Labs وNeurix AI Technology لتقديم معالجة بيانات موثقة وأدوات تحليل ذكية وحلول عملية متمحورة حول الإنسان.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "insights",
                        BandKey = "hero",
                        BadgeEn = "Thought Leadership & Research",
                        BadgeAr = "الريادة الفكرية والأبحاث",
                        TitlePrefixEn = "Insights & ",
                        TitlePrefixAr = "رؤى و ",
                        TitleHighlightEn = "Publications",
                        TitleHighlightAr = "منشورات",
                        BodyEn = "Explore the latest scientific breakthroughs, architectural paradigms, and strategic perspectives from the Neurix AI ecosystem.",
                        BodyAr = "استكشف أحدث الاختراقات العلمية، والمعماريات البرمجية، والرؤى الاستراتيجية الصادرة عن منظومة نيوركس AI.",
                        ItemLinkTextEn = "Read Article",
                        ItemLinkTextAr = "اقرأ المقال",
                        EmptyStateEn = "No insights published yet. Check back soon for new research papers.",
                        EmptyStateAr = "لم تُنشر أي مقالات بعد. يرجى المتابعة لاحقاً للاطلاع على أوراق البحث الجديدة.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "portfolio",
                        BandKey = "hero",
                        BadgeEn = "Engineering Portfolio",
                        BadgeAr = "معرض الأعمال الهندسية",
                        TitlePrefixEn = "Featured ",
                        TitlePrefixAr = "المشاريع ",
                        TitleHighlightEn = "Projects",
                        TitleHighlightAr = "المميزة",
                        BodyEn = "Production-grade AI architectures, sovereign research engines, and high-throughput software systems deployed across sectors.",
                        BodyAr = "معماريات ذكاء اصطناعي إنتاجية، ومحركات بحثية سيادية، وأنظمة برمجية فائقة السرعة نُشرت عبر مختلف القطاعات.",
                        ItemLinkTextEn = "View Case Study",
                        ItemLinkTextAr = "عرض دراسة الحالة",
                        EmptyStateEn = "No projects available at this moment. Check back shortly.",
                        EmptyStateAr = "لا توجد مشروعات معروضة حالياً. يرجى المتابعة قريباً.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        TitlePrefixEn = "What We Stand For",
                        TitlePrefixAr = "ما الذي نؤمن به",
                        BodyEn = "Five foundational principles that govern every line of code, every architectural decision, and guide our entire workflow.",
                        BodyAr = "خمسة مبادئ أساسية تحكم كل سطر كود، وكل قرار معماري، وتوجه مسار عملنا بالكامل.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        BadgeEn = "Organizational Structure",
                        BadgeAr = "الهيكل التنظيمي",
                        TitlePrefixEn = "Five pillars.",
                        TitlePrefixAr = "خمس ركائز.",
                        TitleHighlightEn = "One vision.",
                        TitleHighlightAr = "رؤية واحدة.",
                        BodyEn = "Each division operates with full autonomy under unified strategic direction from the CEO.",
                        BodyAr = "يعمل كل قسم باستقلالية كاملة تحت التوجيه الاستراتيجي الموحد من الرئيس التنفيذي.",
                        ItemLinkTextEn = "Explore",
                        ItemLinkTextAr = "المزيد",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "team",
                        BadgeEn = "Leadership & Researchers",
                        BadgeAr = "القيادة والباحثون",
                        TitlePrefixEn = "The Minds Behind Neurix",
                        TitlePrefixAr = "العقول وراء نيوركس",
                        BodyEn = "A multidisciplinary collective of AI scientists, systems architects, and sovereign technology strategists.",
                        BodyAr = "فريق متعدد التخصصات من علماء الذكاء الاصطناعي ومهندسي الأنظمة وخبراء التقنية السيادية.",
                        DisplayOrder = 2,
                        IsPublished = true
                    }
                );

                _db.PageBandItems.AddRange(
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "hero",
                        TitleEn = "Research",
                        TitleAr = "البحث",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "hero",
                        TitleEn = "Development",
                        TitleAr = "التطوير",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "hero",
                        TitleEn = "AI & ML",
                        TitleAr = "الذكاء الاصطناعي",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "overview",
                        TitleEn = "The Research Core Behind the Ecosystem",
                        TitleAr = "The Research Core Behind the Ecosystem",
                        ImagePath = "/images/The Research Core Behind the Ecosystem.png",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "overview",
                        TitleEn = "Artificial Intelligence Research",
                        TitleAr = "Artificial Intelligence Research",
                        ImagePath = "/images/Artificial Intelligence Research.png",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "overview",
                        TitleEn = "Big Data & Advanced Analytics",
                        TitleAr = "Big Data & Advanced Analytics",
                        ImagePath = "/images/Big Data & Advanced Analytics.png",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "overview",
                        TitleEn = "Data Visualization & Analytics",
                        TitleAr = "Data Visualization & Analytics",
                        ImagePath = "/images/Data Visualization & Analytics.png",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "cards",
                        IconName = "bar-chart-2",
                        TitleEn = "Data Visualization & Analytics",
                        TitleAr = "تصور البيانات وتحليلها",
                        DescriptionEn = "We transform complex data into intuitive, interactive visual experiences that simplify understanding and accelerate decision-making.",
                        DescriptionAr = "نحول البيانات المعقدة إلى تجارب بصرية تفاعلية تساعد على تبسيط الفهم وتسريع اتخاذ القرار",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "cards",
                        IconName = "brain-circuit",
                        TitleEn = "User & System Intelligence",
                        TitleAr = "ذكاء المستخدم والأنظمة",
                        DescriptionEn = "We analyze user behavior and system interactions to develop intelligent solutions that are efficient, relevant, and impact-driven",
                        DescriptionAr = "نحلل سلوك المستخدم وتفاعلات الأنظمة لتطوير حلول ذكية أكثر كفاءة وارتباطًا بالاحتياجات الواقعية",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "cards",
                        IconName = "cpu",
                        TitleEn = "Artificial Intelligence Research",
                        TitleAr = "أبحاث الذكاء الاصطناعي",
                        DescriptionEn = "We develop and experiment with AI and machine learning models to build systems capable of understanding, learning, and making intelligent decisions.",
                        DescriptionAr = "نطوّر ونختبر نماذج الذكاء الاصطناعي والتعلّم الآلي لبناء أنظمة قادرة على الفهم والتعلّم واتخاذ قرارات ذكية",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "labs",
                        BandKey = "cards",
                        IconName = "database",
                        TitleEn = "Big Data & Advanced Analytics",
                        TitleAr = "البيانات الضخمة والتحليلات",
                        DescriptionEn = "We process large-scale datasets to uncover patterns, insights, and operational signals that enhance system intelligence and performance.",
                        DescriptionAr = "نعالج مجموعات البيانات واسعة النطاق لاستخراج الأنماط والرؤى التي تعزز كفاءة الأنظمة وذكاءها التشغيلي",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "hero",
                        TitleEn = "Data Platforms",
                        TitleAr = "منصات البيانات",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "hero",
                        TitleEn = "Integrated Applications",
                        TitleAr = "تطبيقات متكاملة",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "hero",
                        TitleEn = "AI Systems",
                        TitleAr = "أنظمة الذكاء الاصطناعي",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "overview",
                        IconName = "database",
                        TitleEn = "Data & Decision Platforms",
                        TitleAr = "منصات البيانات واتخاذ القرار",
                        DescriptionEn = "Unified platforms that transform complex data into real-time insights, enabling faster, smarter, and more accurate decision-making.",
                        DescriptionAr = "منصات موحدة تحول البيانات المعقدة إلى رؤى لحظية تدعم اتخاذ قرارات أسرع وأكثر دقة",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "overview",
                        IconName = "app-window",
                        TitleEn = "Integrated Applications",
                        TitleAr = "تطبيقات متكاملة",
                        DescriptionEn = "Connected applications designed for real-world environments, enabling intelligent operations, seamless user experiences, and scalable performance.",
                        DescriptionAr = "تطبيقات مترابطة مصممة للاستخدام الواقعي، توفر عمليات ذكية وتجارب استخدام سلسة وقابلة للتوسع",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "technology",
                        BandKey = "overview",
                        IconName = "cpu",
                        TitleEn = "Artificial Intelligence Systems",
                        TitleAr = "أنظمة الذكاء الاصطناعي",
                        DescriptionEn = "Intelligent systems capable of processing and understanding data to support automation, operational efficiency, and intelligent decision-making.",
                        DescriptionAr = "أنظمة ذكية قادرة على معالجة البيانات وفهمها واتخاذ قرارات تدعم الأتمتة والعمليات التشغيلية الحديثة.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "hero",
                        TitleEn = "Operations",
                        TitleAr = "العمليات",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "hero",
                        TitleEn = "Finance",
                        TitleAr = "الشؤون المالية",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "hero",
                        TitleEn = "Human Resources",
                        TitleAr = "الموارد البشرية",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "hero",
                        TitleEn = "Brand Governance",
                        TitleAr = "حوكمة العلامة",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "hero",
                        TitleEn = "Group Alignment",
                        TitleAr = "تنسيق المجموعة",
                        DisplayOrder = 4,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        IconName = "building",
                        TitleEn = "Group Governance",
                        TitleAr = "حوكمة المجموعة",
                        DescriptionEn = "Parent-level oversight, strategic direction, and alignment across all Neurix AI divisions.",
                        DescriptionAr = "إشراف عام وتوجيه استراتيجي وضمان الاتساق عبر جميع أقسام نيوركس AI.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        IconName = "settings",
                        TitleEn = "Operations Management",
                        TitleAr = "إدارة العمليات",
                        DescriptionEn = "Coordinating internal workflows, execution standards, reporting structures, and operational continuity.",
                        DescriptionAr = "تنسيق سير العمل الداخلي ومعايير التنفيذ وآليات المتابعة والاستمرارية التشغيلية.",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        IconName = "wallet",
                        TitleEn = "Finance & Administration",
                        TitleAr = "الشؤون المالية والإدارية",
                        DescriptionEn = "Managing financial planning, administrative controls, and group-level resource coordination.",
                        DescriptionAr = "إدارة التخطيط المالي والضوابط الإدارية وتنسيق الموارد على مستوى المجموعة.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        IconName = "users",
                        TitleEn = "Human Resources",
                        TitleAr = "الموارد البشرية",
                        DescriptionEn = "Supporting recruitment, talent development, internal policies, and people operations.",
                        DescriptionAr = "دعم التوظيف وتطوير الفرق والسياسات الداخلية وإدارة شؤون الأفراد.",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        IconName = "badge-check",
                        TitleEn = "Brand Governance",
                        TitleAr = "حوكمة العلامة التجارية",
                        DescriptionEn = "Maintaining communication standards, identity guidelines, and consistency across all divisions.",
                        DescriptionAr = "الحفاظ على معايير التواصل وإرشادات الهوية واتساق العلامة عبر جميع الأقسام.",
                        DisplayOrder = 4,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "hq",
                        BandKey = "cards",
                        IconName = "git-merge",
                        TitleEn = "Cross-Division Coordination",
                        TitleAr = "تنسيق أذرع المجموعة",
                        DescriptionEn = "Ensuring Labs, Technology, Club, Plus, and future branches operate as one connected ecosystem.",
                        DescriptionAr = "ضمان عمل المختبرات والتقنيات والنادي وبلص والمقر الرئيسي كمنظومة واحدة مترابطة.",
                        DisplayOrder = 5,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "overview",
                        TitleEn = "Academic & Industrial Protocols",
                        TitleAr = "البروتوكولات الأكاديمية والصناعية",
                        DescriptionEn = "Building partnerships with universities and industrial institutions to support scientific exchange, joint scholarship programs, and advanced artificial intelligence initiatives.",
                        DescriptionAr = "تطوير شراكات مع الجامعات والمؤسسات الصناعية لدعم التبادل العلمي، المنح المشتركة، وبرامج الذكاء الاصطناعي المتقدمة.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "overview",
                        TitleEn = "Government Relations",
                        TitleAr = "العلاقات الحكومية",
                        DescriptionEn = "Managing tenders, operational frameworks, maintenance agreements, and collaborative research and development programs with government entities and public institutions.",
                        DescriptionAr = "إدارة المناقصات، أطر الصيانة، وبرامج البحث والتطوير التعاوني بالشراكة مع الجهات الحكومية والمؤسسات العامة.",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "overview",
                        TitleEn = "International Strategic Partnerships",
                        TitleAr = "الشراكات التجارية الدولية",
                        DescriptionEn = "Supporting international trade projects, industrial tenders, and large-scale strategic collaboration opportunities.",
                        DescriptionAr = "دعم الصفقات التجارية الدولية، المناقصات الصناعية، وفرص التعاون الاستراتيجي واسعة النطاق.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "cards",
                        TitleEn = "Collaboration with Experts",
                        TitleAr = "تعاون مع الخبراء",
                        DescriptionEn = "Connect with professionals, researchers, and specialists within a collaborative environment that encourages knowledge sharing and practical experience exchange.",
                        DescriptionAr = "تواصل مع المهنيين والباحثين والمتخصصين ضمن بيئة تعاونية تدعم تبادل المعرفة والخبرات العملية",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "cards",
                        TitleEn = "Specialized Communities",
                        TitleAr = "مجتمعات متخصصة",
                        DescriptionEn = "Professional communities focused on programming, business analysis, and UI/UX design, created to support continuous learning and professional development.",
                        DescriptionAr = "مجتمعات مهنية متخصصة في البرمجة، تحليل الأعمال، وتصميم وتجربة المستخدم، تهدف إلى دعم التعلم والتطور المهني المستمر.",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "plus",
                        BandKey = "cards",
                        TitleEn = "Flexible Digital Work Opportunities",
                        TitleAr = "فرص عمل رقمية مرنة",
                        DescriptionEn = "Benefit from a modern digital work environment built around task-based collaboration and flexible opportunities within the global digital economy.",
                        DescriptionAr = "استفد من بيئة عمل رقمية متطورة تعتمد على المهام والتعاون لتوفير فرص عملية ضمن الاقتصاد الرقمي العالمي",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "overview",
                        IconName = "microscope",
                        TitleEn = "For Researchers & Experts",
                        TitleAr = "للباحثين والخبراء",
                        DescriptionEn = "Connect with specialized scientific and technical minds to exchange expertise and lead advanced research and innovation initiatives.",
                        DescriptionAr = "تواصل مع عقول علمية وتقنية متخصصة لتبادل الخبرات وقيادة مبادرات بحثية وابتكارية متقدمة",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "overview",
                        IconName = "graduation-cap",
                        TitleEn = "For Students",
                        TitleAr = "للطلاب",
                        DescriptionEn = "Learn, build practical tech skills, and prepare for future digital economy opportunities in a supportive environment.",
                        DescriptionAr = "تعلّم وابنِ مهارات تقنية عملية واستعد لفرص الاقتصاد الرقمي القادمة في بيئة داعمة ومحفزة",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "overview",
                        IconName = "briefcase",
                        TitleEn = "For Professionals",
                        TitleAr = "للمهنيين",
                        DescriptionEn = "Join a specialized professional network across software engineering, business analysis, and design, and grow through collaboration.",
                        DescriptionAr = "انضم إلى شبكة مهنية متخصصة في هندسة البرمجيات وتحليل الأعمال والتصميم، ووسّع آفاقك عبر التعاون",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "overview",
                        IconName = "laptop",
                        TitleEn = "For Remote Talent",
                        TitleAr = "لمواهب العمل عن بعد",
                        DescriptionEn = "Discover task-based work opportunities that connect you with global markets and technical networks without boundaries.",
                        DescriptionAr = "اكتشف فرص عمل قائمة على المهام تربطك بالأسواق العالمية والشبكات التقنية دون حدود جغرافية",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "cards",
                        IconName = "users",
                        TitleEn = "Specialized Communities",
                        TitleAr = "مجتمعات متخصصة",
                        DescriptionEn = "Dedicated groups in software development, business analysis, and design built for learning and professional growth.",
                        DescriptionAr = "مجموعات مخصصة في تطوير البرمجيات، تحليل الأعمال، والتصميم مهيأة للتعلم والنمو المهني.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "cards",
                        IconName = "network",
                        TitleEn = "Collaboration with Experts",
                        TitleAr = "التعاون مع الخبراء",
                        DescriptionEn = "Direct communication pathways with researchers, technologists, and industry leaders for active knowledge exchange.",
                        DescriptionAr = "قنوات اتصال مباشرة مع الباحثين، التقنيين، وقادة الصناعة لتبادل المعرفة النشط.",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "club",
                        BandKey = "cards",
                        IconName = "workflow",
                        TitleEn = "Flexible Digital Work Opportunities",
                        TitleAr = "فرص عمل رقمية مرنة",
                        DescriptionEn = "Access to task-based remote work models integrated into the global economy.",
                        DescriptionAr = "فرص عمل عن بعد قائمة على المهام تمكنك من المشاركة في الاقتصاد العالمي.",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "ai",
                        BandKey = "hero",
                        TitleEn = "Go to Neurix AI Labs",
                        TitleAr = "اذهب إلى Neurix AI Labs",
                        LinkUrl = "/Home/Labs",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "ai",
                        BandKey = "hero",
                        TitleEn = "Go to Neurix AI Technology",
                        TitleAr = "اذهب إلى Neurix AI Technology",
                        LinkUrl = "/Home/Technology",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "heart",
                        TitleEn = "Human-Centered Technology",
                        TitleAr = "التقنية في خدمة الإنسان",
                        DescriptionEn = "Technology must remain a tool that supports the human spirit and guides ethical progress.",
                        DescriptionAr = "يجب أن تظل التقنية أداة تدعم الروح البشرية وتوجه التقدم الأخلاقي",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "brain",
                        TitleEn = "Purpose-Driven Intelligence",
                        TitleAr = "ذكاء ذو هدف ورسالة",
                        DescriptionEn = "Our research and development are designed to solve real-world problems and build sustainable community value.",
                        DescriptionAr = "أبحاثنا وتطويرنا مصممان لحل مشكلات واقعية وبناء قيمة مجتمعية مستدامة",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "users",
                        TitleEn = "Systems Thinking",
                        TitleAr = "تفكر الأنظمة المنظم",
                        DescriptionEn = "We approach innovation with organized, structured methodologies to ensure stability, safety, and scalable impact.",
                        DescriptionAr = "نتبع منهجيات منظمة ومرتبة في الابتكار لضمان الاستقرار، الأمان، والأثر القابل للتوسع",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "shield-check",
                        TitleEn = "Trust by Design",
                        TitleAr = "الثقة بالأمان والحوكمة",
                        DescriptionEn = "Uncompromising security, data privacy, and ethical standards are embedded into every system we build.",
                        DescriptionAr = "أمان صارم، خصوصية بيانات، ومعايير أخلاقية راسخة مدمجة في كل نظام نبنيه",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "sparkles",
                        TitleEn = "Built for the Future",
                        TitleAr = "مستعدون للمستقبل",
                        DescriptionEn = "Engineering adaptable platforms that anticipate the needs of tomorrow and empower future generations.",
                        DescriptionAr = "هندسة منصات مرنة تتوقع احتياجات الغد وتمكن الأجيال القادمة.",
                        DisplayOrder = 4,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "layers",
                        TitleEn = "Neurix AI HQ",
                        TitleAr = "المقر الرئيسي لنيوركس AI",
                        DescriptionEn = "The foundational management stack powering every division — secure, modular, scalable.",
                        DescriptionAr = "الحزمة الإدارية والتأسيسية التي تشغل كل قسم — آمنة ومعيارية وقابلة للتطوير.",
                        LinkUrl = "/Home/HQ",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "beaker",
                        TitleEn = "Neurix AI Labs",
                        TitleAr = "مختبرات نيوركس AI",
                        DescriptionEn = "Advanced R&D center building analytical tools, scientific foundations, and applied research.",
                        DescriptionAr = "مختبر بحث وتطوير متقدم يحوّل تقنيات الذكاء الاصطناعي والتعلّم الآلي إلى حلول ذكية واقعية.",
                        LinkUrl = "/Home/Labs",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "cpu",
                        TitleEn = "Neurix AI Technology",
                        TitleAr = "تكنولوجيا نيوركس AI",
                        DescriptionEn = "Transforms Labs' research outputs into advanced software platforms and scalable applications.",
                        DescriptionAr = "تقوم بتحويل مخرجات مختبرات نيوركس AI البحثية إلى تطبيقات ومنصات تقنية متقدمة.",
                        LinkUrl = "/Home/Technology",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "zap",
                        TitleEn = "Neurix AI Plus",
                        TitleAr = "نيوركس AI بلس",
                        DescriptionEn = "Leads high-level government relations, academic protocols, and strategic partnerships.",
                        DescriptionAr = "تقود العلاقات الحكومية رفيعة المستوى والبروتوكولات الأكاديمية ومسارات الشراكة الاستراتيجية.",
                        LinkUrl = "/Home/Plus",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "users",
                        TitleEn = "Neurix AI Club",
                        TitleAr = "نادي نيوركس AI",
                        DescriptionEn = "A professional and tech network connecting digital talent and remote working experts.",
                        DescriptionAr = "منظومة مهنية وتقنية تجمع الكفاءات حول العالم ومواهب العمل عن بعد في مجتمعات متخصصة.",
                        LinkUrl = "/Home/Club",
                        DisplayOrder = 4,
                        IsPublished = true
                    }
                );
            }

            // ── Seed Homepage Dynamic Sections for Neurix AI ──
            if (!await _db.HeroSections.AnyAsync(h => h.CompanyProfileId == neurixProfileId))
            {
                _db.HeroSections.Add(new CmsHeroSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "Empowering the Future",
                    BadgeAr = "تمكين المستقبل",
                    TitlePrefixEn = "Building Human-Centered ",
                    TitleHighlightEn = "AI",
                    TitleSuffixEn = " for Tomorrow",
                    TitlePrefixAr = " ",
                    TitleHighlightAr = "ذكاء اصطناعي",
                    TitleSuffixAr = " محوره الإنسان، من أجل الغد",
                    SubtitleEn = "Neurix AI is building smarter digital experiences for tomorrow.",
                    SubtitleAr = "نصنع في نيوركس AI تجارب رقمية أذكى لمستقبلنا.",
                    PrimaryButtonTextEn = "Explore Our Divisions",
                    PrimaryButtonTextAr = "استكشف اقسامنا",
                    PrimaryButtonUrl = "#divisions",
                    SecondaryButtonTextEn = "Get in Touch",
                    SecondaryButtonTextAr = "تواصل معنا",
                    SecondaryButtonUrl = "/Home/Contact",
                    Stat1Value = "6+",
                    Stat1LabelEn = "AI Systems",
                    Stat1LabelAr = "أنظمة الذكاء الاصطناعي",
                    Stat2Value = "2+",
                    Stat2LabelEn = "Enterprise Scale",
                    Stat2LabelAr = "حلول المؤسسات",
                    Stat3Value = "5+",
                    Stat3LabelEn = "Digital Platforms",
                    Stat3LabelAr = "المنصات الرقمية",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (!await _db.HumanVisionSections.AnyAsync(v => v.CompanyProfileId == neurixProfileId))
            {
                _db.HumanVisionSections.Add(new CmsHumanVisionSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "Our Core Philosophy",
                    BadgeAr = "فلسفتنا الأساسية",
                    TitlePrefixEn = "Neurix AI ",
                    TitleHighlightEn = "Human Vision",
                    TitlePrefixAr = "رؤية نيوركس AI ",
                    TitleHighlightAr = "الإنسانية",
                    Paragraph1En = "Technological advancement is a great achievement, but its misuse—such as gaming addiction and harmful online behaviors—has become a silent threat to our youth.",
                    Paragraph1Ar = "يُعد التقدم التكنولوجي إنجازاً عظيماً، لكن سوء استخدامه—كإدمان الألعاب والسلوكيات الضارة—يحوله إلى تهديد صامت يشتت شبابنا.",
                    Paragraph2En = "Protecting their minds and guiding them toward the purposeful use of technology is a shared responsibility. Neurix AI's mission is to empower young people to use technology positively, transforming their energy into a force for innovation and societal growth to build a digital future that uplifts humanity.",
                    Paragraph2Ar = "حماية عقول الشباب وتوجيههم نحو الاستخدام الهادف للتكنولوجيا هي مسؤولية مشتركة. لذلك، تسعى 'نيوركس AI' إلى تمكين الشباب من استثمار التكنولوجيا بصورة إيجابية، وتحويل طاقاتهم إلى قوة للابتكار وتطوير المجتمع، لنبني معاً مستقبلاً رقمياً يرتقي بالإنسان.",
                    ImagePath = "/images/Bringing Clarity to Complexity_1 2.png",
                    ImageAltEn = "Neurix AI Human Vision",
                    ImageAltAr = "رؤية نيوركس AI الإنسانية",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (!await _db.PioneersSections.AnyAsync(p => p.CompanyProfileId == neurixProfileId))
            {
                _db.PioneersSections.Add(new CmsPioneersSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "For the Innovators",
                    BadgeAr = "للمبتكرين",
                    TitlePrefixEn = "A Message to the ",
                    TitleHighlightEn = "Pioneers of Innovation",
                    TitlePrefixAr = "رسالة ",
                    TitleHighlightAr = "لرواد الابتكار والأبداع",
                    Paragraph1En = "To all programmers and visionaries: Your mission extends beyond writing code; you are shaping the future.",
                    Paragraph1Ar = "مهمتكم تتجاوز كتابة الأكواد إلى صناعة المستقبل وتوجيه مساره.",
                    Paragraph2En = "You hold the power to make technology a catalyst for progress rather than a tool for distraction. It is your responsibility to channel your skills to elevate humanity, not just advance systems. Let your creativity build a more conscious and human-centered world.",
                    Paragraph2Ar = "التكنولوجيا بين أيديكم إما قوة للبناء والوعي أو أداة لتشتيت الأجيال. لذا، مسؤوليتكم هي توظيف مهاراتكم لخدمة الإنسان، وليس مجرد تطوير الأنظمة. اجعلوا من إبداعكم خطوة نحو عالم أكثر وعياً وإنسانية.",
                    ImagePath = "/images/Infrastructure.png",
                    ImageAltEn = "Pioneers of Innovation",
                    ImageAltAr = "رواد الابتكار",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (!await _db.PillarsSections.AnyAsync(p => p.CompanyProfileId == neurixProfileId))
            {
                _db.PillarsSections.Add(new CmsPillarsSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    TitleEn = "Core Capabilities",
                    TitleAr = "الركائز الاساسية",
                    SubtitleEn = "The foundational pillars driving our intelligent digital ecosystems and enterprise solutions.",
                    SubtitleAr = "الركائز الأساسية التي تقود منظوماتنا الرقمية الذكيّة وحلول المؤسسات.",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (!await _db.PillarItems.AnyAsync(p => p.CompanyProfileId == neurixProfileId))
            {
                _db.PillarItems.AddRange(
                    new CmsPillarItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "brain",
                        TitleEn = "Advanced AI",
                        TitleAr = "الذكاء الاصطناعي المتقدم",
                        DescriptionEn = "Neural architectures and intelligent systems designed to expand the boundaries of machine intelligence and real-world applications.",
                        DescriptionAr = "أنظمة عصبية وتقنيات ذكية تهدف إلى توسيع حدود الذكاء الاصطناعي وتطبيقاته في الواقع",
                        DisplayOrder = 1,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsPillarItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "layers",
                        TitleEn = "Digital Platforms",
                        TitleAr = "المنصات الرقمية",
                        DescriptionEn = "Scalable, secure, and high-performance infrastructure built to support modern digital ecosystems and enterprise solutions.",
                        DescriptionAr = "بنية تحتية قابلة للتوسع، عالية الأداء، وآمنة لدعم الأنظمة الرقمية الحديثة وحلول المؤسسات.",
                        DisplayOrder = 2,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsPillarItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "briefcase",
                        TitleEn = "Operational Excellence",
                        TitleAr = "التميز التشغيلي",
                        DescriptionEn = "Precision-driven execution and continuous improvement systems that ensure reliability, efficiency, and measurable impact.",
                        DescriptionAr = "منهجية تنفيذ دقيقة تعتمد على التحسين المستمر لضمان الكفاءة والموثوقية وتحقيق نتائج قابلة للقياس",
                        DisplayOrder = 3,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsPillarItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "shield-check",
                        TitleEn = "Absolute Trust",
                        TitleAr = "الثقة المطلقة",
                        DescriptionEn = "A foundation of uncompromising security, data protection, and ethical governance embedded in every system we build.",
                        DescriptionAr = "أساس قائم على أعلى معايير الأمان وحماية البيانات والحوكمة الأخلاقية في جميع أنظمتنا",
                        DisplayOrder = 4,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );
            }

            if (!await _db.DivisionsSections.AnyAsync(d => d.CompanyProfileId == neurixProfileId))
            {
                _db.DivisionsSections.Add(new CmsDivisionsSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "Our Ecosystem",
                    BadgeAr = "منظومتنا",
                    TitlePrefixEn = "Five subsidiaries. ",
                    TitleHighlightEn = "One ecosystem.",
                    TitlePrefixAr = "خمسة فروع. ",
                    TitleHighlightAr = "منظومة واحدة.",
                    DescriptionEn = "Each division is purpose-built to drive a specific domain of Neurix AI's broader mission.",
                    DescriptionAr = "كل فرع مصمم لقيادة مجال محدد ضمن رسالة نيوركس AI الأشمل.",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (!await _db.DivisionItems.AnyAsync(d => d.CompanyProfileId == neurixProfileId))
            {
                _db.DivisionItems.AddRange(
                    new CmsDivisionItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "beaker",
                        TitleEn = "Neurix AI Labs",
                        TitleAr = "مختبرات نيوركس AI",
                        DescriptionEn = "Primary R&D center building analytical tools and scientific foundations.",
                        DescriptionAr = "المركز الرئيسي للبحث والتطوير وبناء الأدوات التحليلية والأسس العلمية.",
                        LinkUrl = "/Home/Labs",
                        DisplayOrder = 1,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsDivisionItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "cpu",
                        TitleEn = "Neurix AI Technology",
                        TitleAr = "تكنولوجيا نيوركس AI",
                        DescriptionEn = "Transforms Lab research into integrated applications and market-ready projects.",
                        DescriptionAr = "تحول أبحاث المختبر إلى تطبيقات متكاملة ومشروعات جاهزة للسوق.",
                        LinkUrl = "/Home/Technology",
                        DisplayOrder = 2,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsDivisionItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "users",
                        TitleEn = "Neurix AI Club",
                        TitleAr = "نادي نيوركس AI",
                        DescriptionEn = "Professional networking and remote work infrastructure for global digital talent.",
                        DescriptionAr = "بنية مهنية للتواصل والعمل عن بُعد للمواهب التقنية حول العالم.",
                        LinkUrl = "/Home/Club",
                        DisplayOrder = 3,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsDivisionItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "zap",
                        TitleEn = "Neurix AI Plus",
                        TitleAr = "نيوركس بلس AI",
                        DescriptionEn = "Strategic and sovereign engagements with governments, academia, and industry.",
                        DescriptionAr = "شراكات استراتيجية وسيادية مع الجهات الحكومية والأكاديمية والصناعية.",
                        LinkUrl = "/Home/Plus",
                        DisplayOrder = 4,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsDivisionItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "layers",
                        TitleEn = "Neurix AI HQ",
                        TitleAr = "المقر الرئيسي AI",
                        DescriptionEn = "The foundational stack powering every division — secure, modular, scalable.",
                        DescriptionAr = "الحزمة الأساسية التي تشغل كل قسم — آمنة ومعيارية وقابلة للتطوير.",
                        LinkUrl = "/Home/HQ",
                        DisplayOrder = 5,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );
            }

            if (!await _db.AiEngineeringSections.AnyAsync(a => a.CompanyProfileId == neurixProfileId))
            {
                _db.AiEngineeringSections.Add(new CmsAiEngineeringSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "Core Services",
                    BadgeAr = "خدماتنا الأساسية",
                    TitleEn = "AI & Engineering",
                    TitleAr = "الذكاء الاصطناعي والهندسة",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (!await _db.AiEngineeringItems.AnyAsync(a => a.CompanyProfileId == neurixProfileId))
            {
                _db.AiEngineeringItems.AddRange(
                    new CmsAiEngineeringItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "code",
                        TitleEn = "Software Development",
                        TitleAr = "تطوير البرمجيات",
                        DescriptionEn = "Intelligent Systems & Applied AI Product Development",
                        DescriptionAr = "الأنظمة الذكية وتطوير منتجات الذكاء الاصطناعي التطبيقية",
                        DisplayOrder = 1,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsAiEngineeringItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "database",
                        TitleEn = "Data Science Solutions",
                        TitleAr = "حلول علم البيانات",
                        DescriptionEn = "Machine Learning & Data Science Solutions",
                        DescriptionAr = "حلول التعلم الآلي وعلم البيانات",
                        DisplayOrder = 2,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsAiEngineeringItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "sparkles",
                        TitleEn = "AI Software Design",
                        TitleAr = "تصميم وتطوير برمجيات الذكاء الاصطناعي",
                        DescriptionEn = "Artificial Intelligence (AI) Software Design & Development",
                        DescriptionAr = "تصميم وتطوير برمجيات الذكاء الاصطناعي (AI)",
                        DisplayOrder = 3,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsAiEngineeringItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        IconName = "terminal",
                        TitleEn = "Custom Engineering",
                        TitleAr = "تطوير البرمجيات المخصصة",
                        DescriptionEn = "Software Engineering & Scale Architectures",
                        DescriptionAr = "تطوير البرمجيات وهندسة الأنظمة القابلة للتوسع",
                        DisplayOrder = 4,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );
            }

            if (!await _db.ListSectionHeaders.AnyAsync(h => h.CompanyProfileId == neurixProfileId))
            {
                _db.ListSectionHeaders.AddRange(
                    new CmsListSectionHeader
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        SectionKey = CmsListSectionKeys.Portfolio,
                        BadgeEn = "Engineering Portfolio",
                        BadgeAr = "معرض الأعمال",
                        TitlePrefixEn = "Featured ",
                        TitleHighlightEn = "Deployments",
                        TitlePrefixAr = "مشاريع ",
                        TitleHighlightAr = "مميزة",
                        ItemLinkTextEn = "View Case Study",
                        ItemLinkTextAr = "عرض دراسة الحالة",
                        DefaultCategoryLabelEn = "AI System",
                        DefaultCategoryLabelAr = "نظام ذكاء اصطناعي",
                        ButtonTextEn = "View All Projects",
                        ButtonTextAr = "عرض جميع المشاريع",
                        ButtonUrl = "/Home/Portfolio",
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsListSectionHeader
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        SectionKey = CmsListSectionKeys.Insights,
                        BadgeEn = "Research & Thought Leadership",
                        BadgeAr = "الأبحاث والرؤى",
                        TitlePrefixEn = "Latest ",
                        TitleHighlightEn = "Insights",
                        TitlePrefixAr = "أحدث ",
                        TitleHighlightAr = "المقالات والأبحاث",
                        ItemLinkTextEn = "Read Article",
                        ItemLinkTextAr = "اقرأ المقال",
                        DefaultCategoryLabelEn = "Research",
                        DefaultCategoryLabelAr = "أبحاث",
                        ReadTimeSuffixEn = "min read",
                        ReadTimeSuffixAr = "دقيقة قراءة",
                        UndatedLabelEn = "Recent",
                        UndatedLabelAr = "حديثاً",
                        ButtonTextEn = "View All Publications",
                        ButtonTextAr = "عرض جميع المنشورات",
                        ButtonUrl = "/Home/Insights",
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsListSectionHeader
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        SectionKey = CmsListSectionKeys.Testimonials,
                        BadgeEn = "Trusted by Leaders",
                        BadgeAr = "موثوق من قادة الصناعة",
                        TitlePrefixEn = "What Partners Say",
                        TitleHighlightEn = "",
                        TitlePrefixAr = "ماذا يقول شركاؤنا",
                        TitleHighlightAr = "",
                        SubtitleEn = "Endorsements from universities, institutional partners, and technical leadership.",
                        SubtitleAr = "شهادات وتوصيات من الجامعات والشركاء المؤسسيين والقيادات التقنية.",
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );
            }

            if (!await _db.EthicsSections.AnyAsync(e => e.CompanyProfileId == neurixProfileId))
            {
                _db.EthicsSections.Add(new CmsEthicsSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "Operational Strategy",
                    BadgeAr = "الاستراتيجية التشغيلية",
                    TitlePrefixEn = "Our Vision & ",
                    TitleHighlightEn = "Implementation Framework",
                    TitlePrefixAr = "رؤيتنا و ",
                    TitleHighlightAr = "إطار عمل التنفيذ",
                    DescriptionEn = "Neurix AI serves as the technological engine of the ecosystem, transforming scientific research, AI-driven tools, and technical infrastructure into practical solutions for education, healthcare, government, academia, and professional communities.",
                    DescriptionAr = "تعمل نيوركس كمحرك تقني للمنظومة، حيث تقوم بتحويل البحث العلمي والذكاء الاصطناعي والبنية التحتية إلى حلول عملية في مجالات التعليم، الرعاية الصحية، الحكومات، الأكاديميات، والمجتمعات المهنية.",
                    TopImagePath = "/images/Software.png",
                    TopImageAltEn = "Software Office Desk Setup",
                    TopImageAltAr = "بيئة تطوير البرمجيات",
                    BottomImagePath = "/images/Research.png",
                    BottomImageAltEn = "Pristine Laboratory Setup",
                    BottomImageAltAr = "مختبر الأبحاث العلمية",
                    BackgroundImagePath = "/images/Infrastructure.png",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            var existingCta = await _db.CtaSections.FirstOrDefaultAsync(c => c.CompanyProfileId == neurixProfileId);
            if (existingCta == null)
            {
                _db.CtaSections.Add(new CmsCtaSection
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = neurixProfileId,
                    BadgeEn = "Let's Build Together",
                    BadgeAr = "لنبني معاً",
                    TitlePrefixEn = "Ready to engineer",
                    TitleHighlightEn = " your future?",
                    TitlePrefixAr = "مستعد لهندسة",
                    TitleHighlightAr = " مستقبلك؟",
                    ButtonTextEn = "Contact Us",
                    ButtonTextAr = "تواصل معنا",
                    ButtonUrl = "/Home/Contact",
                    ContactEmail = "contact@neurix.ai",
                    BackgroundImagePath = "/images/Contact Us (Home) 2.png",
                    IsPublished = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else if (existingCta.ContactEmail == "neurix@aidaleel.com")
            {
                existingCta.ContactEmail = "contact@neurix.ai";
            }

            if (!await _db.MenuItems.AnyAsync(m => m.CompanyProfileId == neurixProfileId))
            {
                _db.MenuItems.AddRange(
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Home",
                        LabelAr = "الرئيسية",
                        Url = "/",
                        Placement = CmsMenuItemPlacement.Header,
                        DisplayOrder = 1,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Labs",
                        LabelAr = "المختبرات",
                        Url = "/Home/Labs",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 2,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Technology",
                        LabelAr = "التقنية",
                        Url = "/Home/Technology",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 3,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Club",
                        LabelAr = "النادي",
                        Url = "/Home/Club",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 4,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Plus",
                        LabelAr = "بلس",
                        Url = "/Home/Plus",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 5,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "HQ",
                        LabelAr = "المقر الرئيسي",
                        Url = "/Home/HQ",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 6,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Portfolio",
                        LabelAr = "المشاريع",
                        Url = "/Home/Portfolio",
                        Placement = CmsMenuItemPlacement.Header,
                        DisplayOrder = 7,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Insights",
                        LabelAr = "المقالات",
                        Url = "/Home/Insights",
                        Placement = CmsMenuItemPlacement.Header,
                        DisplayOrder = 8,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "About",
                        LabelAr = "من نحن",
                        Url = "/Home/About",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 9,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Careers",
                        LabelAr = "الوظائف",
                        Url = "/Home/ComingSoon",
                        Placement = CmsMenuItemPlacement.Footer,
                        DisplayOrder = 10,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "News",
                        LabelAr = "الأخبار",
                        Url = "/Home/ComingSoon",
                        Placement = CmsMenuItemPlacement.Footer,
                        DisplayOrder = 11,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Contact",
                        LabelAr = "تواصل معنا",
                        Url = "/Home/Contact",
                        Placement = CmsMenuItemPlacement.Both,
                        DisplayOrder = 12,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Privacy",
                        LabelAr = "الخصوصية",
                        Url = "/Home/Privacy",
                        Placement = CmsMenuItemPlacement.FooterBottom,
                        DisplayOrder = 13,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Terms",
                        LabelAr = "الشروط",
                        Url = "/Home/ComingSoon",
                        Placement = CmsMenuItemPlacement.FooterBottom,
                        DisplayOrder = 14,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsMenuItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        LabelEn = "Security",
                        LabelAr = "الأمان",
                        Url = "/Home/ComingSoon",
                        Placement = CmsMenuItemPlacement.FooterBottom,
                        DisplayOrder = 15,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );
            }

            // ── Standalone content pages (About / Privacy / Contact) ──
            // Seeded with the exact copy that used to be hardcoded in the Razor views, so
            // enabling CMS control changes nothing visible until an editor edits something.
            if (!await _db.ContentPages.AnyAsync(p => p.CompanyProfileId == neurixProfileId))
            {
                _db.ContentPages.AddRange(
                    new CmsContentPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "about",
                        TitleEn = "About Neurix AI",
                        TitleAr = "عن نيوركس AI",
                        HeroBadgeEn = "About Neurix AI",
                        HeroBadgeAr = "عن نيوركس AI",
                        HeroTitlePrefixEn = "Engineering at the ",
                        HeroTitlePrefixAr = "الهندسة عند ",
                        HeroTitleHighlightEn = "Point of Intersection",
                        HeroTitleHighlightAr = "نقطة التقاطع",
                        HeroSubtitleEn = "Neurix AI is the technological engine and holding company of an integrated ecosystem. It provides scientific research, technical infrastructure, and implementation protocols, while active in managing government projects, university protocols, and international technology partnerships.",
                        HeroSubtitleAr = "نيوركس AI هي المحرك التقني والشركة القابضة لمنظومة متكاملة، حيث تقدم الأبحاث العلمية والبنية التحتية التقنية وبروتوكولات التنفيذ، كما تنشط في إدارة المشاريع الحكومية والبروتوكولات الجامعية والشراكات التقنية الدولية.",
                        // Title strings keep the two-tone split of the original markup:
                        // everything before the final space renders plain, the last word cyan.
                        MissionTitleEn = "Our Mission",
                        MissionTitleAr = "رسالتنا الرسالة",
                        MissionTextEn = "Our mission is to build a technology ecosystem focused on human development. We translate advanced scientific research and artificial intelligence into practical, ethical systems that serve institutions, communities, and future generations.",
                        MissionTextAr = "رسالتنا هي بناء منظومة تقنية متكاملة تتمحور حول الإنسان، وتحول البحث العلمي والذكاء الاصطناعي إلى حلول عملية وأخلاقية تخدم المؤسسات والمجتمعات والأجيال القادمة.",
                        VisionTitleEn = "Our Vision",
                        VisionTitleAr = "رؤيتنا الرؤية",
                        VisionTextEn = "To establish a conscious technological civilization that serves humanity, maintains organized progress, and preserves human nature in alignment with the divine purpose of existence.",
                        VisionTextAr = "رؤيتنا هي تشكيل حضارة تقنية واعية، تضع الابتكار في خدمة الإنسان، وتدعم التقدم الحضاري المنظم، مع صون الفطرة والطبيعة البشرية بما يتوافق مع غايات الوجود.",
                        CtaBadgeEn = "Let's Build Together",
                        CtaBadgeAr = "لنبني معاً",
                        CtaTitleEn = "Bring your challenge.",
                        CtaTitleAr = "أحضر تحديك.",
                        CtaSubtitleEn = "We validate it, build it, and deliver it — on time, on spec, and production-ready.",
                        CtaSubtitleAr = "نتحقق منه، نبنيه، ونسلمه — في الوقت المحدد، وفق المواصفات، وجاهز للإنتاج.",
                        CtaButtonTextEn = "Start a Conversation",
                        CtaButtonTextAr = "ابدأ محادثة",
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsContentPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "privacy",
                        TitleEn = "Privacy Policy",
                        TitleAr = "سياسة الخصوصية",
                        HeroBadgeEn = "Legal Information",
                        HeroBadgeAr = "المعلومات القانونية",
                        // Body intentionally left empty: a privacy policy is a legal document
                        // and must be written by the business, not invented here. Until it is
                        // filled in, the page keeps showing its "Coming Soon" state.
                        BodyEn = null,
                        BodyAr = null,
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new CmsContentPage
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        Slug = "contact",
                        TitleEn = "Contact Us",
                        TitleAr = "اتصل بنا",
                        HeroTitlePrefixEn = "Let's build the ",
                        HeroTitlePrefixAr = "لنبني ",
                        HeroTitleHighlightEn = "future together.",
                        HeroTitleHighlightAr = "المستقبل معاً.",
                        HeroSubtitleEn = "Get in touch with our team of engineers and researchers.",
                        HeroSubtitleAr = "تواصل مع فريقنا من المهندسين والباحثين.",
                        IsPublished = true,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );
            }

            // ── Ensure About page bands and items exist (Idempotency safeguard) ──
            if (!_db.PageBands.Local.Any(b => b.CompanyProfileId == neurixProfileId && b.PageKey == "about") &&
                !await _db.PageBands.AnyAsync(b => b.CompanyProfileId == neurixProfileId && b.PageKey == "about"))
            {
                _db.PageBands.AddRange(
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        TitlePrefixEn = "What We Stand For",
                        TitlePrefixAr = "ما الذي نؤمن به",
                        BodyEn = "Five foundational principles that govern every line of code, every architectural decision, and guide our entire workflow.",
                        BodyAr = "خمسة مبادئ أساسية تحكم كل سطر كود، وكل قرار معماري، وتوجه مسار عملنا بالكامل.",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        BadgeEn = "Organizational Structure",
                        BadgeAr = "الهيكل التنظيمي",
                        TitlePrefixEn = "Five pillars.",
                        TitlePrefixAr = "خمس ركائز.",
                        TitleHighlightEn = "One vision.",
                        TitleHighlightAr = "رؤية واحدة.",
                        BodyEn = "Each division operates with full autonomy under unified strategic direction from the CEO.",
                        BodyAr = "يعمل كل قسم باستقلالية كاملة تحت التوجيه الاستراتيجي الموحد من الرئيس التنفيذي.",
                        ItemLinkTextEn = "Explore",
                        ItemLinkTextAr = "المزيد",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBand
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "team",
                        BadgeEn = "Leadership & Researchers",
                        BadgeAr = "القيادة والباحثون",
                        TitlePrefixEn = "The Minds Behind Neurix",
                        TitlePrefixAr = "العقول وراء نيوركس",
                        BodyEn = "A multidisciplinary collective of AI scientists, systems architects, and sovereign technology strategists.",
                        BodyAr = "فريق متعدد التخصصات من علماء الذكاء الاصطناعي ومهندسي الأنظمة وخبراء التقنية السيادية.",
                        DisplayOrder = 2,
                        IsPublished = true
                    }
                );
            }

            if (!_db.PageBandItems.Local.Any(i => i.CompanyProfileId == neurixProfileId && i.PageKey == "about") &&
                !await _db.PageBandItems.AnyAsync(i => i.CompanyProfileId == neurixProfileId && i.PageKey == "about"))
            {
                _db.PageBandItems.AddRange(
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "heart",
                        TitleEn = "Human-Centered Technology",
                        TitleAr = "التقنية في خدمة الإنسان",
                        DescriptionEn = "Technology must remain a tool that supports the human spirit and guides ethical progress.",
                        DescriptionAr = "يجب أن تظل التقنية أداة تدعم الروح البشرية وتوجه التقدم الأخلاقي",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "brain",
                        TitleEn = "Purpose-Driven Intelligence",
                        TitleAr = "ذكاء ذو هدف ورسالة",
                        DescriptionEn = "Our research and development are designed to solve real-world problems and build sustainable community value.",
                        DescriptionAr = "أبحاثنا وتطويرنا مصممان لحل مشكلات واقعية وبناء قيمة مجتمعية مستدامة",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "users",
                        TitleEn = "Systems Thinking",
                        TitleAr = "تفكر الأنظمة المنظم",
                        DescriptionEn = "We approach innovation with organized, structured methodologies to ensure stability, safety, and scalable impact.",
                        DescriptionAr = "نتبع منهجيات منظمة ومرتبة في الابتكار لضمان الاستقرار، الأمان، والأثر القابل للتوسع",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "shield-check",
                        TitleEn = "Trust by Design",
                        TitleAr = "الثقة بالأمان والحوكمة",
                        DescriptionEn = "Uncompromising security, data privacy, and ethical standards are embedded into every system we build.",
                        DescriptionAr = "أمان صارم، خصوصية بيانات، ومعايير أخلاقية راسخة مدمجة في كل نظام نبنيه",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "principles",
                        IconName = "sparkles",
                        TitleEn = "Built for the Future",
                        TitleAr = "مستعدون للمستقبل",
                        DescriptionEn = "Engineering adaptable platforms that anticipate the needs of tomorrow and empower future generations.",
                        DescriptionAr = "هندسة منصات مرنة تتوقع احتياجات الغد وتمكن الأجيال القادمة.",
                        DisplayOrder = 4,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "layers",
                        TitleEn = "Neurix AI HQ",
                        TitleAr = "المقر الرئيسي لنيوركس AI",
                        DescriptionEn = "The foundational management stack powering every division — secure, modular, scalable.",
                        DescriptionAr = "الحزمة الإدارية والتأسيسية التي تشغل كل قسم — آمنة ومعيارية وقابلة للتطوير.",
                        LinkUrl = "/Home/HQ",
                        DisplayOrder = 0,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "beaker",
                        TitleEn = "Neurix AI Labs",
                        TitleAr = "مختبرات نيوركس AI",
                        DescriptionEn = "Advanced R&D center building analytical tools, scientific foundations, and applied research.",
                        DescriptionAr = "مختبر بحث وتطوير متقدم يحوّل تقنيات الذكاء الاصطناعي والتعلّم الآلي إلى حلول ذكية واقعية.",
                        LinkUrl = "/Home/Labs",
                        DisplayOrder = 1,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "cpu",
                        TitleEn = "Neurix AI Technology",
                        TitleAr = "تكنولوجيا نيوركس AI",
                        DescriptionEn = "Transforms Labs' research outputs into advanced software platforms and scalable applications.",
                        DescriptionAr = "تقوم بتحويل مخرجات مختبرات نيوركس AI البحثية إلى تطبيقات ومنصات تقنية متقدمة.",
                        LinkUrl = "/Home/Technology",
                        DisplayOrder = 2,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "zap",
                        TitleEn = "Neurix AI Plus",
                        TitleAr = "نيوركس AI بلس",
                        DescriptionEn = "Leads high-level government relations, academic protocols, and strategic partnerships.",
                        DescriptionAr = "تقود العلاقات الحكومية رفيعة المستوى والبروتوكولات الأكاديمية ومسارات الشراكة الاستراتيجية.",
                        LinkUrl = "/Home/Plus",
                        DisplayOrder = 3,
                        IsPublished = true
                    },
                    new CmsPageBandItem
                    {
                        Id = Guid.NewGuid(),
                        CompanyProfileId = neurixProfileId,
                        PageKey = "about",
                        BandKey = "structure",
                        IconName = "users",
                        TitleEn = "Neurix AI Club",
                        TitleAr = "نادي نيوركس AI",
                        DescriptionEn = "A professional and tech network connecting digital talent and remote working experts.",
                        DescriptionAr = "منظومة مهنية وتقنية تجمع الكفاءات حول العالم ومواهب العمل عن بعد في مجتمعات متخصصة.",
                        LinkUrl = "/Home/Club",
                        DisplayOrder = 4,
                        IsPublished = true
                    }
                );
            }

            await _db.SaveChangesAsync();
        }
    }
}
