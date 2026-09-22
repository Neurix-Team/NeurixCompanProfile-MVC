using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Enums;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Contact business operations. Mirrors <see cref="ICompanyService"/>'s shape
    /// so every CRM aggregate is consumed the same way from the Web layer.
    /// </summary>
    public interface IContactService
    {
        Task<PagedResult<ContactListRow>> GetListAsync(
            string? search,
            RecordStatus? status,
            Guid? companyId,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize);

        Task<ContactDetail?> GetDetailAsync(Guid id);

        Task<ContactRequest?> GetForEditAsync(Guid id);

        Task<Guid> CreateAsync(ContactRequest request, Guid currentUserId);

        Task<bool> UpdateAsync(Guid id, ContactRequest request);

        Task<ServiceResult> SoftDeleteAsync(Guid id);
    }
}
