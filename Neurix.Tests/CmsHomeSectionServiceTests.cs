using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    public class CmsHomeSectionServiceTests : IDisposable
    {
        private readonly CmsDbContext _db;
        private readonly CmsHomeSectionService _service;
        private readonly Guid _profileId;

        public CmsHomeSectionServiceTests()
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

        [Fact]
        public async Task UpsertHeroSection_CreatesAndUpdatesHeroCorrectly()
        {
            // Arrange & Act (Insert)
            var insertDto = new CmsHeroSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Empowering Next-Gen AI",
                BadgeAr = "تمكين الذكاء المستقبلي",
                TitlePrefixEn = "Building ",
                TitleHighlightEn = "Intelligent Systems",
                TitleSuffixEn = " for Global Scale",
                TitlePrefixAr = "بناء ",
                TitleHighlightAr = "أنظمة ذكية",
                TitleSuffixAr = " على مستوى عالمي",
                SubtitleEn = "Dynamic Subtitle EN",
                SubtitleAr = "عنوان فرعي ديناميكي عربي",
                PrimaryButtonTextEn = "Explore All",
                PrimaryButtonTextAr = "استكشف الكل",
                PrimaryButtonUrl = "#divisions",
                SecondaryButtonTextEn = "Contact Us",
                SecondaryButtonTextAr = "تواصل معنا",
                SecondaryButtonUrl = "/Home/Contact",
                Stat1Value = "15+",
                Stat1LabelEn = "AI Models",
                Stat1LabelAr = "نماذج ذكاء",
                Stat2Value = "5+",
                Stat2LabelEn = "Enterprises",
                Stat2LabelAr = "مؤسسات",
                Stat3Value = "10+",
                Stat3LabelEn = "Platforms",
                Stat3LabelAr = "منصات",
                IsPublished = true
            };

            var result = await _service.UpsertHeroSectionAsync(insertDto);
            Assert.True(result.Success);

            // Assert
            var hero = await _service.GetHeroSectionByCompanySlugAsync("neurix");
            Assert.NotNull(hero);
            Assert.Equal("Empowering Next-Gen AI", hero.BadgeEn);
            Assert.Equal("تمكين الذكاء المستقبلي", hero.BadgeAr);
            Assert.Equal("15+", hero.Stat1Value);
            Assert.Equal("نماذج ذكاء", hero.Stat1LabelAr);

            // Act (Update)
            insertDto.Stat1Value = "20+";
            insertDto.BadgeEn = "Updated Badge";
            var updateResult = await _service.UpsertHeroSectionAsync(insertDto);
            Assert.True(updateResult.Success);

            // Assert updated
            var updated = await _service.GetHeroSectionByCompanySlugAsync("neurix");
            Assert.NotNull(updated);
            Assert.Equal("20+", updated.Stat1Value);
            Assert.Equal("Updated Badge", updated.BadgeEn);
        }

        [Fact]
        public async Task UpsertHumanVisionSection_CreatesAndUpdatesVisionCorrectly()
        {
            var dto = new CmsHumanVisionSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Our Human-First Philosophy",
                BadgeAr = "فلسفتنا الإنسانية الرائدة",
                TitlePrefixEn = "Neurix AI ",
                TitleHighlightEn = "Human Vision",
                TitlePrefixAr = "رؤية نيوركس AI ",
                TitleHighlightAr = "الإنسانية",
                Paragraph1En = "Paragraph 1 English Content",
                Paragraph1Ar = "الفقرة الأولى باللغة العربية",
                Paragraph2En = "Paragraph 2 English Content",
                Paragraph2Ar = "الفقرة الثانية باللغة العربية",
                ImagePath = "/uploads/cms/homepage/test_vision.png",
                ImageAltEn = "Vision Alt EN",
                ImageAltAr = "النص البديل عربي",
                IsPublished = true
            };

            var result = await _service.UpsertHumanVisionSectionAsync(dto);
            Assert.True(result.Success);

            var vision = await _service.GetHumanVisionSectionByCompanySlugAsync("neurix");
            Assert.NotNull(vision);
            Assert.Equal("Our Human-First Philosophy", vision.BadgeEn);
            Assert.Equal("فلسفتنا الإنسانية الرائدة", vision.BadgeAr);
            Assert.Equal("/uploads/cms/homepage/test_vision.png", vision.ImagePath);
            Assert.Equal("Paragraph 1 English Content", vision.Paragraph1En);
            Assert.Equal("الفقرة الأولى باللغة العربية", vision.Paragraph1Ar);
        }

        [Fact]
        public async Task UpsertPioneersSection_CreatesAndUpdatesPioneersCorrectly()
        {
            var dto = new CmsPioneersSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "To Future Builders",
                BadgeAr = "إلى بناة المستقبل",
                TitlePrefixEn = "A Message to the ",
                TitleHighlightEn = "Pioneers of Innovation",
                TitlePrefixAr = "رسالة ",
                TitleHighlightAr = "لرواد الابتكار والأبداع",
                Paragraph1En = "Pioneers Paragraph 1",
                Paragraph1Ar = "فقرة رواد الابتكار 1",
                Paragraph2En = "Pioneers Paragraph 2",
                Paragraph2Ar = "فقرة رواد الابتكار 2",
                ImagePath = "/uploads/cms/homepage/test_pioneers.png",
                ImageAltEn = "Pioneers Alt EN",
                ImageAltAr = "النص البديل للرواد",
                IsPublished = true
            };

            var result = await _service.UpsertPioneersSectionAsync(dto);
            Assert.True(result.Success);

            var pioneers = await _service.GetPioneersSectionByCompanySlugAsync("neurix");
            Assert.NotNull(pioneers);
            Assert.Equal("To Future Builders", pioneers.BadgeEn);
            Assert.Equal("إلى بناة المستقبل", pioneers.BadgeAr);
            Assert.Equal("/uploads/cms/homepage/test_pioneers.png", pioneers.ImagePath);
            Assert.Equal("Pioneers Paragraph 1", pioneers.Paragraph1En);
            Assert.Equal("فقرة رواد الابتكار 1", pioneers.Paragraph1Ar);
        }
    }
}
