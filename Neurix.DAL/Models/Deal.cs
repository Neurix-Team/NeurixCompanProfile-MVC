using Neurix.DAL.Enums;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// A sales opportunity with a company. Unlike <see cref="Contact"/>, whose
    /// company link is optional, a deal must belong to one — a pipeline value that
    /// is not attributable to a customer is not useful.
    /// </summary>
    public class Deal : IAuditable
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Required. Deleting the company is blocked while deals remain.</summary>
        public Guid CompanyId { get; set; }
        public Company? Company { get; set; }

        /// <summary>The person being dealt with, when known.</summary>
        public Guid? ContactId { get; set; }
        public Contact? Contact { get; set; }

        public decimal Value { get; set; }

        public DealStage Stage { get; set; } = DealStage.New;

        public Guid? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Always set in practice — deals are only ever created by signed-in staff,
        /// with no anonymous capture path like <see cref="Lead"/> has. Nullable to
        /// keep the audit shape identical across every CRM entity.
        /// </summary>
        public Guid? CreatedByUserId { get; set; }

        public bool IsDeleted { get; set; }
    }
}
