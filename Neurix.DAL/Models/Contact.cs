using System.ComponentModel.DataAnnotations.Schema;
using Neurix.DAL.Enums;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// A person at a customer organisation. Optionally linked to a <see cref="Company"/>.
    /// </summary>
    public class Contact : IAuditable
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }

        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }

        public RecordStatus Status { get; set; } = RecordStatus.Active;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
