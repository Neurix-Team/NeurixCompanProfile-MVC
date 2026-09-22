using Neurix.DAL.Enums;

namespace Neurix.BLL.Dtos
{
    /// <summary>Read model for the lead list. Projected in SQL.</summary>
    public class LeadListRow
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? CompanyName { get; init; }
        public string? Phone { get; init; }
        public LeadStatus Status { get; init; }
        public LeadSource Source { get; init; }
        public string? AssignedToUserName { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }

    /// <summary>Everything the lead detail page renders.</summary>
    public class LeadDetail
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? CompanyName { get; init; }
        public string? Phone { get; init; }
        public LeadSource Source { get; init; }
        public LeadStatus Status { get; init; }
        public string? InquiryType { get; init; }
        public string? Message { get; init; }
        public string? Notes { get; init; }
        public Guid? AssignedToUserId { get; init; }
        public string? AssignedToUserName { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }

        /// <summary>True when the lead came from the public website form (no signed-in creator).</summary>
        public bool CapturedFromWebsiteForm { get; init; }

        public DateTime? ConvertedAtUtc { get; init; }
        public Guid? ConvertedToCompanyId { get; init; }
        public string? ConvertedToCompanyName { get; init; }
        public Guid? ConvertedToContactId { get; init; }
        public string? ConvertedToContactName { get; init; }

        public bool IsConverted => Status == LeadStatus.Converted;
    }

    /// <summary>
    /// How an attempted lead update ended. A plain bool cannot say the difference
    /// between "no such lead" (404) and "that status change is not allowed here"
    /// (a validation error on the form), and both need distinct handling.
    /// </summary>
    public enum LeadUpdateOutcome
    {
        Updated,
        NotFound,

        /// <summary>Staff tried to pick Converted by hand instead of using the Convert action.</summary>
        ConversionNotAllowedHere
    }

    /// <summary>Write model for creating or updating a lead from inside the CRM.</summary>
    public class LeadRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public LeadSource Source { get; set; } = LeadSource.Website;
        public LeadStatus Status { get; set; } = LeadStatus.New;
        public string? InquiryType { get; set; }
        public string? Message { get; set; }
        public string? Notes { get; set; }
        public Guid? AssignedToUserId { get; set; }
    }

    /// <summary>
    /// An enquiry submitted from the public marketing site. Deliberately separate
    /// from <see cref="LeadRequest"/>: the visitor supplies no status, source, or
    /// assignee — the BLL decides those.
    /// </summary>
    public class WebsiteEnquiry
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? InquiryType { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>A CRM user who can own a lead.</summary>
    public class AssignableUser
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
    }
}
