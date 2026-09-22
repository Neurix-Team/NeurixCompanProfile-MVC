using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    public class CmsContentPageServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsContentPageService _service;
        private readonly Guid _profileId;

        public CmsContentPageServiceTests()
        {
            var options = new DbContextOptionsBuilder<CmsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CmsDbContext(options);
            _service = new CmsContentPageService(_db);

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

        private CmsContentPageUpsertDto NewDto(string slug = "about") => new()
        {
            CompanyProfileId = _profileId,
            Slug = slug,
            TitleEn = "About Neurix AI",
            TitleAr = "عن نيوركس AI",
            HeroTitlePrefixEn = "Engineering at the ",
            HeroTitleHighlightEn = "Point of Intersection",
            MissionTitleEn = "Our Mission",
            MissionTextEn = "Mission text.",
            IsPublished = true
        };

        [Fact]
        public async Task UpsertAsync_CreatesPage_WhenNoneExists()
        {
            var result = await _service.UpsertAsync(NewDto());

            Assert.True(result.Success);
            Assert.NotEqual(Guid.Empty, result.Value);

            var saved = await _db.ContentPages.SingleAsync();
            Assert.Equal("about", saved.Slug);
            Assert.Equal("About Neurix AI", saved.TitleEn);
        }

        [Fact]
        public async Task UpsertAsync_UpdatesInPlace_ForSameCompanyAndSlug()
        {
            var first = await _service.UpsertAsync(NewDto());

            var edit = NewDto();
            edit.TitleEn = "Changed Title";
            var second = await _service.UpsertAsync(edit);

            // Matching on (company, slug) rather than Id keeps the unique index intact.
            Assert.True(second.Success);
            Assert.Equal(first.Value, second.Value);
            Assert.Equal(1, await _db.ContentPages.CountAsync());
            Assert.Equal("Changed Title", (await _db.ContentPages.SingleAsync()).TitleEn);
        }

        [Fact]
        public async Task UpsertAsync_NormalisesSlugToLowercase()
        {
            var dto = NewDto("PRIVACY");
            var result = await _service.UpsertAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("privacy", (await _db.ContentPages.SingleAsync()).Slug);
        }

        [Fact]
        public async Task UpsertAsync_TurnsBlankOptionalFieldsIntoNull()
        {
            var dto = NewDto();
            dto.HeroSubtitleEn = "   ";
            dto.CtaTitleEn = "";

            await _service.UpsertAsync(dto);

            var saved = await _db.ContentPages.SingleAsync();
            // Null (not empty string) is what lets the views fall back to their defaults.
            Assert.Null(saved.HeroSubtitleEn);
            Assert.Null(saved.CtaTitleEn);
        }

        [Fact]
        public async Task UpsertAsync_Fails_WhenCompanyProfileMissing()
        {
            var dto = NewDto();
            dto.CompanyProfileId = Guid.NewGuid();

            var result = await _service.UpsertAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("company profile", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpsertAsync_Fails_WhenSlugBlank()
        {
            var dto = NewDto();
            dto.Slug = "   ";

            var result = await _service.UpsertAsync(dto);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsPublishedPage()
        {
            await _service.UpsertAsync(NewDto());

            var page = await _service.GetBySlugAsync("neurix", "about");

            Assert.NotNull(page);
            Assert.Equal("about", page!.Slug);
            Assert.Equal("Our Mission", page.MissionTitleEn);
        }

        [Fact]
        public async Task GetBySlugAsync_IsCaseInsensitive()
        {
            await _service.UpsertAsync(NewDto());

            Assert.NotNull(await _service.GetBySlugAsync("NEURIX", "About"));
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsNull_WhenPageUnpublished()
        {
            var dto = NewDto();
            dto.IsPublished = false;
            await _service.UpsertAsync(dto);

            // The public views fall back to their built-in copy when this is null.
            Assert.Null(await _service.GetBySlugAsync("neurix", "about"));
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsNull_WhenBrandProfileUnpublished()
        {
            await _service.UpsertAsync(NewDto());

            var profile = await _db.CompanyProfiles.SingleAsync();
            profile.IsPublished = false;
            await _db.SaveChangesAsync();

            Assert.Null(await _service.GetBySlugAsync("neurix", "about"));
        }

        [Fact]
        public async Task GetBySlugAsync_ReturnsNull_ForUnknownSlugOrBlankInput()
        {
            await _service.UpsertAsync(NewDto());

            Assert.Null(await _service.GetBySlugAsync("neurix", "does-not-exist"));
            Assert.Null(await _service.GetBySlugAsync("", "about"));
            Assert.Null(await _service.GetBySlugAsync("neurix", ""));
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ReturnsUnpublishedPagesToo()
        {
            await _service.UpsertAsync(NewDto("about"));

            var hidden = NewDto("privacy");
            hidden.IsPublished = false;
            await _service.UpsertAsync(hidden);

            var pages = await _service.GetAllByCompanyAsync(_profileId);

            Assert.Equal(2, pages.Count);
            Assert.Contains(pages, p => p.Slug == "privacy" && !p.IsPublished);
        }

        [Fact]
        public async Task GetAllByCompanyAsync_FlagsWhetherBodyContentExists()
        {
            var withBody = NewDto("privacy");
            withBody.BodyEn = "<p>Policy</p>";
            await _service.UpsertAsync(withBody);
            await _service.UpsertAsync(NewDto("about"));

            var pages = await _service.GetAllByCompanyAsync(_profileId);

            Assert.True(pages.Single(p => p.Slug == "privacy").HasBody);
            Assert.False(pages.Single(p => p.Slug == "about").HasBody);
        }

        [Fact]
        public async Task GetByIdAsync_RoundTripsAllContentBlocks()
        {
            var dto = NewDto();
            dto.BodyEn = "<p>Body</p>";
            dto.VisionTextAr = "نص الرؤية";
            dto.CtaButtonTextEn = "Start a Conversation";
            var created = await _service.UpsertAsync(dto);

            var page = await _service.GetByIdAsync(created.Value);

            Assert.NotNull(page);
            Assert.Equal("<p>Body</p>", page!.BodyEn);
            Assert.Equal("نص الرؤية", page.VisionTextAr);
            Assert.Equal("Start a Conversation", page.CtaButtonTextEn);
            Assert.Equal("Neurix AI", page.CompanyNameEn);
        }

        [Fact]
        public async Task UpsertAsync_StripsScriptTagsAndEventHandlersFromBody_ButKeepsSafeFormatting()
        {
            var dto = NewDto();
            dto.BodyEn = "<p>Safe text</p><script>alert('xss')</script><img src=x onerror=\"alert(1)\">";
            var created = await _service.UpsertAsync(dto);

            var page = await _service.GetByIdAsync(created.Value);

            Assert.NotNull(page);
            Assert.Contains("Safe text", page!.BodyEn);
            Assert.DoesNotContain("<script", page.BodyEn, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("onerror", page.BodyEn, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("alert(", page.BodyEn, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_ForUnknownId()
        {
            Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpsertAsync_Fails_WhenDtoNull()
        {
            var result = await _service.UpsertAsync(null!);
            Assert.False(result.Success);
        }
    }
}
