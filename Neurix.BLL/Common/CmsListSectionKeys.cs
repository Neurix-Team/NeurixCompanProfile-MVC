namespace Neurix.BLL.Common
{
    /// <summary>
    /// Section keys for the homepage list bands whose header copy is CMS-editable
    /// (see <c>CmsListSectionHeader</c>). One row per key per company profile.
    /// </summary>
    public static class CmsListSectionKeys
    {
        public const string Portfolio = "portfolio";
        public const string Insights = "insights";
        public const string Testimonials = "testimonials";

        public static readonly string[] All = { Portfolio, Insights, Testimonials };
    }
}
