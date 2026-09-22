using Neurix.BLL.Dtos.Cms;

namespace Neurix.Common
{
    /// <summary>
    /// Helpers the public views use to read <c>CmsPageBand</c> copy. Every method falls back to
    /// the view's own literal when the CMS value is missing, empty or the whole band is absent,
    /// which is what keeps a page looking untouched before it is seeded and if the CMS database
    /// is unreachable.
    /// </summary>
    public static class CmsBandText
    {
        /// <summary>The CMS value when it has content, otherwise the view's own copy.</summary>
        public static string Pick(string? cmsValue, string fallback) =>
            string.IsNullOrWhiteSpace(cmsValue) ? fallback : cmsValue!;

        /// <summary>
        /// One language of a bilingual pair, falling back to the other language before the
        /// view's literal — so a half-filled field never renders as blank on one side.
        /// </summary>
        public static string PickPair(string? preferred, string? other, string fallback) =>
            !string.IsNullOrWhiteSpace(preferred) ? preferred!
                : !string.IsNullOrWhiteSpace(other) ? other!
                : fallback;

        /// <summary>
        /// The bands handed to a public view by <c>HomeController</c>, or an empty set when the
        /// controller could not load them.
        /// </summary>
        public static CmsPageContentDto Bands(object? viewDataValue) =>
            viewDataValue as CmsPageContentDto ?? new CmsPageContentDto();
    }
}
