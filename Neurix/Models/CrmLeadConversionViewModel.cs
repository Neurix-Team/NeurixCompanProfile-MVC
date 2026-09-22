using Microsoft.AspNetCore.Mvc.Rendering;

namespace Neurix.Models
{
    /// <summary>
    /// The convert-lead confirmation screen. Everything except
    /// <see cref="SelectedCompanyId"/> is display-only and repopulated by the
    /// controller — the form posts one decision, not a whole record.
    /// </summary>
    public class CrmLeadConversionViewModel
    {
        public Guid LeadId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }

        /// <summary>Null means "use the default": create a new company, or attach none.</summary>
        public Guid? SelectedCompanyId { get; set; }

        public string? ExistingContactName { get; set; }
        public bool HasExistingContact => !string.IsNullOrWhiteSpace(ExistingContactName);

        public bool HasCompanyName => !string.IsNullOrWhiteSpace(CompanyName);

        /// <summary>Populated by the controller; never bound from the request.</summary>
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
