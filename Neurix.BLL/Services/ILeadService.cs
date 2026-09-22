using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Enums;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Lead business operations. Mirrors the company/contact service shape, plus
    /// the public-website capture path.
    /// </summary>
    public interface ILeadService
    {
        Task<PagedResult<LeadListRow>> GetListAsync(
            string? search,
            LeadStatus? status,
            Guid? assignedToUserId,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize,
            bool unassignedOnly = false);

        Task<LeadDetail?> GetDetailAsync(Guid id);

        Task<LeadRequest?> GetForEditAsync(Guid id);

        Task<Guid> CreateAsync(LeadRequest request, Guid currentUserId);

        /// <summary>
        /// Saves edits to a lead. Unlike the company/contact equivalents this returns
        /// an outcome rather than a bool, because a lead edit can now be refused for
        /// a business reason as well as simply not finding the record.
        /// </summary>
        Task<LeadUpdateOutcome> UpdateAsync(Guid id, LeadRequest request);

        Task<ServiceResult> SoftDeleteAsync(Guid id);

        Task<IReadOnlyList<AssignableUser>> GetAssignableUsersAsync();

        /// <summary>
        /// Captures an enquiry from the public marketing site. The BLL owns the
        /// rules that make it a lead: source is Website, status is New, and there
        /// is no creating user because the visitor is anonymous.
        /// </summary>
        Task<Guid> CaptureWebsiteEnquiryAsync(WebsiteEnquiry enquiry);
    }
}
