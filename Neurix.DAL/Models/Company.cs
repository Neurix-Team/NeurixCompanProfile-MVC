using Neurix.DAL.Enums;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// A customer organisation. Contacts belong to a company.
    /// </summary>
    public class Company : IAuditable
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

        public RecordStatus Status { get; set; } = RecordStatus.Active;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}
