using System;

namespace Neurix.DAL.Models
{
    public class CmsCtaSection : IAuditable
    {
        public Guid Id { get; set; }

        public Guid CompanyProfileId { get; set; }
        public CmsCompanyProfile? CompanyProfile { get; set; }

        public string BadgeEn { get; set; } = "Let's Build Together";
        public string BadgeAr { get; set; } = "لنبني معاً";

        public string TitlePrefixEn { get; set; } = "Ready to engineer";
        public string TitleHighlightEn { get; set; } = " your future?";
        public string TitlePrefixAr { get; set; } = "مستعد لهندسة";
        public string TitleHighlightAr { get; set; } = " مستقبلك؟";

        public string ButtonTextEn { get; set; } = "Contact Us";
        public string ButtonTextAr { get; set; } = "تواصل معنا";
        public string ButtonUrl { get; set; } = "/Home/Contact";

        public string ContactEmail { get; set; } = "neurix@aidaleel.com";
        public string? BackgroundImagePath { get; set; } = "/images/Contact Us (Home) 2.png";

        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
