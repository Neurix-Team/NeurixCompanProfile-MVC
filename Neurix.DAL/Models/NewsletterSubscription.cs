using Neurix.DAL.Enums;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// A public newsletter subscription captured from the footer form. Deliberately
    /// minimal: no password, no profile — just the email plus lifecycle flags so a
    /// subscription can be suspended/reactivated without losing its history.
    /// </summary>
    public class NewsletterSubscription : IAuditable
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        /// <summary>False after an unsubscribe; the row is kept rather than deleted so the opt-out sticks.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Soft delete for administrative cleanup; the unique index spans it, so a resubscribe reactivates the row.</summary>
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
