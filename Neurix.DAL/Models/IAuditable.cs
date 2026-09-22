namespace Neurix.DAL.Models
{
    /// <summary>
    /// Marker for entities whose timestamps are stamped automatically by
    /// <see cref="Data.CrmDbContext"/>. CreatedByUserId is deliberately not part of
    /// this contract — the DbContext has no access to the authenticated user, so
    /// the service layer sets it explicitly.
    /// </summary>
    public interface IAuditable
    {
        DateTime CreatedAtUtc { get; set; }
        DateTime? UpdatedAtUtc { get; set; }
    }
}
