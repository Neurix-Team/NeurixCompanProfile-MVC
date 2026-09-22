using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    public class CrmLeadListItemViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public LeadStatus Status { get; set; }
        public LeadSource Source { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CrmLeadListViewModel
    {
        public IReadOnlyList<CrmLeadListItemViewModel> Items { get; set; } = Array.Empty<CrmLeadListItemViewModel>();
        public CrmPaginationViewModel Pagination { get; set; } = new();

        public string? Search { get; set; }
        public LeadStatus? Status { get; set; }
        public Guid? AssignedToUserId { get; set; }

        /// <summary>True when the reception-queue filter (no owner yet) is active.</summary>
        public bool UnassignedOnly { get; set; }

        public IEnumerable<SelectListItem> AssignedToOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
