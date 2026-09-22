using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Header copy for one horizontal band on one secondary public page (a division page,
    /// the AI capabilities page, or a list page). Keyed by <see cref="PageKey"/> +
    /// <see cref="BandKey"/> rather than modelled as one entity per band, because every band
    /// on every one of those pages has the same shape: an optional eyebrow badge, a headline
    /// split into up to three coloured runs, a lead paragraph, an optional image and an
    /// optional call to action. Adding a band therefore costs a seed row, not a migration.
    ///
    /// The cards, chips and images inside a band live in <see cref="CmsPageBandItem"/>.
    /// Distinct from <see cref="CmsDivisionPage"/>, which holds a division page's hero title,
    /// hero subtitle and mission statement only.
    /// </summary>
    public class CmsPageBand : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        /// <summary>Which public page this band belongs to. Values come from <c>CmsPageKeys</c>.</summary>
        public string PageKey { get; set; } = string.Empty;

        /// <summary>Which band on that page. Values come from <c>CmsPageKeys.Bands</c>.</summary>
        public string BandKey { get; set; } = string.Empty;

        /// <summary>Small pill above the headline, e.g. "Our Innovation Core".</summary>
        public string? BadgeEn { get; set; }
        public string? BadgeAr { get; set; }

        /// <summary>
        /// The headline is stored as up to three runs so the brand gradient can be applied to
        /// the middle one, matching the markup the pages already use
        /// ("Where governance, " + "operations, &amp; strategy" + " come together.").
        /// </summary>
        public string? TitlePrefixEn { get; set; }
        public string? TitlePrefixAr { get; set; }

        public string? TitleHighlightEn { get; set; }
        public string? TitleHighlightAr { get; set; }

        public string? TitleSuffixEn { get; set; }
        public string? TitleSuffixAr { get; set; }

        /// <summary>The lead paragraph under the headline.</summary>
        public string? BodyEn { get; set; }
        public string? BodyAr { get; set; }

        /// <summary>Single feature image for bands that show one, e.g. the HQ role diagram.</summary>
        public string? ImagePath { get; set; }

        public string? ButtonTextEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonUrl { get; set; }

        /// <summary>
        /// Label on the link inside each card of a list page, e.g. "Read Article".
        /// Only the <c>insights</c> and <c>portfolio</c> pages use it.
        /// </summary>
        public string? ItemLinkTextEn { get; set; }
        public string? ItemLinkTextAr { get; set; }

        /// <summary>
        /// Shown instead of the grid when a list page has nothing published yet.
        /// Only the <c>insights</c> and <c>portfolio</c> pages use it.
        /// </summary>
        public string? EmptyStateEn { get; set; }
        public string? EmptyStateAr { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
