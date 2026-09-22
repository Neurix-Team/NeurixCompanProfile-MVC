using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    /// <summary>Paging state plus the current filters, so page links keep them.</summary>
    public class CrmPaginationViewModel
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        /// <summary>Filter values to re-append to every page link.</summary>
        public Dictionary<string, string> RouteValues { get; set; } = new();
    }

    public class CrmCompanyListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int ContactCount { get; set; }
        public RecordStatus Status { get; set; }
    }

    public class CrmCompanyListViewModel
    {
        public IReadOnlyList<CrmCompanyListItemViewModel> Items { get; set; } = Array.Empty<CrmCompanyListItemViewModel>();
        public CrmPaginationViewModel Pagination { get; set; } = new();

        public string? Search { get; set; }
        public RecordStatus? Status { get; set; }
    }

    public class CrmContactListItemViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public RecordStatus Status { get; set; }
    }

    public class CrmContactListViewModel
    {
        public IReadOnlyList<CrmContactListItemViewModel> Items { get; set; } = Array.Empty<CrmContactListItemViewModel>();
        public CrmPaginationViewModel Pagination { get; set; } = new();

        public string? Search { get; set; }
        public RecordStatus? Status { get; set; }
        public Guid? CompanyId { get; set; }
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
