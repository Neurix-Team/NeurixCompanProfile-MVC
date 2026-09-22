using Neurix.DAL.Enums;

namespace Neurix.BLL.Dtos
{
    /// <summary>Read model for the deal list. Projected in SQL.</summary>
    public class DealListRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public Guid CompanyId { get; init; }
        public string CompanyName { get; init; } = string.Empty;
        public string? ContactName { get; init; }
        public decimal Value { get; init; }
        public DealStage Stage { get; init; }
        public string? AssignedToUserName { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }

    /// <summary>Everything the deal detail page renders.</summary>
    public class DealDetail
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Value { get; init; }
        public DealStage Stage { get; init; }
        public string? Notes { get; init; }
        public string? AssignedToUserName { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }

        public Guid CompanyId { get; init; }
        public string CompanyName { get; init; } = string.Empty;

        public Guid? ContactId { get; init; }
        public string? ContactName { get; init; }
        public string? ContactEmail { get; init; }
    }

    /// <summary>Write model for creating or updating a deal.</summary>
    public class DealRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public Guid? ContactId { get; set; }
        public decimal Value { get; set; }
        public DealStage Stage { get; set; } = DealStage.New;
        public Guid? AssignedToUserId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>A contact as it appears in a dropdown.</summary>
    public class ContactOption
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public Guid? CompanyId { get; init; }
    }
}
