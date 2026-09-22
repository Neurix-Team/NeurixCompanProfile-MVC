using Neurix.DAL.Enums;

namespace Neurix.BLL.Dtos
{
    /// <summary>Read model for the company list. Projected in SQL.</summary>
    public class CompanyListRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Industry { get; init; }
        public string? City { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public RecordStatus Status { get; init; }
        public int ContactCount { get; init; }
    }

    /// <summary>A contact as it appears on a company's detail page.</summary>
    public class CompanyContactSummary
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string? JobTitle { get; init; }
        public string? Email { get; init; }
    }

    /// <summary>Everything the company detail page renders.</summary>
    public class CompanyDetail
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Industry { get; init; }
        public string? Website { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? Country { get; init; }
        public string? Description { get; init; }
        public RecordStatus Status { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }

        public IReadOnlyList<CompanyContactSummary> Contacts { get; init; } =
            Array.Empty<CompanyContactSummary>();
    }

    /// <summary>A company as it appears in a dropdown.</summary>
    public class CompanyOption
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// Write model for creating or updating a company. The Web layer fills this in
    /// from its form view model; the service maps it onto the entity, so the entity
    /// itself never leaves the BLL.
    /// </summary>
    public class CompanyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public RecordStatus Status { get; set; } = RecordStatus.Active;
    }
}
