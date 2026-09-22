using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsDivisionPageService
    {
        Task<IReadOnlyList<CmsDivisionPageDetailDto>> GetDivisionPagesByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false);
        Task<CmsDivisionPageDetailDto?> GetByIdAsync(Guid id);
        Task<CmsDivisionPageDetailDto?> GetBySlugAsync(string companySlug, string divisionSlug);
        Task<ServiceResult<Guid>> UpsertAsync(CmsDivisionPageUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null);
    }
}
