using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsMenuItemService
    {
        Task<IReadOnlyList<CmsMenuItemSummaryDto>> GetMenuItemsByCompanySlugAsync(
            string slug,
            CmsMenuItemPlacement? placement = null,
            bool includeUnpublished = false);

        Task<IReadOnlyList<CmsMenuItemSummaryDto>> GetMenuItemsByCompanyIdAsync(
            Guid companyProfileId,
            CmsMenuItemPlacement? placement = null,
            bool includeUnpublished = true);

        Task<CmsMenuItemDetailDto?> GetMenuItemByIdAsync(Guid id);

        Task<ServiceResult<Guid>> CreateMenuItemAsync(CmsMenuItemUpsertDto dto, Guid? userId = null);

        Task<ServiceResult> UpdateMenuItemAsync(CmsMenuItemUpsertDto dto, Guid? userId = null);

        Task<ServiceResult> DeleteMenuItemAsync(Guid id, Guid? userId = null);

        Task<ServiceResult> ReorderMenuItemsAsync(IReadOnlyList<Guid> orderedIds, Guid? userId = null);
    }
}
