using Neurix.DAL.Enums;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// A potential customer who has not been qualified into a Company/Contact yet.
    /// <see cref="CompanyName"/> is free text on purpose — matching it to a real
    /// <see cref="Company"/> record is what the future conversion step does.
    /// </summary>
    public class Lead : IAuditable
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? CompanyName { get; set; }
        public string? Phone { get; set; }

        public LeadSource Source { get; set; } = LeadSource.Website;
        public LeadStatus Status { get; set; } = LeadStatus.New;

        /// <summary>Mirrors the public contact form's inquiry type (Demo/Quote/Partnership/Support).</summary>
        public string? InquiryType { get; set; }

        /// <summary>The original enquiry text, as submitted.</summary>
        public string? Message { get; set; }

        /// <summary>Ongoing staff notes, kept separate from the original message.</summary>
        public string? Notes { get; set; }

        public Guid? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }

        /// <summary>
        /// Set once, when the lead is converted into a real Company/Contact pair.
        /// Recording the outcome (rather than just flipping <see cref="Status"/>)
        /// is what makes conversion idempotent — a second attempt can be refused
        /// instead of quietly creating duplicate records.
        /// </summary>
        public DateTime? ConvertedAtUtc { get; set; }

        public Guid? ConvertedToCompanyId { get; set; }
        public Company? ConvertedToCompany { get; set; }

        public Guid? ConvertedToContactId { get; set; }
        public Contact? ConvertedToContact { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>Null for leads captured from the public contact form (no signed-in user).</summary>
        public Guid? CreatedByUserId { get; set; }

        public bool IsDeleted { get; set; }
    }
}
