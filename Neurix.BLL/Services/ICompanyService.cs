using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Enums;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Company business operations. Every member takes and returns BLL DTOs —
    /// EF entities never cross out of this layer.
    /// </summary>
    public interface ICompanyService
    {
        Task<PagedResult<CompanyListRow>> GetListAsync(
            string? search,
            RecordStatus? status,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize);

        Task<CompanyDetail?> GetDetailAsync(Guid id);

        /// <summary>The company's editable fields, for populating an edit form.</summary>
        Task<CompanyRequest?> GetForEditAsync(Guid id);

        Task<Guid> CreateAsync(CompanyRequest request, Guid currentUserId);

        Task<bool> UpdateAsync(Guid id, CompanyRequest request);

        Task<ServiceResult> SoftDeleteAsync(Guid id);

        Task<IReadOnlyList<CompanyOption>> GetOptionsAsync();
    }
}
