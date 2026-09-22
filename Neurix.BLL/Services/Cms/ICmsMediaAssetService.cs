using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsMediaAssetService
    {
        Task<IReadOnlyList<CmsMediaAssetDto>> GetAssetsByCompanyAsync(Guid? companyProfileId, string? category = null);
        Task<CmsMediaAssetDto?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<CmsMediaAssetUsageDto>> GetUsagesAsync(Guid assetId);
        Task<IReadOnlyDictionary<Guid, int>> GetUsageCountsAsync(Guid? companyProfileId, string? category = null);
        Task<ServiceResult<Guid>> CreateAsync(CmsMediaAssetCreateDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, bool force = false);
    }
}
