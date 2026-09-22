using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsProjectService
    {
        Task<IReadOnlyList<CmsProjectSummaryDto>> GetProjectsByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsProjectSummaryDto>> GetPublishedProjectsByCompanySlugAsync(string companySlug, int? limit = null);
        Task<IReadOnlyList<CmsProjectSummaryDto>> GetFeaturedProjectsByCompanySlugAsync(string companySlug, int? limit = null);
        Task<CmsProjectDetailDto?> GetByIdAsync(Guid id);
        Task<CmsProjectDetailDto?> GetBySlugAsync(string companySlug, string projectSlug);
        Task<ServiceResult<Guid>> CreateAsync(CmsProjectUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdateAsync(Guid id, CmsProjectUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null);
    }
}
