using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Xunit;

namespace Neurix.Tests
{
    public class CmsSeederTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsSeeder _seeder;

        public CmsSeederTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _seeder = new CmsSeeder(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task SeedAsync_SeedsDefaultCompaniesAndServices()
        {
            await _seeder.SeedAsync();

            var companies = await _db.CompanyProfiles.ToListAsync();
            var services = await _db.Services.ToListAsync();
            var socialLinks = await _db.SocialLinks.ToListAsync();

            Assert.Single(companies);
            var neurix = Assert.Single(companies, c => c.Slug == "neurix");
            Assert.Equal("#00C2D4", neurix.PrimaryColor);
            Assert.Equal("#5B5FEF", neurix.AccentColor);
            Assert.Equal("/images/favicon.svg", neurix.FaviconPath);
            Assert.Equal("/images/neurix-logo.png", neurix.LogoPath);

            Assert.Equal(4, services.Count);
            Assert.Contains(services, s => s.Slug == "research-and-development");
            Assert.Contains(services, s => s.Slug == "software-implementation");
            Assert.Equal(4, socialLinks.Count);
        }

        [Fact]
        public async Task SeedAsync_IsIdempotent_DoesNotDuplicateOnMultipleRuns()
        {
            await _seeder.SeedAsync();
            var firstRunCompanyCount = await _db.CompanyProfiles.CountAsync();
            var firstRunServiceCount = await _db.Services.CountAsync();

            // Run second time
            await _seeder.SeedAsync();
            var secondRunCompanyCount = await _db.CompanyProfiles.CountAsync();
            var secondRunServiceCount = await _db.Services.CountAsync();

            Assert.Equal(firstRunCompanyCount, secondRunCompanyCount);
            Assert.Equal(firstRunServiceCount, secondRunServiceCount);
        }

        [Fact]
        public async Task SeedAsync_ReplacesLegacyContactEmailWithoutChangingCustomEmail()
        {
            await _seeder.SeedAsync();
            var cta = await _db.CtaSections.SingleAsync();
            Assert.Equal("contact@neurix.ai", cta.ContactEmail);

            cta.ContactEmail = "neurix@aidaleel.com";
            await _db.SaveChangesAsync();
            await _seeder.SeedAsync();
            Assert.Equal("contact@neurix.ai", cta.ContactEmail);

            cta.ContactEmail = "custom@example.test";
            await _db.SaveChangesAsync();
            await _seeder.SeedAsync();
            Assert.Equal("custom@example.test", cta.ContactEmail);
        }

        [Fact]
        public async Task SeedAsync_SeedsTheHomepageBlocksThatUsedToBeHardcoded()
        {
            await _seeder.SeedAsync();

            var neurixId = (await _db.CompanyProfiles.SingleAsync(c => c.Slug == "neurix")).Id;

            // #divisions — header plus one card per subsidiary, each pointing somewhere.
            var divisionsSection = await _db.DivisionsSections.SingleAsync(d => d.CompanyProfileId == neurixId);
            Assert.Equal("Our Ecosystem", divisionsSection.BadgeEn);
            Assert.False(string.IsNullOrWhiteSpace(divisionsSection.BadgeAr));

            var divisionItems = await _db.DivisionItems
                .Where(d => d.CompanyProfileId == neurixId)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            Assert.Equal(5, divisionItems.Count);
            Assert.Equal(
                new[] { "Neurix AI Labs", "Neurix AI Technology", "Neurix AI Club", "Neurix AI Plus", "Neurix AI HQ" },
                divisionItems.Select(d => d.TitleEn).ToArray());
            Assert.All(divisionItems, d => Assert.False(string.IsNullOrWhiteSpace(d.TitleAr)));
            Assert.All(divisionItems, d => Assert.StartsWith("/Home/", d.LinkUrl));

            // #services — header plus the four core service cards.
            var aiSection = await _db.AiEngineeringSections.SingleAsync(a => a.CompanyProfileId == neurixId);
            Assert.Equal("AI & Engineering", aiSection.TitleEn);
            Assert.False(string.IsNullOrWhiteSpace(aiSection.TitleAr));

            var aiItems = await _db.AiEngineeringItems
                .Where(a => a.CompanyProfileId == neurixId)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            Assert.Equal(4, aiItems.Count);
            Assert.All(aiItems, a => Assert.False(string.IsNullOrWhiteSpace(a.TitleAr)));
            Assert.All(aiItems, a => Assert.False(string.IsNullOrWhiteSpace(a.DescriptionAr)));

            // #portfolio / #insights / #testimonials — one header row per band.
            var headers = await _db.ListSectionHeaders
                .Where(h => h.CompanyProfileId == neurixId)
                .ToListAsync();

            Assert.Equal(3, headers.Count);
            Assert.Equal(
                CmsListSectionKeys.All.OrderBy(k => k).ToArray(),
                headers.Select(h => h.SectionKey).OrderBy(k => k).ToArray());
            Assert.All(headers, h => Assert.False(string.IsNullOrWhiteSpace(h.BadgeAr)));

            // #insights is the only band that renders a read time and a publish date.
            var insights = headers.Single(h => h.SectionKey == CmsListSectionKeys.Insights);
            Assert.Equal("min read", insights.ReadTimeSuffixEn);
            Assert.False(string.IsNullOrWhiteSpace(insights.ReadTimeSuffixAr));
            Assert.False(string.IsNullOrWhiteSpace(insights.UndatedLabelEn));
            Assert.False(string.IsNullOrWhiteSpace(insights.UndatedLabelAr));
            Assert.Null(headers.Single(h => h.SectionKey == CmsListSectionKeys.Portfolio).ReadTimeSuffixEn);

            // The two bands with a "view all" button carry both a label and a destination.
            foreach (var key in new[] { CmsListSectionKeys.Portfolio, CmsListSectionKeys.Insights })
            {
                var header = headers.Single(h => h.SectionKey == key);
                Assert.False(string.IsNullOrWhiteSpace(header.ButtonTextEn));
                Assert.False(string.IsNullOrWhiteSpace(header.ButtonTextAr));
                Assert.False(string.IsNullOrWhiteSpace(header.ButtonUrl));
                Assert.False(string.IsNullOrWhiteSpace(header.ItemLinkTextEn));
                Assert.False(string.IsNullOrWhiteSpace(header.ItemLinkTextAr));
                Assert.False(string.IsNullOrWhiteSpace(header.DefaultCategoryLabelEn));
                Assert.False(string.IsNullOrWhiteSpace(header.DefaultCategoryLabelAr));
            }
        }

        [Fact]
        public async Task SeedAsync_DoesNotDuplicateTheHomepageBlocksOnRerun()
        {
            await _seeder.SeedAsync();
            await _seeder.SeedAsync();

            Assert.Equal(1, await _db.DivisionsSections.CountAsync());
            Assert.Equal(5, await _db.DivisionItems.CountAsync());
            Assert.Equal(1, await _db.AiEngineeringSections.CountAsync());
            Assert.Equal(4, await _db.AiEngineeringItems.CountAsync());
            Assert.Equal(3, await _db.ListSectionHeaders.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_SeedsEveryBandOfTheSecondaryPages()
        {
            await _seeder.SeedAsync();

            var bands = await _db.PageBands.ToListAsync();
            var items = await _db.PageBandItems.ToListAsync();

            // Every page/band pair CmsPageKeys declares must have a row, or the dashboard
            // would show an editor for a band the seeder never filled.
            foreach (var page in CmsPageKeys.All)
            {
                foreach (var band in CmsPageKeys.BandsFor(page))
                {
                    Assert.Single(bands, b => b.PageKey == page && b.BandKey == band);
                }
            }

            Assert.All(bands, b => Assert.True(b.IsPublished));

            // Copy is bilingual wherever it is set at all.
            foreach (var band in bands)
            {
                if (!string.IsNullOrWhiteSpace(band.BadgeEn))
                {
                    Assert.False(string.IsNullOrWhiteSpace(band.BadgeAr));
                }
                if (!string.IsNullOrWhiteSpace(band.TitlePrefixEn))
                {
                    Assert.False(string.IsNullOrWhiteSpace(band.TitlePrefixAr));
                }
                if (!string.IsNullOrWhiteSpace(band.BodyEn))
                {
                    Assert.False(string.IsNullOrWhiteSpace(band.BodyAr));
                }
            }

            Assert.All(items, i => Assert.False(string.IsNullOrWhiteSpace(i.TitleEn)));
            Assert.All(items, i => Assert.False(string.IsNullOrWhiteSpace(i.TitleAr)));

            // Spot-check the shapes the views depend on.
            Assert.Equal(3, items.Count(i => i.PageKey == CmsPageKeys.Labs && i.BandKey == CmsPageKeys.Bands.Hero));
            Assert.Equal(4, items.Count(i => i.PageKey == CmsPageKeys.Labs && i.BandKey == CmsPageKeys.Bands.Cards));
            Assert.Equal(5, items.Count(i => i.PageKey == CmsPageKeys.Hq && i.BandKey == CmsPageKeys.Bands.Hero));
            Assert.Equal(6, items.Count(i => i.PageKey == CmsPageKeys.Hq && i.BandKey == CmsPageKeys.Bands.Cards));
            Assert.Equal(5, items.Count(i => i.PageKey == CmsPageKeys.About && i.BandKey == CmsPageKeys.Bands.Structure));

            // The Labs research-core band renders images rather than cards.
            var labsImages = items.Where(i => i.PageKey == CmsPageKeys.Labs && i.BandKey == CmsPageKeys.Bands.Overview);
            Assert.Equal(4, labsImages.Count());
            Assert.All(labsImages, i => Assert.StartsWith("/images/", i.ImagePath));

            // The AI page's two hero buttons are the only items that need a destination.
            var aiButtons = items.Where(i => i.PageKey == CmsPageKeys.Ai).ToList();
            Assert.Equal(2, aiButtons.Count);
            Assert.All(aiButtons, i => Assert.StartsWith("/Home/", i.LinkUrl));

            // About's subsidiary cards each link to the page they describe.
            var aboutCards = items.Where(i => i.PageKey == CmsPageKeys.About
                                           && i.BandKey == CmsPageKeys.Bands.Structure);
            Assert.All(aboutCards, i => Assert.StartsWith("/Home/", i.LinkUrl));

            // The two list pages are the only bands with an empty-list message.
            foreach (var page in new[] { CmsPageKeys.Insights, CmsPageKeys.Portfolio })
            {
                var header = bands.Single(b => b.PageKey == page && b.BandKey == CmsPageKeys.Bands.Hero);
                Assert.False(string.IsNullOrWhiteSpace(header.EmptyStateEn));
                Assert.False(string.IsNullOrWhiteSpace(header.EmptyStateAr));
                Assert.False(string.IsNullOrWhiteSpace(header.ItemLinkTextEn));
            }

            // Technology is the only page with a closing call to action.
            var cta = bands.Single(b => b.BandKey == CmsPageKeys.Bands.Cta);
            Assert.Equal(CmsPageKeys.Technology, cta.PageKey);
            Assert.False(string.IsNullOrWhiteSpace(cta.ButtonTextEn));
            Assert.StartsWith("/Home/", cta.ButtonUrl);
        }

        [Fact]
        public async Task SeedAsync_DoesNotDuplicateTheSecondaryPageBandsOnRerun()
        {
            await _seeder.SeedAsync();
            var bands = await _db.PageBands.CountAsync();
            var items = await _db.PageBandItems.CountAsync();

            await _seeder.SeedAsync();

            Assert.Equal(bands, await _db.PageBands.CountAsync());
            Assert.Equal(items, await _db.PageBandItems.CountAsync());
        }
    }
}
