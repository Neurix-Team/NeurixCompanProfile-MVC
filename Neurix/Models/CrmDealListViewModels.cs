using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    public class CrmDealListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public decimal Value { get; set; }
        public DealStage Stage { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CrmDealListViewModel
    {
        public IReadOnlyList<CrmDealListItemViewModel> Items { get; set; } = Array.Empty<CrmDealListItemViewModel>();
        public CrmPaginationViewModel Pagination { get; set; } = new();

        public string? Search { get; set; }
        public DealStage? Stage { get; set; }
        public Guid? CompanyId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
