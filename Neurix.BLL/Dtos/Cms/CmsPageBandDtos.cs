using System;

namespace Neurix.BLL.Dtos.Cms
{
    /// <summary>Read model for one band on a secondary public page.</summary>
    public class CmsPageBandDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string PageKey { get; set; } = string.Empty;
        public string BandKey { get; set; } = string.Empty;

        public string? BadgeEn { get; set; }
        public string? BadgeAr { get; set; }

        public string? TitlePrefixEn { get; set; }
        public string? TitlePrefixAr { get; set; }
        public string? TitleHighlightEn { get; set; }
        public string? TitleHighlightAr { get; set; }
        public string? TitleSuffixEn { get; set; }
        public string? TitleSuffixAr { get; set; }

        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        public string? ImagePath { get; set; }

        public string? ButtonTextEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonUrl { get; set; }

        public string? ItemLinkTextEn { get; set; }
        public string? ItemLinkTextAr { get; set; }

        public string? EmptyStateEn { get; set; }
        public string? EmptyStateAr { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
    }

    /// <summary>Write model for one band. <see cref="PageKey"/> + <see cref="BandKey"/> identify the row.</summary>
    public class CmsPageBandUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string PageKey { get; set; } = string.Empty;
        public string BandKey { get; set; } = string.Empty;

        public string? BadgeEn { get; set; }
        public string? BadgeAr { get; set; }

        public string? TitlePrefixEn { get; set; }
        public string? TitlePrefixAr { get; set; }
        public string? TitleHighlightEn { get; set; }
        public string? TitleHighlightAr { get; set; }
        public string? TitleSuffixEn { get; set; }
        public string? TitleSuffixAr { get; set; }

        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        public string? ImagePath { get; set; }

        public string? ButtonTextEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonUrl { get; set; }

        public string? ItemLinkTextEn { get; set; }
        public string? ItemLinkTextAr { get; set; }

        public string? EmptyStateEn { get; set; }
        public string? EmptyStateAr { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    /// <summary>Read model for one card, chip, image or button inside a band.</summary>
    public class CmsPageBandItemDto
    {
        public Guid Id { get; set; }
        public Guid CompanyProfileId { get; set; }

        public string PageKey { get; set; } = string.Empty;
        public string BandKey { get; set; } = string.Empty;

        public string? IconName { get; set; }

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? ImagePath { get; set; }

        public string? LinkUrl { get; set; }
        public string? LinkTextEn { get; set; }
        public string? LinkTextAr { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
    }

    /// <summary>Write model for one band item.</summary>
    public class CmsPageBandItemUpsertDto
    {
        public Guid CompanyProfileId { get; set; }

        public string PageKey { get; set; } = string.Empty;
        public string BandKey { get; set; } = string.Empty;

        public string? IconName { get; set; }

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? ImagePath { get; set; }

        public string? LinkUrl { get; set; }
        public string? LinkTextEn { get; set; }
        public string? LinkTextAr { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    /// <summary>
    /// A whole page's CMS-editable bands with their items already grouped, so a public page
    /// costs one service call instead of one per band.
    /// </summary>
    public class CmsPageContentDto
    {
        public string PageKey { get; set; } = string.Empty;

        public IReadOnlyList<CmsPageBandDto> Bands { get; set; } = Array.Empty<CmsPageBandDto>();

        public IReadOnlyList<CmsPageBandItemDto> Items { get; set; } = Array.Empty<CmsPageBandItemDto>();

        /// <summary>The band with <paramref name="bandKey"/>, or null when no row exists yet.</summary>
        public CmsPageBandDto? Band(string bandKey) =>
            Bands.FirstOrDefault(b => b.BandKey == bandKey);

        /// <summary>Published items of <paramref name="bandKey"/> in display order.</summary>
        public IReadOnlyList<CmsPageBandItemDto> ItemsOf(string bandKey) =>
            Items.Where(i => i.BandKey == bandKey).ToList();
    }
}
