namespace Neurix.BLL.Dtos
{
    /// <summary>
    /// Everything the "convert this lead" confirmation screen needs: the lead as it
    /// stands, what the service would do by default, and the choices staff can make.
    /// </summary>
    public class LeadConversionPreview
    {
        public Guid LeadId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Phone { get; init; }

        /// <summary>The free-text company name carried on the lead, if any.</summary>
        public string? CompanyName { get; init; }

        /// <summary>
        /// An existing company whose name matches <see cref="CompanyName"/> exactly.
        /// Pre-selected on the form, but always overridable.
        /// </summary>
        public Guid? SuggestedCompanyId { get; init; }

        /// <summary>
        /// An existing contact with the same email. When set, conversion reuses that
        /// contact instead of creating a second record for the same person.
        /// </summary>
        public Guid? ExistingContactId { get; init; }
        public string? ExistingContactName { get; init; }

        public IReadOnlyList<CompanyOption> Companies { get; init; } =
            Array.Empty<CompanyOption>();
    }

    /// <summary>What a completed conversion produced, so the caller can link to it.</summary>
    public class LeadConversionResult
    {
        public Guid? CompanyId { get; init; }
        public string? CompanyName { get; init; }
        public Guid ContactId { get; init; }
        public string ContactName { get; init; } = string.Empty;
    }
}
