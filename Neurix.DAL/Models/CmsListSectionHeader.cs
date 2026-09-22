using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// Editable header copy for the homepage bands whose *items* already come from the CMS
    /// (#portfolio, #insights, #testimonials) but whose badge, headline, intro line and
    /// "view all" button label used to be hardcoded in the view.
    ///
    /// One keyed table rather than three near-identical section entities: the shape is the
    /// same for every band and new bands then cost a seed row instead of a migration.
    /// </summary>
    public class CmsListSectionHeader : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        /// <summary>One of <c>Neurix.BLL.Common.CmsListSectionKeys</c>: portfolio, insights, testimonials.</summary>
        public string SectionKey { get; set; } = string.Empty;

        public string BadgeEn { get; set; } = string.Empty;
        public string BadgeAr { get; set; } = string.Empty;

        public string TitlePrefixEn { get; set; } = string.Empty;
        public string TitleHighlightEn { get; set; } = string.Empty;
        public string TitlePrefixAr { get; set; } = string.Empty;
        public string TitleHighlightAr { get; set; } = string.Empty;

        /// <summary>Optional intro line under the headline. Only #testimonials renders one today.</summary>
        public string? SubtitleEn { get; set; }
        public string? SubtitleAr { get; set; }

        /// <summary>
        /// Optional label on each card's own link — "View Case Study" on #portfolio,
        /// "Read Article" on #insights. Not every band renders one.
        /// </summary>
        public string? ItemLinkTextEn { get; set; }
        public string? ItemLinkTextAr { get; set; }

        /// <summary>
        /// Shown on a card's category pill when the underlying record has no category of its
        /// own — "AI System" on #portfolio, "Research" on #insights. Blank hides the pill.
        /// </summary>
        public string? DefaultCategoryLabelEn { get; set; }
        public string? DefaultCategoryLabelAr { get; set; }

        /// <summary>Unit that follows the read-time number, e.g. "min read". #insights only.</summary>
        public string? ReadTimeSuffixEn { get; set; }
        public string? ReadTimeSuffixAr { get; set; }

        /// <summary>Stands in for the publish date when a post has none. #insights only.</summary>
        public string? UndatedLabelEn { get; set; }
        public string? UndatedLabelAr { get; set; }

        /// <summary>Optional "view all" button. Left blank the button is not rendered.</summary>
        public string? ButtonTextEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonUrl { get; set; }

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
