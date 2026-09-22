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
    /// Covers the bands of the secondary public pages — the five division pages, the AI
    /// capabilities page, the two list pages and About — which used to be hardcoded in their
    /// views and are now editable from /cms/page-sections.
    /// </summary>
    public class CmsPageBandServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsPageBandService _service;
        private readonly Guid _profileId;

        public CmsPageBandServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsPageBandService(_db, NullLogger<CmsPageBandService>.Instance);

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

        private CmsPageBandUpsertDto Band(string page, string band) => new()
        {
            CompanyProfileId = _profileId,
            PageKey = page,
            BandKey = band,
            BadgeEn = "Badge",
            BadgeAr = "شارة",
            IsPublished = true
        };

        private CmsPageBandItemUpsertDto Item(string page, string band, string title, int order = 0) => new()
        {
            CompanyProfileId = _profileId,
            PageKey = page,
            BandKey = band,
            IconName = "cpu",
            TitleEn = title,
            TitleAr = "عنصر",
            DescriptionEn = "Description",
            DescriptionAr = "وصف",
            DisplayOrder = order,
            IsPublished = true
        };

        // ── bands ─────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertBand_CreatesThenUpdatesInPlace()
        {
            var insert = await _service.UpsertBandAsync(Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero));
            Assert.True(insert.Success);

            var second = Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero);
            second.BadgeEn = "Changed";
            var update = await _service.UpsertBandAsync(second);
            Assert.True(update.Success);

            // One row, not two: the page/band pair identifies it.
            Assert.Equal(1, await _db.PageBands.CountAsync(
                b => b.PageKey == CmsPageKeys.Labs && b.BandKey == CmsPageKeys.Bands.Hero));

            var stored = await _service.GetBandByCompanyIdAsync(_profileId, CmsPageKeys.Labs, CmsPageKeys.Bands.Hero);
            Assert.Equal("Changed", stored!.BadgeEn);
        }

        [Theory]
        [InlineData(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero)]
        [InlineData(CmsPageKeys.Technology, CmsPageKeys.Bands.Cta)]
        [InlineData(CmsPageKeys.Hq, CmsPageKeys.Bands.Overview)]
        [InlineData(CmsPageKeys.Plus, CmsPageKeys.Bands.Cards)]
        [InlineData(CmsPageKeys.Club, CmsPageKeys.Bands.Overview)]
        [InlineData(CmsPageKeys.Ai, CmsPageKeys.Bands.Hero)]
        [InlineData(CmsPageKeys.Insights, CmsPageKeys.Bands.Hero)]
        [InlineData(CmsPageKeys.Portfolio, CmsPageKeys.Bands.Hero)]
        [InlineData(CmsPageKeys.About, CmsPageKeys.Bands.Principles)]
        public async Task UpsertBand_AcceptsEveryDeclaredBand(string page, string band)
        {
            var result = await _service.UpsertBandAsync(Band(page, band));
            Assert.True(result.Success, result.ErrorMessage);
        }

        [Fact]
        public async Task UpsertBand_RejectsABandThatIsNotOnThatPage()
        {
            // "cta" exists, but only on the Technology page.
            var result = await _service.UpsertBandAsync(Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Cta));

            Assert.False(result.Success);
            Assert.Empty(await _db.PageBands.ToListAsync());
        }

        [Fact]
        public async Task UpsertBand_RejectsAnUnknownPage()
        {
            var result = await _service.UpsertBandAsync(Band("careers", CmsPageKeys.Bands.Hero));

            Assert.False(result.Success);
            Assert.Empty(await _db.PageBands.ToListAsync());
        }

        [Fact]
        public async Task UpsertBand_NormalisesTheKeysItIsGiven()
        {
            var dto = Band("  LABS  ", "  Hero  ");
            Assert.True((await _service.UpsertBandAsync(dto)).Success);

            var stored = await _db.PageBands.SingleAsync();
            Assert.Equal("labs", stored.PageKey);
            Assert.Equal("hero", stored.BandKey);
        }

        [Fact]
        public async Task UpsertBand_ClearedFieldsBecomeNullSoTheViewFallsBack()
        {
            var dto = Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero);
            dto.BadgeEn = "   ";
            dto.BodyEn = string.Empty;

            Assert.True((await _service.UpsertBandAsync(dto)).Success);

            var stored = await _db.PageBands.SingleAsync();
            Assert.Null(stored.BadgeEn);
            Assert.Null(stored.BodyEn);
        }

        [Fact]
        public async Task GetPageContent_ReturnsOnlyThatPagesPublishedBands()
        {
            await _service.UpsertBandAsync(Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero));

            var hiddenCards = Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards);
            hiddenCards.IsPublished = false;
            await _service.UpsertBandAsync(hiddenCards);

            await _service.UpsertBandAsync(Band(CmsPageKeys.Hq, CmsPageKeys.Bands.Hero));

            var content = await _service.GetPageContentByCompanySlugAsync("neurix", CmsPageKeys.Labs);

            Assert.Single(content.Bands);
            Assert.Equal(CmsPageKeys.Bands.Hero, content.Bands[0].BandKey);
            Assert.Null(content.Band(CmsPageKeys.Bands.Cards));
        }

        [Fact]
        public async Task GetPageContent_IsEmptyRatherThanNullWhenNothingIsSeeded()
        {
            var content = await _service.GetPageContentByCompanySlugAsync("neurix", CmsPageKeys.Club);

            Assert.Empty(content.Bands);
            Assert.Empty(content.Items);
            Assert.Null(content.Band(CmsPageKeys.Bands.Hero));
            Assert.Empty(content.ItemsOf(CmsPageKeys.Bands.Hero));
        }

        [Fact]
        public async Task GetPageContent_IgnoresAnotherBrandsBands()
        {
            var otherId = Guid.NewGuid();
            _db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = otherId,
                Slug = "other",
                NameEn = "Other",
                NameAr = "أخرى",
                IsPublished = true
            });
            await _db.SaveChangesAsync();

            var other = Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Hero);
            other.CompanyProfileId = otherId;
            other.BadgeEn = "Other brand";
            await _service.UpsertBandAsync(other);

            var content = await _service.GetPageContentByCompanySlugAsync("neurix", CmsPageKeys.Labs);
            Assert.Empty(content.Bands);
        }

        // ── band items ────────────────────────────────────────────────────────────

        [Fact]
        public async Task BandItems_SupportTheFullCreateUpdateDeleteCycle()
        {
            var created = await _service.CreateBandItemAsync(
                Item(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards, "First"));
            Assert.True(created.Success);

            var update = await _service.UpdateBandItemAsync(created.Value, new CmsPageBandItemUpsertDto
            {
                CompanyProfileId = _profileId,
                PageKey = CmsPageKeys.Labs,
                BandKey = CmsPageKeys.Bands.Cards,
                IconName = "database",
                TitleEn = "Renamed",
                TitleAr = "معاد التسمية",
                DisplayOrder = 3,
                IsPublished = true
            });
            Assert.True(update.Success);

            var reloaded = await _service.GetBandItemByIdAsync(created.Value);
            Assert.Equal("Renamed", reloaded!.TitleEn);
            Assert.Equal("database", reloaded.IconName);
            Assert.Equal(3, reloaded.DisplayOrder);

            Assert.True((await _service.DeleteBandItemAsync(created.Value)).Success);

            // Soft delete: filtered out of reads, but the row survives for audit.
            Assert.Empty(await _service.GetBandItemsByCompanyIdAsync(
                _profileId, CmsPageKeys.Labs, CmsPageKeys.Bands.Cards));
            Assert.Equal(1, await _db.PageBandItems.IgnoreQueryFilters().CountAsync());
        }

        [Fact]
        public async Task BandItems_ComeBackInDisplayOrder()
        {
            await _service.CreateBandItemAsync(Item(CmsPageKeys.Hq, CmsPageKeys.Bands.Cards, "Third", order: 2));
            await _service.CreateBandItemAsync(Item(CmsPageKeys.Hq, CmsPageKeys.Bands.Cards, "First", order: 0));
            await _service.CreateBandItemAsync(Item(CmsPageKeys.Hq, CmsPageKeys.Bands.Cards, "Second", order: 1));

            var items = await _service.GetBandItemsByCompanyIdAsync(
                _profileId, CmsPageKeys.Hq, CmsPageKeys.Bands.Cards);

            Assert.Equal(new[] { "First", "Second", "Third" }, items.Select(i => i.TitleEn));
        }

        [Fact]
        public async Task BandItems_AreScopedToTheirOwnBand()
        {
            await _service.CreateBandItemAsync(Item(CmsPageKeys.Club, CmsPageKeys.Bands.Overview, "Persona"));
            await _service.CreateBandItemAsync(Item(CmsPageKeys.Club, CmsPageKeys.Bands.Cards, "Perk"));

            var personas = await _service.GetBandItemsByCompanyIdAsync(
                _profileId, CmsPageKeys.Club, CmsPageKeys.Bands.Overview);
            var perks = await _service.GetBandItemsByCompanyIdAsync(
                _profileId, CmsPageKeys.Club, CmsPageKeys.Bands.Cards);

            Assert.Equal("Persona", Assert.Single(personas).TitleEn);
            Assert.Equal("Perk", Assert.Single(perks).TitleEn);
        }

        [Fact]
        public async Task CreateBandItem_RejectsABandThatIsNotOnThatPage()
        {
            var result = await _service.CreateBandItemAsync(
                Item(CmsPageKeys.Insights, CmsPageKeys.Bands.Cards, "Nowhere"));

            Assert.False(result.Success);
            Assert.Empty(await _db.PageBandItems.ToListAsync());
        }

        [Fact]
        public async Task UpdateAndDeleteBandItem_FailOnAnIdThatIsGone()
        {
            var missing = Guid.NewGuid();

            var update = await _service.UpdateBandItemAsync(missing, Item(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards, "x"));
            Assert.False(update.Success);

            var delete = await _service.DeleteBandItemAsync(missing);
            Assert.False(delete.Success);
        }

        [Fact]
        public async Task GetPageContent_GroupsItemsByBandAndHidesUnpublishedOnes()
        {
            await _service.UpsertBandAsync(Band(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards));
            await _service.CreateBandItemAsync(Item(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards, "Visible"));

            var hidden = Item(CmsPageKeys.Labs, CmsPageKeys.Bands.Cards, "Hidden", order: 1);
            hidden.IsPublished = false;
            await _service.CreateBandItemAsync(hidden);

            var content = await _service.GetPageContentByCompanySlugAsync("neurix", CmsPageKeys.Labs);

            Assert.Equal("Visible", Assert.Single(content.ItemsOf(CmsPageKeys.Bands.Cards)).TitleEn);
        }

        // ── the key table itself ──────────────────────────────────────────────────

        [Fact]
        public void EveryPageDeclaresAtLeastOneBand()
        {
            foreach (var page in CmsPageKeys.All)
            {
                Assert.NotEmpty(CmsPageKeys.BandsFor(page));
            }
        }

        [Fact]
        public void BandsForIsEmptyRatherThanThrowingOnAnUnknownPage()
        {
            Assert.Empty(CmsPageKeys.BandsFor("careers"));
            Assert.Empty(CmsPageKeys.BandsFor(null));
            Assert.False(CmsPageKeys.IsKnown("careers", CmsPageKeys.Bands.Hero));
        }

        [Fact]
        public void DivisionPagesAreTheFiveThatAlsoHaveADivisionPageRow()
        {
            Assert.Equal(
                new[] { "labs", "technology", "hq", "plus", "club" },
                CmsPageKeys.DivisionPages);

            foreach (var page in CmsPageKeys.DivisionPages)
            {
                Assert.Contains(page, CmsPageKeys.All);
            }
        }
    }
}
