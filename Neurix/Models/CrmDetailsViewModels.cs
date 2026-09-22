using Neurix.DAL.Enums;

namespace Neurix.Models
{
    /// <summary>
    /// View models for the CRM detail pages. These exist so views bind to Web-layer
    /// types instead of EF entities — the BLL hands back a DTO, the controller maps
    /// it here, and the view never sees a tracked entity or a lazy navigation.
    /// </summary>
    public class CrmCompanyContactViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
    }

    public class CrmCompanyDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public RecordStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public IReadOnlyList<CrmCompanyContactViewModel> Contacts { get; set; } =
            Array.Empty<CrmCompanyContactViewModel>();
    }

    public class CrmContactDetailsViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        public RecordStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }

        public bool HasCompany => CompanyId.HasValue && !string.IsNullOrWhiteSpace(CompanyName);
    }

    public class CrmLeadDetailsViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public LeadSource Source { get; set; }
        public LeadStatus Status { get; set; }
        public string? InquiryType { get; set; }
        public string? Message { get; set; }
        public string? Notes { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public bool CapturedFromWebsiteForm { get; set; }

        public bool IsConverted { get; set; }
        public DateTime? ConvertedAtUtc { get; set; }
        public Guid? ConvertedToCompanyId { get; set; }
        public string? ConvertedToCompanyName { get; set; }
        public Guid? ConvertedToContactId { get; set; }
        public string? ConvertedToContactName { get; set; }
    }

    public class CrmDealDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DealStage Stage { get; set; }
        public string? Notes { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;

        public Guid? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }

        public bool HasContact => ContactId.HasValue && !string.IsNullOrWhiteSpace(ContactName);
    }
}
