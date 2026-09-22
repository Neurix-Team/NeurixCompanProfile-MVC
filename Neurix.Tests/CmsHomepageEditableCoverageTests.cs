using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    /// <summary>
    /// Covers the homepage blocks that used to be hardcoded in Views/Home/Index.cshtml and are
    /// now editable from /cms/home-sections: the "Our Ecosystem" band (#divisions), the
    /// "AI &amp; Engineering" band (#services), and the header copy for the three bands whose
    /// cards come from elsewhere (#portfolio, #insights, #testimonials).
    /// </summary>
    public class CmsHomepageEditableCoverageTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsHomeSectionService _service;
        private readonly Guid _profileId;

        public CmsHomepageEditableCoverageTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsHomeSectionService(_db, NullLogger<CmsHomeSectionService>.Instance);

            _profileId = Guid.NewGuid();
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

        // ── #divisions ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertDivisionsSection_CreatesThenUpdatesInPlace()
        {
            var insert = await _service.UpsertDivisionsSectionAsync(new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Our Ecosystem",
                BadgeAr = "منظومتنا",
                TitlePrefixEn = "Five subsidiaries. ",
                TitleHighlightEn = "One ecosystem.",
                TitlePrefixAr = "خمسة فروع. ",
                TitleHighlightAr = "منظومة واحدة.",
                DescriptionEn = "Each division drives a specific domain.",
                DescriptionAr = "كل فرع يقود مجالاً محدداً.",
                IsPublished = true
            });

            Assert.True(insert.Success);

            var created = await _service.GetDivisionsSectionByCompanyIdAsync(_profileId);
            Assert.NotNull(created);
            Assert.Equal("Our Ecosystem", created!.BadgeEn);
            Assert.Equal("منظومتنا", created.BadgeAr);
            Assert.Equal("One ecosystem.", created.TitleHighlightEn);
            Assert.Null(created.UpdatedAtUtc);

            var update = await _service.UpsertDivisionsSectionAsync(new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "The Group",
                BadgeAr = "المجموعة",
                TitlePrefixEn = "Six subsidiaries. ",
                TitleHighlightEn = "One mission.",
                TitlePrefixAr = "ستة فروع. ",
                TitleHighlightAr = "مهمة واحدة.",
                DescriptionEn = "Updated copy.",
                DescriptionAr = "نص محدث.",
                IsPublished = false
            });

            Assert.True(update.Success);

            // One row per company profile: the upsert must not insert a second.
            Assert.Equal(1, await _db.DivisionsSections.CountAsync());

            var updated = await _service.GetDivisionsSectionByCompanyIdAsync(_profileId);
            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated!.Id);
            Assert.Equal("The Group", updated.BadgeEn);
            Assert.False(updated.IsPublished);
            Assert.NotNull(updated.UpdatedAtUtc);
        }

        [Fact]
        public async Task GetDivisionsSectionByCompanySlug_HidesUnpublishedFromThePublicSite()
        {
            await _service.UpsertDivisionsSectionAsync(new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Our Ecosystem",
                IsPublished = false
            });

            Assert.Null(await _service.GetDivisionsSectionByCompanySlugAsync("neurix"));
            Assert.NotNull(await _service.GetDivisionsSectionByCompanySlugAsync("neurix", includeUnpublished: true));
        }

        [Fact]
        public async Task DivisionItems_FullCrudOrderingAndPublishFiltering()
        {
            var second = await _service.CreateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "cpu",
                TitleEn = "Neurix AI Technology",
                TitleAr = "تكنولوجيا نيوركس AI",
                DescriptionEn = "Turns research into products.",
                DescriptionAr = "تحول الأبحاث إلى منتجات.",
                LinkUrl = "/Home/Technology",
                DisplayOrder = 2,
                IsPublished = true
            });

            var first = await _service.CreateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "beaker",
                TitleEn = "Neurix AI Labs",
                TitleAr = "مختبرات نيوركس AI",
                DescriptionEn = "Primary R&D center.",
                DescriptionAr = "مركز البحث والتطوير.",
                LinkUrl = "/Home/Labs",
                DisplayOrder = 1,
                IsPublished = true
            });

            var draft = await _service.CreateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "zap",
                TitleEn = "Neurix AI Plus",
                TitleAr = "نيوركس بلس AI",
                LinkUrl = "/Home/Plus",
                DisplayOrder = 3,
                IsPublished = false
            });

            Assert.True(second.Success);
            Assert.True(first.Success);
            Assert.True(draft.Success);

            // Dashboard sees drafts, ordered by DisplayOrder.
            var dashboard = await _service.GetDivisionItemsByCompanyIdAsync(_profileId, includeUnpublished: true);
            Assert.Equal(3, dashboard.Count);
            Assert.Equal(new[] { "Neurix AI Labs", "Neurix AI Technology", "Neurix AI Plus" },
                dashboard.Select(d => d.TitleEn).ToArray());

            // The public site does not.
            var live = await _service.GetDivisionItemsByCompanySlugAsync("neurix");
            Assert.Equal(2, live.Count);
            Assert.DoesNotContain("Neurix AI Plus", live.Select(d => d.TitleEn));

            // Blank link text falls back rather than rendering an empty anchor.
            Assert.All(live, item => Assert.Equal("Explore", item.LinkTextEn));
            Assert.All(live, item => Assert.Equal("المزيد", item.LinkTextAr));

            // Update
            var updateResult = await _service.UpdateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                Id = first.Value,
                CompanyProfileId = _profileId,
                IconName = "microscope",
                TitleEn = "Neurix AI Research",
                TitleAr = "أبحاث نيوركس AI",
                DescriptionEn = "Renamed.",
                DescriptionAr = "تم التغيير.",
                LinkUrl = "/Home/Labs",
                LinkTextEn = "Discover",
                LinkTextAr = "اكتشف",
                DisplayOrder = 1,
                IsPublished = true
            });

            Assert.True(updateResult.Success);

            var reloaded = await _service.GetDivisionItemByIdAsync(first.Value);
            Assert.NotNull(reloaded);
            Assert.Equal("Neurix AI Research", reloaded!.TitleEn);
            Assert.Equal("microscope", reloaded.IconName);
            Assert.Equal("Discover", reloaded.LinkTextEn);
            Assert.NotNull(reloaded.UpdatedAtUtc);

            // Soft delete: gone from every read path, still a row underneath.
            Assert.True((await _service.DeleteDivisionItemAsync(second.Value)).Success);
            Assert.Equal(2, (await _service.GetDivisionItemsByCompanyIdAsync(_profileId, includeUnpublished: true)).Count);
            Assert.Equal(3, await _db.DivisionItems.IgnoreQueryFilters().CountAsync());
        }

        [Fact]
        public async Task UpdateAndDeleteDivisionItem_FailCleanlyForUnknownIds()
        {
            var missingId = await _service.UpdateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                CompanyProfileId = _profileId,
                TitleEn = "No id"
            });

            Assert.False(missingId.Success);

            var unknown = await _service.UpdateDivisionItemAsync(new CmsDivisionItemUpsertDto
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = _profileId,
                TitleEn = "Ghost"
            });

            Assert.False(unknown.Success);
            Assert.False((await _service.DeleteDivisionItemAsync(Guid.NewGuid())).Success);
        }

        // ── #services (AI & Engineering) ──────────────────────────────────────────

        [Fact]
        public async Task UpsertAiEngineeringSection_CreatesThenUpdatesInPlace()
        {
            Assert.True((await _service.UpsertAiEngineeringSectionAsync(new CmsAiEngineeringSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Core Services",
                BadgeAr = "خدماتنا الأساسية",
                TitleEn = "AI & Engineering",
                TitleAr = "الذكاء الاصطناعي والهندسة",
                IsPublished = true
            })).Success);

            var created = await _service.GetAiEngineeringSectionByCompanyIdAsync(_profileId);
            Assert.NotNull(created);
            Assert.Equal("AI & Engineering", created!.TitleEn);
            Assert.Equal("الذكاء الاصطناعي والهندسة", created.TitleAr);

            Assert.True((await _service.UpsertAiEngineeringSectionAsync(new CmsAiEngineeringSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "What We Do",
                BadgeAr = "ما نقوم به",
                TitleEn = "Applied Engineering",
                TitleAr = "الهندسة التطبيقية",
                IsPublished = true
            })).Success);

            Assert.Equal(1, await _db.AiEngineeringSections.CountAsync());

            var updated = await _service.GetAiEngineeringSectionByCompanyIdAsync(_profileId);
            Assert.Equal(created.Id, updated!.Id);
            Assert.Equal("Applied Engineering", updated.TitleEn);
        }

        [Fact]
        public async Task AiEngineeringItems_FullCrudOrderingAndPublishFiltering()
        {
            var later = await _service.CreateAiEngineeringItemAsync(new CmsAiEngineeringItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "database",
                TitleEn = "Data Science Solutions",
                TitleAr = "حلول علم البيانات",
                DescriptionEn = "Machine learning.",
                DescriptionAr = "التعلم الآلي.",
                DisplayOrder = 2,
                IsPublished = true
            });

            var earlier = await _service.CreateAiEngineeringItemAsync(new CmsAiEngineeringItemUpsertDto
            {
                CompanyProfileId = _profileId,
                IconName = "code",
                TitleEn = "Software Development",
                TitleAr = "تطوير البرمجيات",
                DescriptionEn = "Intelligent systems.",
                DescriptionAr = "الأنظمة الذكية.",
                DisplayOrder = 1,
                IsPublished = true
            });

            var hidden = await _service.CreateAiEngineeringItemAsync(new CmsAiEngineeringItemUpsertDto
            {
                CompanyProfileId = _profileId,
                TitleEn = "Not Ready",
                TitleAr = "غير جاهز",
                DisplayOrder = 3,
                IsPublished = false
            });

            var live = await _service.GetAiEngineeringItemsByCompanySlugAsync("neurix");
            Assert.Equal(new[] { "Software Development", "Data Science Solutions" }, live.Select(i => i.TitleEn).ToArray());

            // An icon left blank still renders, using the section default.
            var hiddenDto = await _service.GetAiEngineeringItemByIdAsync(hidden.Value);
            Assert.Equal("code", hiddenDto!.IconName);

            Assert.True((await _service.UpdateAiEngineeringItemAsync(new CmsAiEngineeringItemUpsertDto
            {
                Id = later.Value,
                CompanyProfileId = _profileId,
                IconName = "sparkles",
                TitleEn = "Applied Data Science",
                TitleAr = "علم البيانات التطبيقي",
                DisplayOrder = 2,
                IsPublished = true
            })).Success);

            Assert.Equal("Applied Data Science", (await _service.GetAiEngineeringItemByIdAsync(later.Value))!.TitleEn);

            Assert.True((await _service.DeleteAiEngineeringItemAsync(earlier.Value)).Success);
            Assert.Equal(2, (await _service.GetAiEngineeringItemsByCompanyIdAsync(_profileId, includeUnpublished: true)).Count);
            Assert.Equal(3, await _db.AiEngineeringItems.IgnoreQueryFilters().CountAsync());
        }

        // ── #portfolio / #insights / #testimonials headers ────────────────────────

        [Theory]
        [InlineData(CmsListSectionKeys.Portfolio)]
        [InlineData(CmsListSectionKeys.Insights)]
        [InlineData(CmsListSectionKeys.Testimonials)]
        public async Task UpsertListSectionHeader_RoundTripsEveryKnownBand(string sectionKey)
        {
            Assert.True((await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = sectionKey,
                BadgeEn = $"{sectionKey} badge",
                BadgeAr = "شارة",
                TitlePrefixEn = "Featured ",
                TitleHighlightEn = "Work",
                TitlePrefixAr = "أعمال ",
                TitleHighlightAr = "مميزة",
                SubtitleEn = "Intro line.",
                SubtitleAr = "سطر تعريفي.",
                ItemLinkTextEn = "View Case Study",
                ItemLinkTextAr = "عرض دراسة الحالة",
                DefaultCategoryLabelEn = "AI System",
                DefaultCategoryLabelAr = "نظام ذكاء اصطناعي",
                ReadTimeSuffixEn = "min read",
                ReadTimeSuffixAr = "دقيقة قراءة",
                UndatedLabelEn = "Recent",
                UndatedLabelAr = "حديثاً",
                ButtonTextEn = "View All",
                ButtonTextAr = "عرض الكل",
                ButtonUrl = "/Home/Portfolio",
                IsPublished = true
            })).Success);

            var header = await _service.GetListSectionHeaderByCompanySlugAsync("neurix", sectionKey);
            Assert.NotNull(header);
            Assert.Equal(sectionKey, header!.SectionKey);
            Assert.Equal($"{sectionKey} badge", header.BadgeEn);
            Assert.Equal("View Case Study", header.ItemLinkTextEn);
            Assert.Equal("AI System", header.DefaultCategoryLabelEn);
            Assert.Equal("نظام ذكاء اصطناعي", header.DefaultCategoryLabelAr);
            Assert.Equal("min read", header.ReadTimeSuffixEn);
            Assert.Equal("Recent", header.UndatedLabelEn);
            Assert.Equal("View All", header.ButtonTextEn);
            Assert.Equal("/Home/Portfolio", header.ButtonUrl);
        }

        [Fact]
        public async Task ListSectionHeaders_AreKeyedIndependentlyPerBand()
        {
            await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = CmsListSectionKeys.Portfolio,
                BadgeEn = "Engineering Portfolio",
                IsPublished = true
            });

            await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = CmsListSectionKeys.Insights,
                BadgeEn = "Research & Thought Leadership",
                IsPublished = true
            });

            // Editing one band must not disturb the other.
            await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = CmsListSectionKeys.Portfolio,
                BadgeEn = "Selected Work",
                IsPublished = true
            });

            var all = await _service.GetListSectionHeadersByCompanyIdAsync(_profileId);
            Assert.Equal(2, all.Count);
            Assert.Equal("Selected Work", all.Single(h => h.SectionKey == CmsListSectionKeys.Portfolio).BadgeEn);
            Assert.Equal("Research & Thought Leadership", all.Single(h => h.SectionKey == CmsListSectionKeys.Insights).BadgeEn);
        }

        [Fact]
        public async Task UpsertListSectionHeader_NormalisesTheKeyAndRejectsUnknownBands()
        {
            Assert.True((await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = "  Portfolio  ",
                BadgeEn = "Engineering Portfolio",
                IsPublished = true
            })).Success);

            Assert.NotNull(await _service.GetListSectionHeaderByCompanyIdAsync(_profileId, CmsListSectionKeys.Portfolio));

            var rejected = await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = "newsletter",
                BadgeEn = "Nope"
            });

            Assert.False(rejected.Success);
            Assert.Equal(1, await _db.ListSectionHeaders.CountAsync());
        }

        [Fact]
        public async Task GetListSectionHeaderByCompanySlug_HidesUnpublishedSoTheViewFallsBack()
        {
            await _service.UpsertListSectionHeaderAsync(new CmsListSectionHeaderUpsertDto
            {
                CompanyProfileId = _profileId,
                SectionKey = CmsListSectionKeys.Testimonials,
                BadgeEn = "Trusted by Leaders",
                IsPublished = false
            });

            Assert.Null(await _service.GetListSectionHeaderByCompanySlugAsync("neurix", CmsListSectionKeys.Testimonials));
            Assert.NotNull(await _service.GetListSectionHeaderByCompanySlugAsync("neurix", CmsListSectionKeys.Testimonials, includeUnpublished: true));
        }
    }
}
