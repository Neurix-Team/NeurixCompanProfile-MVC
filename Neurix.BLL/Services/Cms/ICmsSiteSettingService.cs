using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsSiteSettingService
    {
        Task<IReadOnlyList<CmsSiteSettingDto>> GetSettingsByCompanyAsync(Guid? companyProfileId);
        Task<IReadOnlyList<CmsSiteSettingDto>> GetSettingsByCompanySlugAsync(string companySlug);
        Task<CmsSiteSettingDto?> GetSettingAsync(string companySlug, string key);
        Task<string?> GetSettingValueEnAsync(string companySlug, string key, string? defaultValue = null);
        Task<ServiceResult<Guid>> UpsertAsync(CmsSiteSettingUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SaveBatchAsync(Guid companyProfileId, IEnumerable<CmsSiteSettingUpsertDto> settings, Guid? userId = null);
        Task<ServiceResult<Guid>> CreateAsync(CmsSiteSettingUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null);
    }
}
