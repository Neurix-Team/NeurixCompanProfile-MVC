using System;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// One subsidiary card in the homepage "Our Ecosystem" band (#divisions).
    /// Distinct from <see cref="CmsDivisionPage"/>, which is the destination page's own content:
    /// this is only the card that links to it.
    /// </summary>
    public class CmsDivisionItem : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string IconName { get; set; } = "layers";

        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        /// <summary>Where the card links to, e.g. "/Home/Labs".</summary>
        public string LinkUrl { get; set; } = "/";

        public string LinkTextEn { get; set; } = "Explore";
        public string LinkTextAr { get; set; } = "المزيد";

        public int DisplayOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
