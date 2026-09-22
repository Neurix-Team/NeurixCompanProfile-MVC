using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Enums;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Deal business operations. Same contract shape as the company, contact and
    /// lead services, so the Web layer consumes every CRM aggregate identically.
    /// </summary>
    public interface IDealService
    {
        Task<PagedResult<DealListRow>> GetListAsync(
            string? search,
            DealStage? stage,
            Guid? companyId,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize);

        Task<DealDetail?> GetDetailAsync(Guid id);

        Task<DealRequest?> GetForEditAsync(Guid id);

        Task<Guid> CreateAsync(DealRequest request, Guid currentUserId);

        Task<bool> UpdateAsync(Guid id, DealRequest request);

        Task<ServiceResult> SoftDeleteAsync(Guid id);

        /// <summary>
        /// Contacts for the deal form's dropdown. Returns every non-deleted contact
        /// with its company id, so the Web layer can narrow the list client-side
        /// once a company is picked without a second round trip.
        /// </summary>
        Task<IReadOnlyList<ContactOption>> GetContactOptionsAsync();
    }
}
