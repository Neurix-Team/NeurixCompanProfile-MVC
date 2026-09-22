using Neurix.DAL.Enums;

namespace Neurix.BLL.Dtos
{
    /// <summary>Read model for the contact list. Projected in SQL.</summary>
    public class ContactListRow
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string? JobTitle { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public Guid? CompanyId { get; init; }
        public string? CompanyName { get; init; }
        public RecordStatus Status { get; init; }
    }

    /// <summary>Everything the contact detail page renders, including its company.</summary>
    public class ContactDetail
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string? JobTitle { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Notes { get; init; }
        public RecordStatus Status { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }

        public Guid? CompanyId { get; init; }
        public string? CompanyName { get; init; }
        public string? CompanyIndustry { get; init; }
        public string? CompanyEmail { get; init; }
        public string? CompanyPhone { get; init; }
    }

    /// <summary>Write model for creating or updating a contact.</summary>
    public class ContactRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        public Guid? CompanyId { get; set; }
        public RecordStatus Status { get; set; } = RecordStatus.Active;
    }
}
